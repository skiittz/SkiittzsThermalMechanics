using Sandbox.ModAPI;
using System.Collections.Generic;
using System;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.ChatBot;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core.DebuggingTools;
using VRage.Game.ModAPI;
using VRage.Game;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Radiator
{
	public partial class HeatRadiatorLogic
	{
		public override void UpdateAfterSimulation()
		{
			if (MyAPIGateway.Utilities == null || !MyAPIGateway.Utilities.IsDedicated)
				Animate();
		}
		public override void UpdateAfterSimulation100()
		{
			if (!ThermalAuthority.IsServer || block == null || radiatorData == null || block.CubeGrid?.Physics == null) return;
			CheckIsExterior();

			if (!block.IsFunctional || !radiatorData.CanSeeSky)
			{
				radiatorData.CurrentDissipation = 0;
				ThermalAuthority.Sync(this);
				return;
			}

			ticksSinceWeatherCheck = ticksSinceWeatherCheck >= 1000 ? 0 : ticksSinceWeatherCheck + 100;
			if (ticksSinceWeatherCheck == 0)
			{
				MyObjectBuilder_WeatherEffect currentWeatherEffect;
				var position = block.PositionComp.GetPosition();
				float naturalGravity;
				MyAPIGateway.Physics.CalculateNaturalGravityAt(position, out naturalGravity);
				if (naturalGravity < 0.0001)
				{
					dissipationMult = Configuration.Configuration.DissipationModifiers["Space"];
				}
				else if (MyAPIGateway.Session.WeatherEffects.GetWeather(position, out currentWeatherEffect) && Configuration.Configuration.DissipationModifiers.ContainsKey(currentWeatherEffect.Weather))
				{
					dissipationMult = Configuration.Configuration.DissipationModifiers[currentWeatherEffect.Weather];
				}
				else
				{
					dissipationMult = Configuration.Configuration.DissipationModifiers["Default"];
				}
			}

			if (Configuration.Configuration.DebugMode
			    && (MyAPIGateway.Utilities == null || !MyAPIGateway.Utilities.IsDedicated))
			{
				if (radiatorData.DebugMessages.Count >= 20)
					radiatorData.DebugMessages.Clear();
				radiatorData.DebugMessages.Add($"Ticks since weather check: {ticksSinceWeatherCheck}");
				radiatorData.DebugMessages.Add($"Weather Mult: {dissipationMult}");
			}

			if (!block.Enabled)
			{
				radiatorData.CurrentDissipation = Math.Max(0, (radiatorData.CurrentDissipation - radiatorData.StepSize));
			}
			else
			{
				var beacon = Utilities.GetHeatSinkLogic(block?.CubeGrid);
				if (beacon == null)
				{
					radiatorData.CurrentDissipation = 0;
					ThermalAuthority.Sync(this);
					return;
				}

				if (radiatorData.CurrentDissipation < 0)
					radiatorData.CurrentDissipation = 0;
				if (radiatorData.CurrentDissipation > radiatorData.MaxDissipation)
					radiatorData.CurrentDissipation = radiatorData.MaxDissipation;

				var attemptToDissipate = radiatorData.CurrentDissipation.LowerBoundedBy(radiatorData.StepSize) * dissipationMult;
				var dissipatedHeat = beacon.RemoveHeat(attemptToDissipate);
				if (Configuration.Configuration.DebugMode
				    && (MyAPIGateway.Utilities == null || !MyAPIGateway.Utilities.IsDedicated))
				{
					radiatorData.DebugMessages.Add($"Attempted to dissipate: {attemptToDissipate}");
					radiatorData.DebugMessages.Add($"Dissipated heat: {dissipatedHeat}");
				}
				if (dissipatedHeat < attemptToDissipate)
					radiatorData.CurrentDissipation = (radiatorData.CurrentDissipation - radiatorData.StepSize).LowerBoundedBy(0);
				else
					radiatorData.CurrentDissipation = (radiatorData.CurrentDissipation + radiatorData.StepSize).UpperBoundedBy(radiatorData.MaxDissipation);
			}

			if (MyAPIGateway.Utilities == null || !MyAPIGateway.Utilities.IsDedicated)
				Animate();
			ThermalAuthority.Sync(this);
			block.RefreshCustomInfo();
		}

		private void CheckIsExterior()
		{
			if (block == null) return;
			Vector3D startPosition = block.GetPosition();
			Vector3D forwardRelativeToBlock = Vector3D.TransformNormal(radiatorData.ForwardDirection, block.WorldMatrix);
			Vector3D endPosition = startPosition + forwardRelativeToBlock * (block.CubeGrid.GridSizeEnum == MyCubeSize.Small ? 10: 100);

			var hitList = new List<IHitInfo>();
			MyAPIGateway.Physics.CastRay(startPosition, endPosition, hitList);

			radiatorData.CanSeeSky = true;
			foreach (var result in hitList)
			{

				var grid = result.HitEntity as IMyCubeGrid ?? (result.HitEntity as IMyCubeBlock)?.CubeGrid;
				if (grid != null)
				{
					//check to see if the block/grid hit belongs to the same construct.  otherwise ignore.
					if (grid.EntityId == block.CubeGrid.EntityId)
						radiatorData.CanSeeSky = false;
				}
				else
				{
					//not a grid or block hit, must be voxel.  which means if we are a station, its a problem.  otherwise ignore.
					if (block.CubeGrid.IsStatic)
						radiatorData.CanSeeSky = false;
				}
			}

			if (!radiatorData.CanSeeSky)
				ChatBot.ChatBot.WarnPlayer(block, "This radiator will not function.  The radiating face must have line of sight to empty space.", MessageSeverity.Tutorial);
		}
	}
}
