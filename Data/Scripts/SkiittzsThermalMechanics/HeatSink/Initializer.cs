using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

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

			if (!_destroyHandlerRegistered)
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
					HeatSinkData.SaveData(obj.EntityId, obj.GameLogic.GetAs<HeatSinkLogic>().HeatSinkData);
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
				return;

			try
			{
				(Container.Entity as IMyCubeBlock).OnClose += HeatSinkLogic_OnClose;
			}
			catch (Exception ex)
			{
				MyLog.Default.WriteLine($"SkiittzThermalMechanics: {ex}");
			}
			ScriptHookCreator.AddBeaconHeatRatioControl();
		}
	}
}
