using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.HeatSink
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon), false)]
	public partial class HeatSinkLogic : MyGameLogicComponent
	{
		private IMyBeacon block;
		public HeatSinkData HeatSinkData;
		private float signalMult = 1.0f;
		private int ticksSinceWeatherCheck = 0;
		private static bool _destroyHandlerRegistered = false;
		private bool _heatWasRedistributed;
		private bool hasAuthoritativeState;

		public override void Init(MyObjectBuilder_EntityBase objectBuilder)
		{
			block = (IMyBeacon)Entity;
			if (block == null)
				return;

			bool configFound;
			if (ThermalAuthority.IsServer)
				HeatSinkData = HeatSinkData.LoadData(block, out configFound);
			else
			{
				HeatSinkData = new HeatSinkData
				{
					HeatCapacity = 1f,
					OriginalGridId = block.CubeGrid.EntityId
				};
				configFound = true;
			}
			if (!configFound) return;
			hasAuthoritativeState = ThermalAuthority.IsServer;

			HeatSinkData.IsSmallGrid = block.CubeGrid.GridSizeEnum == MyCubeSize.Small;
			if (ThermalAuthority.IsServer)
			{
				bool shuntToParent;
				if (Configuration.Configuration.TryGetGeneralSettingValue("SmallGridShuntsToLarge", out shuntToParent))
					HeatSinkData.ShuntToParent = shuntToParent;
			}

			NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
			if (ThermalAuthority.IsServer)
				NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
			(Container.Entity as IMyTerminalBlock).AppendingCustomInfo += HeatSinkLogic_AppendingCustomInfo;

			if (ThermalAuthority.IsServer && !_destroyHandlerRegistered)
			{
				MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, OnBlockDestroyed);
				_destroyHandlerRegistered = true;
			}
		}

		void HeatSinkLogic_OnClose(IMyEntity obj)
		{
			try
			{
				if (Entity != null)
				{
					(Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= HeatSinkLogic_AppendingCustomInfo;
					(Container.Entity as IMyCubeBlock).OnClose -= HeatSinkLogic_OnClose;

					if (ThermalAuthority.IsServer)
					{
						if (!SkiittzThermalMechanicsSession.IsSessionUnloading
						    && block?.SlimBlock?.IsDestroyed == true)
							RedistributeHeatOnce();
						SaveAuthoritativeState();
					}
					ThermalAuthority.Unregister(obj.EntityId);
				}
			}
			catch (Exception ex)
			{
				MyLog.Default.WriteLine($"SkiittzThermalMechanics: {ex}");
			}
		}

		public override void UpdateOnceBeforeFrame()
		{
			if (block.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
			{
				NeedsUpdate = MyEntityUpdateEnum.NONE;
				return;
			}

			try
			{
				(Container.Entity as IMyCubeBlock).OnClose += HeatSinkLogic_OnClose;
				ThermalAuthority.Register(this);
			}
			catch (Exception ex)
			{
				MyLog.Default.WriteLine($"SkiittzThermalMechanics: {ex}");
			}
			ScriptHookCreator.AddBeaconHeatRatioControl();
		}

		public static void ResetSessionState()
		{
			_destroyHandlerRegistered = false;
		}
	}
}
