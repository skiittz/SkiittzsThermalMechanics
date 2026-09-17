using System;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.H2Thruster
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false)]
    public partial class HydrogenThrusterLogic : MyGameLogicComponent
    {
        private ThrusterHeatData heatData;
        private IMyThrust block;
        private bool hasAuthoritativeState;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyThrust)Container.Entity;
            if (block == null)
                return;

            bool configFound;
            if (ThermalAuthority.IsServer)
				heatData = ThrusterHeatData.LoadData(block, out configFound);
            else
            {
                heatData = new ThrusterHeatData();
                configFound = true;
            }
            if (!configFound) return;
            hasAuthoritativeState = ThermalAuthority.IsServer;

			NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
			if (ThermalAuthority.IsServer)
				NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += ThrusterLogic_AppendingCustomInfo;
        }

        void ThrusterLogic_OnClose(IMyEntity obj)
        {
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= ThrusterLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= ThrusterLogic_OnClose;
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

                AddCurrentHeatControl();
                try
                {
                    (Container.Entity as IMyCubeBlock).OnClose += ThrusterLogic_OnClose;
                    ThermalAuthority.Register(this);
                }
                catch (Exception ex)
                {
                    MyLog.Default.WriteLine($"SkiittzThermalMechanics: {ex}");
                }
        }
    }
}
