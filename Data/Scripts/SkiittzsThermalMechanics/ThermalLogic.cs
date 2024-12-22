using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public class ThrusterHeatData
    {
        public float CurrentHeat { get; set; }
        [XmlIgnore]
        public float PassiveCooling { get; set; }
        public float LastHeatDelta { get; set; }
        [XmlIgnore]
        public float MwHeatPerNewtonThrust { get; set; }

        public void ApplyHeating(IMyThrust block)
        {
            LastHeatDelta = CalculateHeating(block);
            CurrentHeat += LastHeatDelta;
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
            if (!block.IsWorking || !block.IsOwnedByAPlayer())
                return 0;
            return (block.CurrentThrust * MwHeatPerNewtonThrust) - PassiveCooling;
        }

        public void AppendCustomThermalInfo(IMyThrust block, StringBuilder customInfo)
        {
            var debugInfo = new StringBuilder();
            debugInfo.Append($"DEBUG INFO - {block.CustomName}:\n");
            debugInfo.Append($"Current Heat: {CurrentHeat}\n");

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
            var data = new ThrusterHeatData();
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(file, typeof(ThrusterHeatData)))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(file, typeof(ThrusterHeatData));
                    string content = reader.ReadToEnd();
                    reader.Close();
                    data = MyAPIGateway.Utilities.SerializeFromXML<ThrusterHeatData>(content);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Failed to load data: {e.Message}");
            }

            LoadConfigFileValues(ref data, block.BlockDefinition.SubtypeId);
            return data;
        }

        public static void LoadConfigFileValues(ref ThrusterHeatData data, string subTypeId)
        {
            if (!Configuration.BlockSettings.ContainsKey(subTypeId)) return;
            float mwHeatPerNewtonThrust;
            float passiveCooling;
            if (Configuration.TryGetValue(subTypeId, "MwHeatPerNewtonThrust", out mwHeatPerNewtonThrust))
                data.MwHeatPerNewtonThrust = mwHeatPerNewtonThrust;
            if (Configuration.TryGetValue(subTypeId, "PassiveCooling", out passiveCooling))
                data.PassiveCooling = passiveCooling;
        }
    }
    public class PowerPlantHeatData
    {
        public float CurrentHeat { get; set; }
        [XmlIgnore]
        public float HeatCapacity { get; set; }
        [XmlIgnore]
        public float PassiveCooling { get; set; }
        public float LastHeatDelta;
        public float HeatRatio => (CurrentHeat / HeatCapacity);
        public int OverHeatCycles { get; set; }
        public float ThermalFatigue => 1+(OverHeatCycles / 100);
        public float AvailableHeatCapacity => HeatCapacity - CurrentHeat;

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
            if (Configuration.TryGetValue(subTypeId, "HeatCapacity", out heatCapacity))
                data.HeatCapacity = heatCapacity;
            if (Configuration.TryGetValue(subTypeId, "PassiveCooling", out passiveCooling))
                data.PassiveCooling = passiveCooling;
        }

        private float CalculateCooling(IMyPowerProducer block, float availableHeatToSink)
        {
            return Utilities.GetHeatSinkLogic(block.CubeGrid)?.ActiveCooling(availableHeatToSink) ?? 0;
        }

        public float FeedHeatBack(float incomingHeat, bool allowOverheat = false)
        {
	        if (incomingHeat < 0)
		        return 0;

            var acceptedHeat = Math.Min(incomingHeat, (AvailableHeatCapacity * (allowOverheat ? 1.5f : 1)));
            CurrentHeat += acceptedHeat;
            return acceptedHeat;
        }

        private float CalculateHeating(IMyPowerProducer block)
        {
            if (!block.IsWorking || !block.IsOwnedByAPlayer())
                return 0;

            var powerProducers = new List<IMyPowerProducer>();
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid);
            gts.GetBlocksOfType(powerProducers, x => x.IsWorking && x.IsSameConstructAs(block));

            var additionalGeneratorCount = Math.Min(powerProducers.Count - 1, 0);
            var spamPenalty = 1 + (additionalGeneratorCount / 100);
            if(spamPenalty > 1.1)
                ChatBot.WarnPlayer(block, $"Wow that's a lot of power plants!  Did you know that spamming generators will incur a penalty?  You are currently generating {additionalGeneratorCount}% more  heat than you'd otherwise be.  It's better to use fewer, more powerful power plants.", MessageSeverity.Tutorial);
            return (block.CurrentOutput * spamPenalty) - PassiveCooling;
        }

        public void ApplyHeating(IMyPowerProducer block)
        {
            if (block == null || !block.IsOwnedByAPlayer()) return;
            var heatGenerated = CalculateHeating(block);
            var heatDissipated = CalculateCooling(block, CurrentHeat + heatGenerated);
            LastHeatDelta = heatGenerated - heatDissipated;
            CurrentHeat += LastHeatDelta;
            if (CurrentHeat < 0)
                CurrentHeat = 0;

            if (CurrentHeat >= HeatCapacity)
            {
                ChatBot.WarnPlayer(block, $"Taking damage due to overheating!", MessageSeverity.Warning);
                OverHeatCycles++;
                if(OverHeatCycles > 10)
                    ChatBot.WarnPlayer(block, $"Due to repeated overheating, this block is suffering from thermal fatigue.  It will generate more heat than usual.  This effect is permanent.", MessageSeverity.Tutorial);
                var thermalFatigue = CurrentHeat + (CurrentHeat * ThermalFatigue);
                block.SlimBlock.DoDamage((thermalFatigue - HeatCapacity), MyStringHash.GetOrCompute("Overheating"), true);
                if (block.IsFunctional)
                    CurrentHeat = HeatCapacity;
                else
                {
                    ChatBot.WarnPlayer(block, "Disabled due to heat damage.", MessageSeverity.Warning);
                    CurrentHeat = 0;
                }
            }
            else if (CurrentHeat > HeatCapacity * .8)
            {
                block.SetDamageEffect(true);
                ChatBot.WarnPlayer(block, $"Approaching heat threshold.", MessageSeverity.Tutorial);
            }
            else
                block.SetDamageEffect(false);
        }

        public void AppendCustomThermalInfo(IMyPowerProducer block, StringBuilder customInfo)
        {
            var remainingSeconds = RemainingSeconds();

            var debugInfo = new StringBuilder();
            debugInfo.Append($"DEBUG INFO - {block.CustomName}:\n");
            debugInfo.Append($"Current Heat: {CurrentHeat}\n");
            debugInfo.Append($"Heat Capacity: {HeatCapacity}\n");
            debugInfo.Append($"Last Heat Delta: {LastHeatDelta}\n");
            debugInfo.Append($"Remaining Seconds: {remainingSeconds}");

            customInfo.Append($"Heat Level: {(CurrentHeat / HeatCapacity) * 100}%\n");
            customInfo.Append($"Time until {(LastHeatDelta <= 0 ? "cooled" : "overheat")}: {TimeUntilOverheatDisplay(Math.Abs(remainingSeconds))}\n");

            customInfo.Append($"Heat Generation: {ThermalFatigue * 100:F0}%");
        }

        private const float SecondsPer100Ticks = 1.667f;
        private float RemainingSeconds()
        {
            var numerator = LastHeatDelta > 0 ? HeatCapacity - CurrentHeat : CurrentHeat;
            var denominator = LastHeatDelta == 0 ? 1 : Math.Abs(LastHeatDelta);
            return (numerator / denominator) * SecondsPer100Ticks;
        }

        private static string TimeUntilOverheatDisplay(float remainingSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(remainingSeconds);
            return timeSpan.ToString("hh\\:mm\\:ss");
        }
    }
}
