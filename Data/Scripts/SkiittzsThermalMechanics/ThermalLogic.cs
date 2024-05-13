using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
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
            return Utilities.GetBeaconLogic(Block.CubeGrid)?.ActiveCooling(availableHeatToSink) ?? 0;
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
        private MyParticleEffect fireEffect;
        public PowerPlantHeatData(IMyPowerProducer block, float heatCapacity, float passiveCooling)
        {
            Block = block;
            this.HeatCapacity = heatCapacity;
            this.passiveCooling = passiveCooling;
        }

        private float CalculateCooling(float availableHeatToSink)
        {
            return Utilities.GetBeaconLogic(Block.CubeGrid)?.ActiveCooling(availableHeatToSink) ?? 0;
        }

        private float CalculateHeating()
        {
            if (!Block.IsWorking)
                return 0;
            Logger.Instance.LogDebug($"{Block.CustomName} is {(Block.Enabled ? "Enabled" : "Disabled")}");
            Logger.Instance.LogDebug($"{Block.CustomName} heating: (currentOutput {Block.CurrentOutput}) - (passiveCooling {passiveCooling})");
            return Block.CurrentOutput - passiveCooling;
        }

        MatrixD m_LocalOffset = new MatrixD();
        public void ApplyHeating()
        {
            lastHeatDelta = CalculateHeating();
            CurrentHeat += lastHeatDelta;
            CurrentHeat -= CalculateCooling(CurrentHeat);
            if (CurrentHeat < 0)
                CurrentHeat = 0;

            if (CurrentHeat <= HeatCapacity * .8)
            {
                if (fireEffect != null)
                {
                    fireEffect.Stop(true);
                    fireEffect = null;
                }
                return;
            }

            if (CurrentHeat <= HeatCapacity)
            {
                if (fireEffect == null && MyParticlesManager.TryCreateParticleEffect(8, out fireEffect, false))
                //m_LocalOffset = MatrixD.CreateTranslation(0.85f * Block.PositionComp.LocalVolume.Center);//<1 because 100% of offset would bury the effect inside wall
                //fireEffect.WorldMatrix = m_LocalOffset * Block.WorldMatrix;
                //if (fireEffect == null && MyParticlesManager.TryCreateParticleEffect(8, out fireEffect, ref m_LocalOffset, ref ????, Block.Render.GetRenderObjectID(), false))
                {
                    fireEffect.WorldMatrix = MatrixD.CreateTranslation(0.85f * Block.PositionComp.LocalVolume.Center);
                    fireEffect.Autodelete = false;
                    fireEffect.UserScale = Block.Model.BoundingBox.Perimeter * .018f;
                }
            }

            Utilities.GetBeaconLogic(Block.CubeGrid)?.RemoveHeatDueToBlockDeath(HeatCapacity);
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
            return ((lastHeatDelta > 0 ? HeatCapacity - CurrentHeat : CurrentHeat) / (lastHeatDelta == 0 ? 1 : lastHeatDelta)) * 1.667f;
        }

        private static string TimeUntilOverheatDisplay(float remainingSeconds)
        {
            Logger.Instance.LogDebug($"Display Timespan for remaining seconds: {remainingSeconds}");
            var timeSpan = TimeSpan.FromSeconds(remainingSeconds);

            return timeSpan.ToString("hh\\:mm\\:ss");
        }
    }
}
