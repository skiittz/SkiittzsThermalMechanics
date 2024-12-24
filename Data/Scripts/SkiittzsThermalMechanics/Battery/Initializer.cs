using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Battery
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_BatteryBlock), false)]
	public partial class BatteryLogic : MyGameLogicComponent
	{
		public PowerPlantHeatData heatData;
		private IMyPowerProducer block;
		public override void Init(MyObjectBuilder_EntityBase objectBuilder)
		{
			block = (IMyPowerProducer)Entity;
			if (block == null)
				return;

			bool configFound = false;
			heatData = PowerPlantHeatData.LoadData(block, out configFound);
			if (!configFound) return;

			NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
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
					if (block.IsOwnedByAPlayer())
						PowerPlantHeatData.SaveData(obj.EntityId, obj.GameLogic.GetAs<BatteryLogic>().heatData);
				}
			}
			catch (Exception ex)
			{

			}
		}

		public override void UpdateOnceBeforeFrame()
		{
			if (block.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
				return;

			try
			{
				(Container.Entity as IMyCubeBlock).OnClose += BatteryLogic_OnClose;
			}
			catch (Exception ex)
			{

			}
			ScriptHookCreator.AddBatteryHeatRatioControl();
		}

	}
}
