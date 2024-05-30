using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using VRage.Utils;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public class ThrusterHeatData
    {
        public float CurrentHeat { get; set; }
        private float passiveCooling => .25f;
        private float lastHeatDelta;
        private const float thrustDivisor = 100000;
        public void ApplyHeating(IMyThrust block)
        {
            lastHeatDelta = CalculateHeating(block);
            CurrentHeat += lastHeatDelta;
            CurrentHeat -= CalculateCooling(block, CurrentHeat);
            if (CurrentHeat < 0)
                CurrentHeat = 0;
        }

        private float CalculateCooling(IMyThrust block, float availableHeatToSink)
        {
            return Utilities.GetHeatSinkLogic(block.CubeGrid)?.ActiveCooling(availableHeatToSink) ?? 0;
        }

        private float CalculateHeating(IMyThrust block)
        {
            if (!block.IsWorking || !block.IsPlayerOwned())
                return 0;
            Logger.Instance.LogDebug($"{block.CustomName} is {(block.Enabled ? "Enabled" : "Disabled")}");
            Logger.Instance.LogDebug($"{block.CustomName} heating: (currentOutput {block.CurrentThrust/thrustDivisor}) - (passiveCooling {passiveCooling})");
            return (block.CurrentThrust/thrustDivisor) - passiveCooling;
        }

        public void AppendCustomThermalInfo(IMyThrust block, StringBuilder customInfo)
        {
            var debugInfo = new StringBuilder();
            debugInfo.Append($"DEBUG INFO - {block.CustomName}:\n");
            debugInfo.Append($"Current Heat: {CurrentHeat}\n");
            Logger.Instance.LogDebug(debugInfo.ToString());

            customInfo.Append($"Current Heat Level: {CurrentHeat}\n");
        }

        public static void SaveData(long entityId, ThrusterHeatData data)
        {
            if (data == null) return;
            try
            {
                var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage($"{entityId}.xml", typeof(ThrusterHeatData));
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(data));
                writer.Flush();
                writer.Close();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Failed to save data: {e.Message}");
            }
        }

        public static ThrusterHeatData LoadData(IMyThrust block)
        {
            var file = $"{block.EntityId}.xml";
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(file, typeof(ThrusterHeatData)))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(file, typeof(ThrusterHeatData));
                    string content = reader.ReadToEnd();
                    reader.Close();
                    return MyAPIGateway.Utilities.SerializeFromXML<ThrusterHeatData>(content);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Failed to load data: {e.Message}");
            }

            return new ThrusterHeatData();
        }
    }
    public class PowerPlantHeatData
    {
        public float CurrentHeat { get; set; }
        public float HeatCapacity { get; set; }
        public float PassiveCooling { get; set; }
        public float lastHeatDelta;
        public float HeatRatio => (CurrentHeat / HeatCapacity);
        public int overHeatCycles { get; set; }
        private float availableHeatCapacity => HeatCapacity - CurrentHeat;

        public static void SaveData(long entityId, PowerPlantHeatData data)
        {
            try
            {
                var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage($"{entityId}.xml", typeof(PowerPlantHeatData));
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(data));
                writer.Flush();
                writer.Close();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Failed to save data: {e.Message}");
            }
        }

        public static PowerPlantHeatData LoadData(IMyPowerProducer block)
        {
            var file = $"{block.EntityId}.xml";
            var heatData = new PowerPlantHeatData();
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(file, typeof(PowerPlantHeatData)))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(file, typeof(PowerPlantHeatData));
                    string content = reader.ReadToEnd();
                    reader.Close();
                    heatData = MyAPIGateway.Utilities.SerializeFromXML<PowerPlantHeatData>(content);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Failed to load data: {e.Message}");
            }

            LoadConfigFileValues(ref heatData, block.BlockDefinition.SubtypeId);
            return heatData;
        }

        public static void LoadConfigFileValues(ref PowerPlantHeatData data, string subTypeId)
        {
            if (!Configuration.BlockSettings.ContainsKey(subTypeId)) return;
            float heatCapacity;
            float passiveCooling;
            if(Configuration.TryGetValue(subTypeId, "HeatCapacity", out heatCapacity))
                data.HeatCapacity = heatCapacity;
            if (Configuration.TryGetValue(subTypeId, "PassiveCooling", out passiveCooling))
                data.PassiveCooling = passiveCooling;
        }

        private float CalculateCooling(IMyPowerProducer block, float availableHeatToSink)
        {
            return Utilities.GetHeatSinkLogic(block.CubeGrid)?.ActiveCooling(availableHeatToSink) ?? 0;
        }

        public float FeedHeatBack(float incomingHeat)
        {
            var acceptedHeat = Math.Min(incomingHeat, availableHeatCapacity);
            CurrentHeat += acceptedHeat;
            return acceptedHeat;
        }

        private float CalculateHeating(IMyPowerProducer block)
        {
            if (!block.IsWorking || !block.IsPlayerOwned())
                return 0;
            Logger.Instance.LogDebug($"{block.CustomName} is {(block.Enabled ? "Enabled" : "Disabled")}");
            Logger.Instance.LogDebug($"{block.CustomName} heating: (currentOutput {block.CurrentOutput}) - (passiveCooling {PassiveCooling})");
            var powerProducers = new List<IMyPowerProducer>();
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid);
            gts.GetBlocksOfType(powerProducers, x => x.IsWorking && x.IsSameConstructAs(block));

            var additionalGeneratorCount = Math.Min(powerProducers.Count - 1, 0);
            var spamPenalty = 1 + (additionalGeneratorCount / 100);
            return block.CurrentOutput*spamPenalty - PassiveCooling;
        }

        public void ApplyHeating(IMyPowerProducer block)
        {
            var heatGenerated = CalculateHeating(block);
            var heatDissipated = CalculateCooling(block, CurrentHeat + heatGenerated);
            lastHeatDelta = heatGenerated - heatDissipated;
            CurrentHeat += lastHeatDelta;
            if (CurrentHeat < 0)
                CurrentHeat = 0;

            if (CurrentHeat >= HeatCapacity)
            {
                WarnPlayer(block, $"Taking damage due to overheating!");
                overHeatCycles++;
                var thermalFatigue = CurrentHeat + (CurrentHeat * (overHeatCycles/10));
                block.SlimBlock.DoDamage((thermalFatigue - HeatCapacity), MyStringHash.GetOrCompute("Overheating"), true);
                if (block.IsFunctional)
                    CurrentHeat = HeatCapacity;
                else
                {
                    WarnPlayer(block, "Disabled due to heat damage.");
                    CurrentHeat = 0;
                }
            }
            else if (CurrentHeat > HeatCapacity * .8)
            {
                block.SetDamageEffect(true);
                WarnPlayer(block, $"Approaching heat threshold.");
            }
            else
                block.SetDamageEffect(false);
        }

        private const int messageDelay = 10;
        private int messageAttemptCounter = 0;
        private void WarnPlayer(IMyPowerProducer block, string message)
        {
            if (block.OwnerId != MyAPIGateway.Session.Player.IdentityId)
                return;
            if (messageAttemptCounter == 0)
            {
                MyAPIGateway.Utilities.ShowMessage("Thermal Monitoring System", $"{block.CubeGrid.CustomName}-{block.CustomName}: {message}");
            }

            messageAttemptCounter++;
            if (messageAttemptCounter == messageDelay)
                messageAttemptCounter = 0;
        }

        public void AppendCustomThermalInfo(IMyPowerProducer block, StringBuilder customInfo)
        {
            var remainingSeconds = RemainingSeconds();

            var debugInfo = new StringBuilder();
            debugInfo.Append($"DEBUG INFO - {block.CustomName}:\n");
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
