using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Sandbox.ModAPI;
using VRage.Game.ModAPI.Ingame;

namespace SkiittzsThermalMechanics
{
    public class HeatData
    {
        public IMyPowerProducer Block { get; }
        public float CurrentHeat { get; private set; }
        public float HeatCapacity { get; }
        private float passiveCooling;
        public bool IsInitialized { get; set; }
        private bool isOverheated;
        private float lastHeatDelta;

        public HeatData(IMyPowerProducer block, float heatCapacity, float passiveCooling)
        {
            Block = block;
            this.HeatCapacity = heatCapacity;
            this.passiveCooling = passiveCooling;
        }

        private float CalculateCooling()
        {
            var beacons = new List<IMyBeacon>();
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Block.CubeGrid);
            gts.GetBlocksOfType(beacons, x => x.IsWorking);

            if (!beacons.Any())
                return 0;

            return beacons.OrderByDescending(x => x.Radius).First().GameLogic.GetAs<BeaconLogic>().ActiveCooling();
        }

        private float CalculateHeating()
        {
            if (!Block.IsWorking)
                return 0;

            return Block.CurrentOutputRatio - passiveCooling;
        }

        public void ApplyHeating()
        {
            lastHeatDelta = CalculateHeating() - CalculateCooling();
            CurrentHeat += lastHeatDelta;
            if (CurrentHeat < 0)
                CurrentHeat = 0;


            if (CurrentHeat > HeatCapacity)
            {
                Block.Enabled = false;
                isOverheated = true;
            }
            else if (isOverheated)
            {
                isOverheated = false;
                Block.Enabled = true;
            }
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
            var timeSpan = TimeSpan.FromSeconds(remainingSeconds);

            return timeSpan.ToString("hh\\:mm\\:ss");
        }
    }
}
