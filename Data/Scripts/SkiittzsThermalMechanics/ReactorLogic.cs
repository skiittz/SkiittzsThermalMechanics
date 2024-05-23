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
            Logger.Instance.LogDebug("Initializing Reactor Logic");
            block = (IMyPowerProducer)Entity;
            if (block == null || !block.CubeGrid.IsPlayerOwnedGrid())
                return;

            if (!PowerPlantHeatData.LoadData(block, out heatData))
                heatData = new PowerPlantHeatData
                {
                    HeatCapacity = block.MaxOutput * 100,
                    PassiveCooling = block.MaxOutput / 600
                };
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += ReactorLogic_AppendingCustomInfo;
        }

        void ReactorLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            Logger.Instance.LogDebug("Appending Custom Info");
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

            AddHeatRatioControl();
            try
            {
                (Container.Entity as IMyCubeBlock).OnClose += ReactorLogic_OnClose;
            }
            catch (Exception ex)
            {

            }
        }

        public override void UpdateBeforeSimulation100()
        {
            if (block == null)
                return;

            heatData.ApplyHeating(block);
            block.RefreshCustomInfo();
        }

        public void AddHeatRatioControl()
        {
            var existingControls = new List<IMyTerminalControl>();
            MyAPIGateway.TerminalControls.GetControls<IMyReactor>(out existingControls);
            if (existingControls.Any(x => x.Id == Utilities.HeatRatioControlId))
                return;

            var heatPercent = MyAPIGateway.TerminalControls.CreateProperty<float, IMyReactor>(Utilities.HeatRatioControlId);
            heatPercent.Getter = x => heatData.HeatRatio;
            MyAPIGateway.TerminalControls.AddControl<IMyReactor>(heatPercent);
        }
    }
}
