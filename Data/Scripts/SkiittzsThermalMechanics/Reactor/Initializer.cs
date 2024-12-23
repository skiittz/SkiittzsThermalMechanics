using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Reactor
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_Reactor), false)]
	public partial class ReactorLogic : MyGameLogicComponent
	{
		public PowerPlantHeatData heatData;
		private IMyPowerProducer block;
		public override void Init(MyObjectBuilder_EntityBase objectBuilder)
		{
			block = (IMyPowerProducer)Entity;
			if (block == null)
				return;

			heatData = PowerPlantHeatData.LoadData(block);
			NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
			(Container.Entity as IMyTerminalBlock).AppendingCustomInfo += ReactorLogic_AppendingCustomInfo;
		}

		void ReactorLogic_OnClose(IMyEntity obj)
		{
			try
			{
				if (Entity != null)
				{
					(Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= ReactorLogic_AppendingCustomInfo;
					(Container.Entity as IMyCubeBlock).OnClose -= ReactorLogic_OnClose;
					if (block.IsOwnedByAPlayer())
						PowerPlantHeatData.SaveData(obj.EntityId, obj.GameLogic.GetAs<ReactorLogic>().heatData);
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
				(Container.Entity as IMyCubeBlock).OnClose += ReactorLogic_OnClose;
			}
			catch (Exception ex)
			{

			}
			ScriptHookCreator.AddReactorHeatRatioControl();
		}
	}
}
