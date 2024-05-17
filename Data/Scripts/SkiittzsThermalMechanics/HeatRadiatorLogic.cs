using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_UpgradeModule), false, new []
    {
        "LargeHeatRadiatorBlock", "SmallHeatRadiatorBlock"
    })]
    public class HeatRadiatorLogic : MyGameLogicComponent
    {
        private IMyUpgradeModule block;
        private bool isInitialized;
        private float dissipation = 10f;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Logger.Instance.LogDebug("Initializing Radiator Logic");
            block = (IMyUpgradeModule) Entity;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += RadiatorLogic_AppendingCustomInfo;

        }

        void RadiatorLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            var debugInfo = new StringBuilder();
            debugInfo.Append($"DEBUG INFO - {arg1.CustomName}:\n");
            Logger.Instance.LogDebug(debugInfo.ToString());

            var logic = arg1.GameLogic.GetAs<HeatRadiatorLogic>();
        }

        void RadiatorLogic_OnClose(IMyEntity obj)
        {
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= RadiatorLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= RadiatorLogic_OnClose;
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

            if (!isInitialized)
            {
                CreateControls();
                try
                {
                    (Container.Entity as IMyCubeBlock).OnClose += RadiatorLogic_OnClose;
                }
                catch (Exception ex)
                {

                }
                isInitialized = true;
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            var beacon = Utilities.GetBeaconLogic(block.CubeGrid);
            if (beacon == null)
                return;

            beacon.RemoveHeat(dissipation);
            Animate(beacon.HeatRatio);
        }

        private void CreateControls()
        {
        }

        private void Animate(float heatLevel)
        {

        }
    }
}
