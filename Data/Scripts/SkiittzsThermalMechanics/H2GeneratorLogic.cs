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
        private PowerPlantHeatData heatData;
        private IMyPowerProducer block;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Logger.Instance.LogDebug("Initializing H2 Engine Logic");
            block = (IMyPowerProducer)Entity;
            if (block == null || !block.CubeGrid.IsPlayerOwnedGrid())
                return;

            if (!PowerPlantHeatData.LoadData(block, out heatData))
                heatData = new PowerPlantHeatData
                {
                    HeatCapacity = block.MaxOutput * 100,
                    PassiveCooling = block.MaxOutput / 50
                };

            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += H2EngineLogic_AppendingCustomInfo;
        }

        void H2EngineLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            Logger.Instance.LogDebug("Appending Custom Info");
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
        }

        public override void UpdateBeforeSimulation100()
        {
            heatData.ApplyHeating(block);
            block.RefreshCustomInfo();
        }
    }
}
