using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace SkiittzsThermalMechanics
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon),false, new []{ "LargeHeatSink", "SmallHeatSink" })]
    public class HeatSinkLogic : MyGameLogicComponent
    {
        private IMyBeacon block;
        private float currentHeat;
        private float heatCapacity => 1000000f;
        private float availableCapacity => heatCapacity - currentHeat;
        public float HeatRatio => (currentHeat / heatCapacity);
        private bool isInitialized;
        private float passiveCooling = 0.01f;
        private float ventingHeat;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Logger.Instance.LogDebug("Initializing Heat Sink Logic");
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

            var logic = arg1.GameLogic.GetAs<HeatSinkLogic>();
            customInfo.Append($"Heat Level: {(logic.currentHeat / logic.heatCapacity) * 100}%\n");
            customInfo.Append($"Current IR Detectable Distance: {block.Radius:N0} meters \n");
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
                AddHeatRatioControl();
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
            block.Enabled = true;
            currentHeat -= Math.Min(passiveCooling, currentHeat);
            block.Radius = Math.Min(500000, ventingHeat);
            (block as IMyTerminalBlock).RefreshCustomInfo();
            ventingHeat *= 0.95f;
        }

        public void AddHeatRatioControl()
        {
            var existingControls = new List<IMyTerminalControl>();
            MyAPIGateway.TerminalControls.GetControls<IMyBeacon>(out existingControls);
            if (existingControls.Any(x => x.Id == Utilities.HeatRatioControlId))
                return;

            var heatPercent = MyAPIGateway.TerminalControls.CreateProperty<float, IMyBeacon>(Utilities.HeatRatioControlId);
            heatPercent.Getter = x => HeatRatio;
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(heatPercent);
        }

        public float RemoveHeat(float heat)
        {
            var dissipatedHeat = Math.Min(heat, currentHeat);
            currentHeat -= dissipatedHeat;
            ventingHeat += dissipatedHeat;

            return dissipatedHeat;
        }
    }
}
