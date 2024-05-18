using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_BatteryBlock), false)]
    public class BatteryLogic : MyGameLogicComponent
    {
        private PowerPlantHeatData heatData;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Logger.Instance.LogDebug("Initializing Battery Logic");
            var block = (IMyPowerProducer)Entity;
            var heatCapacity = (block as IMyBatteryBlock).MaxInput * 100;
            var passiveCooling = (block as IMyBatteryBlock).MaxInput / 30;
            heatData = new PowerPlantHeatData(block, heatCapacity, passiveCooling);
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += BatteryLogic_AppendingCustomInfo;
        }

        void BatteryLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            Logger.Instance.LogDebug("Appending Custom Info");
            var logic = arg1.GameLogic.GetAs<BatteryLogic>();
            logic.heatData.AppendCustomThermalInfo(customInfo);
        }

        void BatteryLogic_OnClose(IMyEntity obj)
        {
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= BatteryLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= BatteryLogic_OnClose;
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
                AddHeatRatioControl();
                try
                {
                    (Container.Entity as IMyCubeBlock).OnClose += BatteryLogic_OnClose;
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

        public void AddHeatRatioControl()
        {
            var existingControls = new List<IMyTerminalControl>();
            MyAPIGateway.TerminalControls.GetControls<IMyPowerProducer>(out existingControls);
            if (existingControls.Any(x => x.Id == Utilities.HeatRatioControlId))
                return;

            var heatPercent = MyAPIGateway.TerminalControls.CreateProperty<float, IMyPowerProducer>(Utilities.HeatRatioControlId);
            heatPercent.Getter = x => heatData.HeatRatio;
            MyAPIGateway.TerminalControls.AddControl<IMyPowerProducer>(heatPercent);
        }
    }
}
