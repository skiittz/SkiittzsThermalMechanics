using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI;
using VRage.Utils;

namespace SkiittzsThermalMechanics
{
    public class HeatData
    {
        public IMyPowerProducer Block { get; }
        public float CurrentHeat { get; private set; }
        public float HeatCapacity { get; }
        private float passiveCooling;
        public bool IsInitialized { get; set; }
        private float lastHeatDelta;
        public float HeatRatio => (CurrentHeat / HeatCapacity);

        public HeatData(IMyPowerProducer block, float heatCapacity, float passiveCooling)
        {
            Block = block;
            this.HeatCapacity = heatCapacity;
            this.passiveCooling = passiveCooling;
        }

        private float CalculateCooling(float lastHeatDelta)
        {
            var beacons = new List<IMyBeacon>();
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Block.CubeGrid);
            gts.GetBlocksOfType(beacons, x => x.IsWorking);

            if (!beacons.Any())
                return 0;

            return beacons.OrderByDescending(x => x.Radius).First().GameLogic.GetAs<BeaconLogic>().ActiveCooling(lastHeatDelta);
        }

        private float CalculateHeating()
        {
            if (!Block.IsWorking)
                return 0;
            Logger.Instance.LogDebug($"{Block.CustomName} is {(Block.Enabled ? "Enabled" : "Disabled")}");
            Logger.Instance.LogDebug($"{Block.CustomName} heating: (currentOutput {Block.CurrentOutputRatio}) - (passiveCooling {passiveCooling})");
            return Block.CurrentOutputRatio - passiveCooling;
        }

        public void ApplyHeating()
        {
            lastHeatDelta = CalculateHeating();
            lastHeatDelta -= CalculateCooling(lastHeatDelta);
            CurrentHeat += lastHeatDelta;
            if (CurrentHeat < 0)
                CurrentHeat = 0;

            if (CurrentHeat <= HeatCapacity)
                return;

            var beacons = new List<IMyBeacon>();
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Block.CubeGrid);
            gts.GetBlocksOfType(beacons, x => x.IsWorking);
            if (beacons.Any())
            {
                var beacon = beacons.OrderByDescending(x => x.Radius).First();
                var logic = beacon.GameLogic.GetAs<BeaconLogic>();
                logic.RemoveHeatDueToBlockDeath(HeatCapacity);
            }

            Block.SlimBlock.DoDamage(10000f, MyStringHash.GetOrCompute("Overheating"), true);
        }

        public void AppendCustomThermalInfo(StringBuilder customInfo)
        {
            var remainingSeconds = SecondsUntilOverheat();

            var debugInfo = new StringBuilder();
            debugInfo.Append($"DEBUG INFO - {Block.CustomName}:\n");
            debugInfo.Append($"Current Heat: {CurrentHeat}\n");
            debugInfo.Append($"Heat Capacity: {HeatCapacity}\n");
            debugInfo.Append($"Last Heat Delta: {lastHeatDelta}\n");
            debugInfo.Append($"Remaining Seconds: {remainingSeconds}");
            Logger.Instance.LogDebug(debugInfo.ToString());

            customInfo.Append($"Heat Level: {(CurrentHeat / HeatCapacity) * 100}%\n");
            customInfo.Append($"Time until {(remainingSeconds < 0 ? "cooled" : "overheat")}: {TimeUntilOverheatDisplay(Math.Abs(remainingSeconds))}\n");
        }

        private float SecondsUntilOverheat()
        {
            return ((lastHeatDelta > 0 ? HeatCapacity - CurrentHeat : CurrentHeat) / (lastHeatDelta == 0 ? 1 : lastHeatDelta)) / 1.667f;
        }

        private static string TimeUntilOverheatDisplay(float remainingSeconds)
        {
            Logger.Instance.LogDebug($"Display Timespan for remaining seconds: {remainingSeconds}");
            var timeSpan = TimeSpan.FromSeconds(remainingSeconds);

            return timeSpan.ToString("hh\\:mm\\:ss");
        }
    }
}
