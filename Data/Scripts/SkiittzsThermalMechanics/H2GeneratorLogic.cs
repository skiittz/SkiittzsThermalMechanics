using System;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_HydrogenEngine), false)]
    public class H2EngineLogic : MyGameLogicComponent
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
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += H2EngineLogic_AppendingCustomInfo;
        }

        void H2EngineLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            var logic = arg1.GameLogic.GetAs<H2EngineLogic>();
            logic.heatData.AppendCustomThermalInfo(logic.block, customInfo);
        }

        void H2EngineLogic_OnClose(IMyEntity obj)
        {
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= H2EngineLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= H2EngineLogic_OnClose;
                    if (block.IsPlayerOwned())
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

        public override void UpdateAfterSimulation100()
        {
            if(block == null || heatData == null || !block.IsPlayerOwned() ) return;

            heatData.ApplyHeating(block);
            block.RefreshCustomInfo();
        }
    }
}
