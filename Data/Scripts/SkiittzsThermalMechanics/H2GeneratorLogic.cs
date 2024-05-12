using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkiittzsThermalMechanics;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_HydrogenEngine), false)]
    public class H2EngineLogic : MyGameLogicComponent
    {
        private HeatData heatData;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Logger.Instance.LogDebug("Initializing H2 Engine Logic");
            var block = (IMyPowerProducer)Entity;
            var heatCapacity = block.MaxOutput*2;
            var passiveCooling = 1 / block.MaxOutput;
            heatData = new HeatData(block, heatCapacity, passiveCooling);
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += H2EngineLogic_AppendingCustomInfo;
        }

        void H2EngineLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            Logger.Instance.LogDebug("Appending Custom Info");
            var logic = arg1.GameLogic.GetAs<H2EngineLogic>();
            logic.heatData.AppendCustomThermalInfo(customInfo);
        }

        void H2EngineLogic_OnClose(IMyEntity obj)
        {
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= H2EngineLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= H2EngineLogic_OnClose;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            if (heatData.Block.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;

            if (!heatData.IsInitialized)
            {
                CreateControls();
                try
                {
                    (Container.Entity as IMyCubeBlock).OnClose += H2EngineLogic_OnClose;
                }
                catch (Exception ex)
                {

                }
                heatData.IsInitialized = true;
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            heatData.ApplyHeating();
        }

        private void CreateControls()
        {
            var heatPercent = MyAPIGateway.TerminalControls.CreateProperty<float, IMyReactor>("HeatRatio");
            heatPercent.Getter = x => heatData.HeatRatio;
            MyAPIGateway.TerminalControls.AddControl<IMyReactor>(heatPercent);
        }
    }
}
