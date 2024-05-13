using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Reactor), false)]
    public class ReactorLogic : MyGameLogicComponent
    {
        private PowerPlantHeatData heatData;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Logger.Instance.LogDebug("Initializing Reactor Logic");
            var block = (IMyPowerProducer)Entity;
            var heatCapacity = block.MaxOutput*100;
            var passiveCooling = block.MaxOutput/60;
            heatData = new PowerPlantHeatData(block, heatCapacity, passiveCooling);
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += ReactorLogic_AppendingCustomInfo;
        }

        void ReactorLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            Logger.Instance.LogDebug("Appending Custom Info");
            var logic = arg1.GameLogic.GetAs<ReactorLogic>();
            logic.heatData.AppendCustomThermalInfo(customInfo);
        }

     

        void ReactorLogic_OnClose(IMyEntity obj)
        {
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= ReactorLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= ReactorLogic_OnClose;
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
                    (Container.Entity as IMyCubeBlock).OnClose += ReactorLogic_OnClose;
                }
                catch (Exception ex)
                {

                }
                heatData.IsInitialized = true;
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            if (heatData?.Block == null)
                return;

            heatData.ApplyHeating();
            (heatData.Block as IMyTerminalBlock).RefreshCustomInfo();
        }

        private void CreateControls()
        {
            var heatPercent = MyAPIGateway.TerminalControls.CreateProperty<float, IMyReactor>("HeatRatio");
            heatPercent.Getter = x => heatData.HeatRatio;
            MyAPIGateway.TerminalControls.AddControl<IMyReactor>(heatPercent);
        }
    }
}
