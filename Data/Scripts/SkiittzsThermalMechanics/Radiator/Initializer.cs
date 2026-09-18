using Sandbox.ModAPI;
using System;
using Sandbox.Common.ObjectBuilders;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Radiator
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_UpgradeModule), false)]
	public partial class HeatRadiatorLogic : MyGameLogicComponent
	{
		private float dissipationMult = 1.0f;
		private int ticksSinceWeatherCheck = 0;
		private RadiatorData radiatorData;
		private IMyUpgradeModule block;
		private bool hasAuthoritativeState;

		public override void Init(MyObjectBuilder_EntityBase objectBuilder)
		{
			block = (Container.Entity as IMyUpgradeModule);
			if (block == null)
				return;

			bool configFound;
			if (ThermalAuthority.IsServer)
				radiatorData = RadiatorData.LoadData(block, out configFound);
			else
			{
				radiatorData = new RadiatorData
				{
					MaxDissipation = 1f,
					MinColor = VRageMath.Color.Black,
					MaxColor = VRageMath.Color.Red
				};
				configFound = true;
			}
			if (!configFound) return;
			hasAuthoritativeState = ThermalAuthority.IsServer;

			NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
			if (ThermalAuthority.IsServer)
				NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
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
			CreateControls();
			try
			{
				(Container.Entity as IMyCubeBlock).OnClose += RadiatorLogic_OnClose;
				ThermalAuthority.Register(this);
			}
			catch (Exception ex)
			{
				MyLog.Default.WriteLine($"SkiittzThermalMechanics: {ex}");
			}
		}

	}
}
