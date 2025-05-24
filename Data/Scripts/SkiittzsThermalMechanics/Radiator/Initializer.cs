using Sandbox.ModAPI;
using System;
using Sandbox.Common.ObjectBuilders;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Radiator
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_UpgradeModule), false)]
	public partial class HeatRadiatorLogic : MyGameLogicComponent
	{
		private float dissipationMult = 1.0f;
		private int ticksSinceWeatherCheck = 0;
		private RadiatorData radiatorData;
		private IMyUpgradeModule block;

		public override void Init(MyObjectBuilder_EntityBase objectBuilder)
		{
			block = (Container.Entity as IMyUpgradeModule);
			if (block == null)
				return;

			bool configFound = false;
			radiatorData = RadiatorData.LoadData(block, out configFound);
			if (!configFound) return;

			NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
			(Container.Entity as IMyTerminalBlock).AppendingCustomInfo += RadiatorLogic_AppendingCustomInfo;
		}

		void RadiatorLogic_OnClose(IMyEntity obj)
		{
			try
			{
				if (Entity != null)
				{
					(Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= RadiatorLogic_AppendingCustomInfo;
					(Container.Entity as IMyCubeBlock).OnClose -= RadiatorLogic_OnClose;
					RadiatorData.SaveData(obj.EntityId, (obj).GameLogic.GetAs<HeatRadiatorLogic>().radiatorData);
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
			CreateControls();
			try
			{
				(Container.Entity as IMyCubeBlock).OnClose += RadiatorLogic_OnClose;
			}
			catch (Exception ex)
			{

			}
		}

	}
}
