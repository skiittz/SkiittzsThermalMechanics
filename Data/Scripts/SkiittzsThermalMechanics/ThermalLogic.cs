using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.Render.Particles;
using VRage.Utils;
using VRageMath;

namespace SkiittzsThermalMechanics
{
    public class ThrusterHeatData
    {
        public IMyThrust Block { get; }
        public float CurrentHeat { get; private set; }
        private float passiveCooling;
        public bool IsInitialized { get; set; }
        private float lastHeatDelta;
        public ThrusterHeatData(IMyThrust block, float passiveCooling)
        {
            Block = block;
            this.passiveCooling = passiveCooling;
        }

        public void ApplyHeating()
        {
            lastHeatDelta = CalculateHeating();
            CurrentHeat += lastHeatDelta;
            CurrentHeat -= CalculateCooling(CurrentHeat);
            if (CurrentHeat < 0)
                CurrentHeat = 0;
        }

        private float CalculateCooling(float availableHeatToSink)
        {
            return Utilities.GetHeatSinkLogic(Block.CubeGrid)?.ActiveCooling(availableHeatToSink) ?? 0;
        }

        private float CalculateHeating()
        {
            if (!Block.IsWorking)
                return 0;
            Logger.Instance.LogDebug($"{Block.CustomName} is {(Block.Enabled ? "Enabled" : "Disabled")}");
            Logger.Instance.LogDebug($"{Block.CustomName} heating: (currentOutput {Block.CurrentThrust/ 1000000}) - (passiveCooling {passiveCooling})");
            return (Block.CurrentThrust/1000000) - passiveCooling;
        }

        public void AppendCustomThermalInfo(StringBuilder customInfo)
        {
            var debugInfo = new StringBuilder();
            debugInfo.Append($"DEBUG INFO - {Block.CustomName}:\n");
            debugInfo.Append($"Current Heat: {CurrentHeat}\n");
            Logger.Instance.LogDebug(debugInfo.ToString());

            customInfo.Append($"Current Heat Level: {CurrentHeat}\n");
        }
    }
    public class PowerPlantHeatData
    {
        public IMyPowerProducer Block { get; }
        public float CurrentHeat { get; private set; }
        public float HeatCapacity { get; }
        private float passiveCooling;
        public bool IsInitialized { get; set; }
        private float lastHeatDelta;
        public float HeatRatio => (CurrentHeat / HeatCapacity);
        private int overHeatCycles { get; set; }

        public PowerPlantHeatData(IMyPowerProducer block, float heatCapacity, float passiveCooling)
        {
            Block = block;
            this.HeatCapacity = heatCapacity;
            this.passiveCooling = passiveCooling;
        }

        private float CalculateCooling(float availableHeatToSink)
        {
            return Utilities.GetHeatSinkLogic(Block.CubeGrid)?.ActiveCooling(availableHeatToSink) ?? 0;
        }

        private float CalculateHeating()
        {
            if (!Block.IsWorking)
                return 0;
            Logger.Instance.LogDebug($"{Block.CustomName} is {(Block.Enabled ? "Enabled" : "Disabled")}");
            Logger.Instance.LogDebug($"{Block.CustomName} heating: (currentOutput {Block.CurrentOutput}) - (passiveCooling {passiveCooling})");
            var powerProducers = new List<IMyPowerProducer>();
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Block.CubeGrid);
            gts.GetBlocksOfType(powerProducers, x => x.IsWorking);

            var additionalGeneratorCount = Math.Min(powerProducers.Count - 1, 0);
            var spamPenalty = 1 + (additionalGeneratorCount / 100);
            return Block.CurrentOutput*spamPenalty - passiveCooling;
        }

        public void ApplyHeating()
        {
            var heatGenerated = CalculateHeating();
            var heatDissipated = CalculateCooling(CurrentHeat + heatGenerated);
            lastHeatDelta = heatGenerated - heatDissipated;
            CurrentHeat += lastHeatDelta;
            if (CurrentHeat < 0)
                CurrentHeat = 0;

            if (CurrentHeat >= HeatCapacity)
            {
                WarnPlayer($"Taking damage due to overheating!");
                overHeatCycles++;
                var thermalFatigue = CurrentHeat + (CurrentHeat * (overHeatCycles/10));
                Block.SlimBlock.DoDamage((thermalFatigue - HeatCapacity), MyStringHash.GetOrCompute("Overheating"), true);
                if (Block.IsFunctional)
                    CurrentHeat = HeatCapacity;
                else
                {
                    WarnPlayer("Disabled due to heat damage.");
                    CurrentHeat = 0;
                }
            }
            else if (CurrentHeat > HeatCapacity * .8)
            {
                Block.SetDamageEffect(true);
                WarnPlayer($"Approaching heat threshold.");
            }
            else
                Block.SetDamageEffect(false);
        }

        private const int messageDelay = 10;
        private int messageAttemptCounter = 0;
        private void WarnPlayer(string message)
        {
            if (messageAttemptCounter == 0)
            {
                MyAPIGateway.Utilities.ShowMessage("dumdum.bot", $"{Block.CubeGrid.CustomName}-{Block.CustomName}: {message}");
            }

            messageAttemptCounter++;
            if (messageAttemptCounter == messageDelay)
                messageAttemptCounter = 0;
        }

        public void AppendCustomThermalInfo(StringBuilder customInfo)
        {
            var remainingSeconds = RemainingSeconds();

            var debugInfo = new StringBuilder();
            debugInfo.Append($"DEBUG INFO - {Block.CustomName}:\n");
            debugInfo.Append($"Current Heat: {CurrentHeat}\n");
            debugInfo.Append($"Heat Capacity: {HeatCapacity}\n");
            debugInfo.Append($"Last Heat Delta: {lastHeatDelta}\n");
            debugInfo.Append($"Remaining Seconds: {remainingSeconds}");
            Logger.Instance.LogDebug(debugInfo.ToString());

            customInfo.Append($"Heat Level: {(CurrentHeat / HeatCapacity) * 100}%\n");
            customInfo.Append($"Time until {(lastHeatDelta <= 0 ? "cooled" : "overheat")}: {TimeUntilOverheatDisplay(Math.Abs(remainingSeconds))}\n");
        }

        private const float SecondsPer100Ticks = 1.667f;
        private float RemainingSeconds()
        {
            var numerator = lastHeatDelta > 0 ? HeatCapacity - CurrentHeat : CurrentHeat;
            var denominator = lastHeatDelta == 0 ? 1 : Math.Abs(lastHeatDelta);
            return (numerator / denominator) * SecondsPer100Ticks;
        }

        private static string TimeUntilOverheatDisplay(float remainingSeconds)
        {
            Logger.Instance.LogDebug($"Display Timespan for remaining seconds: {remainingSeconds}");
            var timeSpan = TimeSpan.FromSeconds(remainingSeconds);

            return timeSpan.ToString("hh\\:mm\\:ss");
        }
    }
}
