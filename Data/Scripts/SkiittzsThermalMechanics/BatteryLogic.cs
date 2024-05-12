using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
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
        private IMyPowerProducer block;
        private float currentHeat;
        private float heatCapacity;
        private float passiveCooling;
        private bool initialized;
        private bool isOverheated;
        private float lastHeatDelta;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Logger.Instance.LogDebug("Initializing Thermal Logic");
            block = (IMyPowerProducer)Entity;
            heatCapacity = block.MaxOutput*2;
            passiveCooling = 1 / block.MaxOutput;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += BatteryLogic_AppendingCustomInfo;
        }

        void BatteryLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            Logger.Instance.LogDebug("Appending Custom Info");
            var logic = arg1.GameLogic.GetAs<BatteryLogic>();
            ThermalLogic.AppendCustomThermalInfo(customInfo, logic.heatCapacity, logic.currentHeat, logic.lastHeatDelta);
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
            if (block.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;

            if (!initialized)
            {
                CreateControls();
                try
                {
                    (Container.Entity as IMyCubeBlock).OnClose += BatteryLogic_OnClose;
                }
                catch (Exception ex)
                {

                }
                initialized = true;
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            ApplyHeating();
            (block as IMyTerminalBlock).RefreshCustomInfo();
        }

        private void CreateControls()
        {
            var heatPercent = MyAPIGateway.TerminalControls.CreateProperty<float, IMyBatteryBlock>("HeatRatio");
            heatPercent.Getter = x => currentHeat / heatCapacity;
            MyAPIGateway.TerminalControls.AddControl<IMyBatteryBlock>(heatPercent);
        }



        private float CalculateCooling()
        {
            var beacons = new List<IMyBeacon>();
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid);
            gts.GetBlocksOfType(beacons, x => x.IsWorking);

            if (!beacons.Any())
                return 0;

            return beacons.OrderByDescending(x => x.Radius).First().GameLogic.GetAs<BeaconLogic>().ActiveCooling();
        }


        private float CalculateHeating()
        {
            if (!block.IsWorking)
                return 0;

            return block.CurrentOutputRatio - passiveCooling;
        }

        private void ApplyHeating()
        {
            lastHeatDelta = CalculateHeating() - CalculateCooling();
            currentHeat += lastHeatDelta;
            if (currentHeat < 0)
                currentHeat = 0;


            if (currentHeat > heatCapacity)
            {
                block.Enabled = false;
                isOverheated = true;
            }
            else if (isOverheated)
            {
                isOverheated = false;
                block.Enabled = true;
            }
        }
    }
}
