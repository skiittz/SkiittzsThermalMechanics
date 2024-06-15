using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_BatteryBlock), false)]
    public class BatteryLogic : MyGameLogicComponent
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
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += BatteryLogic_AppendingCustomInfo;
        }

        void BatteryLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            var logic = arg1.GameLogic.GetAs<BatteryLogic>();
            logic.heatData.AppendCustomThermalInfo(logic.block, customInfo);
        }

        void BatteryLogic_OnClose(IMyEntity obj)
        {
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= BatteryLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= BatteryLogic_OnClose;
                    if(block.IsOwnedByAPlayer())
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

        public override void UpdateAfterSimulation100()
        {
            if (block == null || heatData == null || !block.IsOwnedByAPlayer())
                return;

            heatData.ApplyHeating(block);
            block.RefreshCustomInfo();
        }
    }
}
