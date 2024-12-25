using Sandbox.ModAPI;
using System.Collections.Generic;
using System;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.ChatBot;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Game.ModAPI;
using VRage.Game;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Radiator
{
	public partial class HeatRadiatorLogic
	{
		public override void UpdateAfterSimulation()
		{
			Animate();
		}
		public override void UpdateAfterSimulation100()
		{
			if (block == null || radiatorData == null || !block.IsOwnedByAPlayer()) return;
			CheckIsExterior();

			if (!block.IsFunctional || !radiatorData.CanSeeSky)
			{
				radiatorData.CurrentDissipation = 0;
				return;
			}

			ticksSinceWeatherCheck = ticksSinceWeatherCheck >= 1000 ? 0 : ticksSinceWeatherCheck + 100;
			if (ticksSinceWeatherCheck == 0)
			{
				MyObjectBuilder_WeatherEffect currentWeatherEffect;
				var position = block.PositionComp.GetPosition();
				if (!MyAPIGateway.Session.WeatherEffects.GetWeather(position, out currentWeatherEffect) || !Configuration.Configuration.WeatherSettings.ContainsKey(currentWeatherEffect.Weather))
				{
					weatherMult = 1;
					return;
				}

				weatherMult = 1 / Configuration.Configuration.WeatherSettings[currentWeatherEffect.Weather];
			}

			if (!block.Enabled)
			{
				radiatorData.CurrentDissipation = Math.Max(0, (radiatorData.CurrentDissipation - radiatorData.StepSize));
			}
			else
			{
				var beacon = Utilities.GetHeatSinkLogic(block.CubeGrid);
				if (beacon == null)
					return;

				if (radiatorData.CurrentDissipation < 0)
					radiatorData.CurrentDissipation = 0;
				if (radiatorData.CurrentDissipation > radiatorData.MaxDissipation)
					radiatorData.CurrentDissipation = radiatorData.MaxDissipation;

				var dissipatedHeat = beacon.RemoveHeat(radiatorData.CurrentDissipation, weatherMult);
				if (dissipatedHeat < radiatorData.CurrentDissipation)
					radiatorData.CurrentDissipation -= radiatorData.StepSize;
				if (dissipatedHeat == radiatorData.CurrentDissipation)
					if (radiatorData.CurrentDissipation + radiatorData.StepSize < radiatorData.MaxDissipation)
						radiatorData.CurrentDissipation += radiatorData.StepSize;
					else
						radiatorData.CurrentDissipation = radiatorData.MaxDissipation;
			}

			Animate();
			block.RefreshCustomInfo();
		}

		private void CheckIsExterior()
		{
			if (block == null) return;
			Vector3D startPosition = block.GetPosition();
			Vector3D forwardRelativeToBlock = Vector3D.TransformNormal(radiatorData.ForwardDirection, block.WorldMatrix);
			Vector3D endPosition = startPosition + forwardRelativeToBlock * 100;

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
