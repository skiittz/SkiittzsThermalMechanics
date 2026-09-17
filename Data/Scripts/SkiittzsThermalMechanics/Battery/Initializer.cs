using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Battery
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_BatteryBlock), false)]
	public partial class BatteryLogic : MyGameLogicComponent
	{
		public PowerPlantHeatData heatData;
		private IMyPowerProducer block;
		private bool hasAuthoritativeState;
		public override void Init(MyObjectBuilder_EntityBase objectBuilder)
		{
			block = (IMyPowerProducer)Entity;
			if (block == null)
				return;

			bool configFound;
			if (ThermalAuthority.IsServer)
				heatData = PowerPlantHeatData.LoadData(block, out configFound, block.DefaultId("Battery"));
			else
			{
				heatData = new PowerPlantHeatData { HeatCapacity = 1f, HeatGenerationMultiplier = 1f };
				configFound = true;
			}
			if (!configFound) return;
			hasAuthoritativeState = ThermalAuthority.IsServer;

			NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
			if (ThermalAuthority.IsServer)
				NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
			(Container.Entity as IMyTerminalBlock).AppendingCustomInfo += BatteryLogic_AppendingCustomInfo;
		}

		void BatteryLogic_OnClose(IMyEntity obj)
		{
			try
			{
				if (Entity != null)
				{
					(Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= BatteryLogic_AppendingCustomInfo;
					(Container.Entity as IMyCubeBlock).OnClose -= BatteryLogic_OnClose;
					if (ThermalAuthority.IsServer)
						SaveAuthoritativeState();
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
				(Container.Entity as IMyCubeBlock).OnClose += BatteryLogic_OnClose;
				ThermalAuthority.Register(this);
			}
			catch (Exception ex)
			{
				MyLog.Default.WriteLine($"SkiittzThermalMechanics: {ex}");
			}
			ScriptHookCreator.AddBatteryHeatRatioControl();
		}

	}
}
