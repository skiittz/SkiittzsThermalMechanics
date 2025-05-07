using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System;
using System.Net.Configuration;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.HeatSink
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon), false)]
	public partial class HeatSinkLogic : MyGameLogicComponent
	{
		private IMyBeacon block;
		public HeatSinkData HeatSinkData;

		public override void Init(MyObjectBuilder_EntityBase objectBuilder)
		{
			block = (IMyBeacon)Entity;
			if (block == null)
				return;

			bool configFound = false;
			HeatSinkData = HeatSinkData.LoadData(block, out configFound);
			if (!configFound) return;

			HeatSinkData.IsSmallGrid = block.CubeGrid.GridSizeEnum == MyCubeSize.Small;
			bool shuntToParent = false;
			Configuration.Configuration.TryGetGeneralSettingValue("SmallGridShuntsToLarge", out shuntToParent);
			HeatSinkData.ShuntToParent = shuntToParent;

			NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
			(Container.Entity as IMyTerminalBlock).AppendingCustomInfo += HeatSinkLogic_AppendingCustomInfo;

			MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, OnBlockDestroyed);
		}

		void HeatSinkLogic_OnClose(IMyEntity obj)
		{
			try
			{
				if (Entity != null)
				{
					(Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= HeatSinkLogic_AppendingCustomInfo;
					(Container.Entity as IMyCubeBlock).OnClose -= HeatSinkLogic_OnClose;
					HeatSinkData.SaveData(obj.EntityId, obj.GameLogic.GetAs<HeatSinkLogic>().HeatSinkData);
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
				(Container.Entity as IMyCubeBlock).OnClose += HeatSinkLogic_OnClose;
			}
			catch (Exception ex)
			{

			}
			ScriptHookCreator.AddBeaconHeatRatioControl();
		}
	}
}
