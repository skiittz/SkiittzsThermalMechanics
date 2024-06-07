using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Reactor), false)]
    public class ReactorLogic : MyGameLogicComponent
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

        void ReactorLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            var logic = arg1.GameLogic.GetAs<ReactorLogic>();
            logic.heatData.AppendCustomThermalInfo(logic.block, customInfo);
        }

     

        void ReactorLogic_OnClose(IMyEntity obj)
        {
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= ReactorLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= ReactorLogic_OnClose;
                    if (block.IsOwnedByCurrentPlayer())
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

        public override void UpdateAfterSimulation100()
        {
            if (block == null || heatData == null || !block.IsOwnedByCurrentPlayer() ) return;

            heatData.ApplyHeating(block);
            block.RefreshCustomInfo();
        }
    }
}
