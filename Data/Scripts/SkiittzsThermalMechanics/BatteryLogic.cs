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
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_BatteryBlock), false)]
    public class BatteryLogic : MyGameLogicComponent
    {
        private PowerPlantHeatData heatData;
        private IMyPowerProducer block;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Logger.Instance.LogDebug("Initializing Battery Logic");
            block = (IMyPowerProducer)Entity;
            if (block == null || !block.CubeGrid.IsPlayerOwnedGrid())
                return;

            if (!PowerPlantHeatData.LoadData(block, out heatData))
                heatData = new PowerPlantHeatData
                {
                    HeatCapacity = (block as IMyBatteryBlock).MaxInput * 400,
                    PassiveCooling = (block as IMyBatteryBlock).MaxInput / 30
                };
            
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += BatteryLogic_AppendingCustomInfo;
        }

        void BatteryLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            Logger.Instance.LogDebug("Appending Custom Info");
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

            AddHeatRatioControl();
            try
            {
                (Container.Entity as IMyCubeBlock).OnClose += BatteryLogic_OnClose;
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
            MyAPIGateway.TerminalControls.GetControls<IMyBatteryBlock>(out existingControls);
            if (existingControls.Any(x => x.Id == Utilities.HeatRatioControlId))
                return;

            var heatPercent = MyAPIGateway.TerminalControls.CreateProperty<float, IMyBatteryBlock>(Utilities.HeatRatioControlId);
            heatPercent.Getter = x => heatData.HeatRatio;
            MyAPIGateway.TerminalControls.AddControl<IMyBatteryBlock>(heatPercent);
        }
    }
}
