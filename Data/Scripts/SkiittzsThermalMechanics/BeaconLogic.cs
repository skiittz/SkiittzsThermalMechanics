using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon),false)]
    public class BeaconLogic : MyGameLogicComponent
    {
        private IMyBeacon block;
        private float currentHeat;
        private float heatCapacity => 1000f;
        private float availableCapacity => heatCapacity - currentHeat;
        private float heatRatio => (currentHeat / heatCapacity);
        private bool isInitialized;
        private float cooling => heatRatio * 5;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Logger.Instance.LogDebug("Initializing Beacon Logic");
            block = (IMyBeacon)Entity;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += BeaconLogic_AppendingCustomInfo;
        }

        public float ActiveCooling(float incomingHeat)
        {
            if (availableCapacity > incomingHeat)
            {
                currentHeat += incomingHeat;
                return incomingHeat;
            }
            else
            {
                var remainingHeat = incomingHeat - availableCapacity;
                currentHeat = heatCapacity;
                return incomingHeat - remainingHeat;
            }
        }

        void BeaconLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            var debugInfo = new StringBuilder();
            debugInfo.Append($"DEBUG INFO - {arg1.CustomName}:\n");
            debugInfo.Append($"Current Heat: {currentHeat}\n");
            debugInfo.Append($"Heat Capacity: {heatCapacity}\n");
            Logger.Instance.LogDebug(debugInfo.ToString());

            var logic = arg1.GameLogic.GetAs<BeaconLogic>();
            customInfo.Append($"Heat Level: {(logic.currentHeat / logic.heatCapacity) * 100}%\n");
        }

        void BeaconLogic_OnClose(IMyEntity obj)
        {
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= BeaconLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= BeaconLogic_OnClose;
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

            if (!isInitialized)
            {
                CreateControls();
                try
                {
                    (Container.Entity as IMyCubeBlock).OnClose += BeaconLogic_OnClose;
                }
                catch (Exception ex)
                {

                }
                isInitialized = true;
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            currentHeat -= cooling;

            block.Radius = Math.Min(500000,500000 * heatRatio);
            (block as IMyTerminalBlock).RefreshCustomInfo();
        }

        public void RemoveHeatDueToBlockDeath(float heat)
        {
            currentHeat -= heat;
        }

        private void CreateControls()
        {
            var heatPercent = MyAPIGateway.TerminalControls.CreateProperty<float, IMyBeacon>("HeatRatio");
            heatPercent.Getter = x => heatRatio;
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(heatPercent);
        }
    }
}
