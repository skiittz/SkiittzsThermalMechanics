using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.H2Generator
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_HydrogenEngine), false)]
	public partial class H2EngineLogic : MyGameLogicComponent
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
			(Container.Entity as IMyTerminalBlock).AppendingCustomInfo += H2EngineLogic_AppendingCustomInfo;
		}

		void H2EngineLogic_OnClose(IMyEntity obj)
		{
			try
			{
				if (Entity != null)
				{
					(Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= H2EngineLogic_AppendingCustomInfo;
					(Container.Entity as IMyCubeBlock).OnClose -= H2EngineLogic_OnClose;
					if (block.IsOwnedByAPlayer())
						PowerPlantHeatData.SaveData(obj.EntityId, obj.GameLogic.GetAs<H2EngineLogic>().heatData);
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
				(Container.Entity as IMyCubeBlock).OnClose += H2EngineLogic_OnClose;
			}
			catch (Exception ex)
			{

			}
			ScriptHookCreator.AddH2HeatRatioControl();
		}
	}
}
