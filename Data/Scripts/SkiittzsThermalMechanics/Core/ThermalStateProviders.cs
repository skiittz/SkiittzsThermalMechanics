using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Networking;
using VRageMath;
using BatteryStateProvider = SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Battery.BatteryLogic;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Battery
{
    public partial class BatteryLogic : IThermalStateProvider
    {
        public long EntityId => block == null ? 0 : block.EntityId;
        public ThermalBlockKind StateKind => ThermalBlockKind.Battery;
        public IMyTerminalBlock TerminalBlock => block as IMyTerminalBlock;
        public bool HasAuthoritativeState => hasAuthoritativeState;

        public ThermalBlockState CaptureState()
        {
            return !hasAuthoritativeState || heatData == null ? null : PowerPlantState(EntityId, StateKind, heatData);
        }

        public void ApplyState(ThermalBlockState state)
        {
            if (state == null || state.Kind != StateKind || heatData == null)
                return;
            hasAuthoritativeState = true;
            ApplyPowerPlantState(state, heatData);
            TerminalBlock?.RefreshCustomInfo();
        }

        public void SaveAuthoritativeState()
        {
            if (block != null && heatData != null)
                PowerPlantHeatData.SaveData(block.EntityId, heatData);
        }

        public void ReloadAuthoritativeConfiguration()
        {
            ReloadPowerPlantConfiguration(block, heatData, "Battery");
        }

        internal static ThermalBlockState PowerPlantState(long entityId, ThermalBlockKind kind,
            PowerPlantHeatData data)
        {
            return new ThermalBlockState
            {
                EntityId = entityId,
                Kind = kind,
                CurrentHeat = data.CurrentHeat,
                HeatCapacity = data.HeatCapacity,
                LastHeatDelta = data.LastHeatDelta,
                OverheatCycles = data.OverHeatCycles,
                IsUnknownSubtype = data.IsUnknownSubType
            };
        }

        internal static void ApplyPowerPlantState(ThermalBlockState state, PowerPlantHeatData data)
        {
            data.CurrentHeat = state.CurrentHeat;
            data.HeatCapacity = state.HeatCapacity;
            data.LastHeatDelta = state.LastHeatDelta;
            data.OverHeatCycles = state.OverheatCycles;
            data.IsUnknownSubType = state.IsUnknownSubtype;
        }

        internal static void ReloadPowerPlantConfiguration(IMyPowerProducer powerBlock, PowerPlantHeatData data,
            string defaultType)
        {
            if (powerBlock == null || data == null)
                return;

            bool configFound;
            data.IsUnknownSubType = false;
            PowerPlantHeatData.LoadConfigFileValues(ref data, powerBlock.BlockDefinition.SubtypeId, out configFound);
            if (!configFound)
            {
                data.IsUnknownSubType = true;
                PowerPlantHeatData.LoadConfigFileValues(ref data, powerBlock.DefaultId(defaultType), out configFound);
            }
        }
    }
}

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Reactor
{
    public partial class ReactorLogic : IThermalStateProvider
    {
        public long EntityId => block == null ? 0 : block.EntityId;
        public ThermalBlockKind StateKind => ThermalBlockKind.Reactor;
        public IMyTerminalBlock TerminalBlock => block as IMyTerminalBlock;
        public bool HasAuthoritativeState => hasAuthoritativeState;

        public ThermalBlockState CaptureState()
        {
            return !hasAuthoritativeState || heatData == null
                ? null
                : BatteryStateProvider.PowerPlantState(EntityId, StateKind, heatData);
        }

        public void ApplyState(ThermalBlockState state)
        {
            if (state == null || state.Kind != StateKind || heatData == null)
                return;
            hasAuthoritativeState = true;
            BatteryStateProvider.ApplyPowerPlantState(state, heatData);
            TerminalBlock?.RefreshCustomInfo();
        }

        public void SaveAuthoritativeState()
        {
            if (block != null && heatData != null)
                PowerPlantHeatData.SaveData(block.EntityId, heatData);
        }

        public void ReloadAuthoritativeConfiguration()
        {
            BatteryStateProvider.ReloadPowerPlantConfiguration(block, heatData, "Reactor");
        }
    }
}

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.H2Generator
{
    public partial class H2EngineLogic : IThermalStateProvider
    {
        public long EntityId => block == null ? 0 : block.EntityId;
        public ThermalBlockKind StateKind => ThermalBlockKind.HydrogenEngine;
        public IMyTerminalBlock TerminalBlock => block as IMyTerminalBlock;
        public bool HasAuthoritativeState => hasAuthoritativeState;

        public ThermalBlockState CaptureState()
        {
            return !hasAuthoritativeState || heatData == null
                ? null
                : BatteryStateProvider.PowerPlantState(EntityId, StateKind, heatData);
        }

        public void ApplyState(ThermalBlockState state)
        {
            if (state == null || state.Kind != StateKind || heatData == null)
                return;
            hasAuthoritativeState = true;
            BatteryStateProvider.ApplyPowerPlantState(state, heatData);
            TerminalBlock?.RefreshCustomInfo();
        }

        public void SaveAuthoritativeState()
        {
            if (block != null && heatData != null)
                PowerPlantHeatData.SaveData(block.EntityId, heatData);
        }

        public void ReloadAuthoritativeConfiguration()
        {
            BatteryStateProvider.ReloadPowerPlantConfiguration(block, heatData, "H2Engine");
        }
    }
}

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.H2Thruster
{
    public partial class HydrogenThrusterLogic : IThermalStateProvider
    {
        public long EntityId => block == null ? 0 : block.EntityId;
        public ThermalBlockKind StateKind => ThermalBlockKind.HydrogenThruster;
        public IMyTerminalBlock TerminalBlock => block as IMyTerminalBlock;
        public bool HasAuthoritativeState => hasAuthoritativeState;

        public ThermalBlockState CaptureState()
        {
            if (!hasAuthoritativeState || heatData == null)
                return null;

            return new ThermalBlockState
            {
                EntityId = EntityId,
                Kind = StateKind,
                CurrentHeat = heatData.CurrentHeat,
                LastHeatDelta = heatData.LastHeatDelta
            };
        }

        public void ApplyState(ThermalBlockState state)
        {
            if (state == null || state.Kind != StateKind || heatData == null)
                return;
            hasAuthoritativeState = true;
            heatData.CurrentHeat = state.CurrentHeat;
            heatData.LastHeatDelta = state.LastHeatDelta;
            TerminalBlock?.RefreshCustomInfo();
        }

        public void SaveAuthoritativeState()
        {
            if (block != null && heatData != null)
                ThrusterHeatData.SaveData(block.EntityId, heatData);
        }

        public void ReloadAuthoritativeConfiguration()
        {
            if (block == null || heatData == null)
                return;
            bool configFound;
            ThrusterHeatData.LoadConfigFileValues(ref heatData, block.BlockDefinition.SubtypeId, out configFound);
        }
    }
}

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.HeatSink
{
    public partial class HeatSinkLogic : IThermalStateProvider
    {
        public long EntityId => block == null ? 0 : block.EntityId;
        public ThermalBlockKind StateKind => ThermalBlockKind.HeatSink;
        public IMyTerminalBlock TerminalBlock => block as IMyTerminalBlock;
        public bool HasAuthoritativeState => hasAuthoritativeState;

        public ThermalBlockState CaptureState()
        {
            if (!hasAuthoritativeState || HeatSinkData == null)
                return null;

            return new ThermalBlockState
            {
                EntityId = EntityId,
                Kind = StateKind,
                CurrentHeat = HeatSinkData.CurrentHeat,
                HeatCapacity = HeatSinkData.HeatCapacity,
                VentingHeat = HeatSinkData.VentingHeat,
                IsSmallGrid = HeatSinkData.IsSmallGrid,
                ShuntToParent = HeatSinkData.ShuntToParent,
                SignalRadius = HeatSinkData.SignalRadius,
                SignalDecay = HeatSinkData.SignalDecay,
                EnvironmentalMultiplier = signalMult
            };
        }

        public void ApplyState(ThermalBlockState state)
        {
            if (state == null || state.Kind != StateKind || HeatSinkData == null)
                return;

            hasAuthoritativeState = true;

            HeatSinkData.CurrentHeat = state.CurrentHeat;
            HeatSinkData.HeatCapacity = state.HeatCapacity;
            HeatSinkData.VentingHeat = state.VentingHeat;
            HeatSinkData.IsSmallGrid = state.IsSmallGrid;
            HeatSinkData.ShuntToParent = state.ShuntToParent;
            HeatSinkData.SignalRadius = state.SignalRadius;
            HeatSinkData.SignalDecay = state.SignalDecay;
            signalMult = state.EnvironmentalMultiplier;
            TerminalBlock?.RefreshCustomInfo();
        }

        public void SaveAuthoritativeState()
        {
            if (block != null && HeatSinkData != null)
                HeatSinkData.SaveData(block.EntityId, HeatSinkData);
        }

        public void ReloadAuthoritativeConfiguration()
        {
            if (block == null || HeatSinkData == null)
                return;

            bool configFound;
            HeatSinkData.LoadConfigFileValues(ref HeatSinkData, block.BlockDefinition.SubtypeId, out configFound);
            bool shuntToParent;
            if (Configuration.Configuration.TryGetGeneralSettingValue("SmallGridShuntsToLarge", out shuntToParent))
                HeatSinkData.ShuntToParent = shuntToParent;
        }
    }
}

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Radiator
{
    public partial class HeatRadiatorLogic : IRadiatorStateProvider
    {
        public long EntityId => block == null ? 0 : block.EntityId;
        public ThermalBlockKind StateKind => ThermalBlockKind.Radiator;
        public IMyTerminalBlock TerminalBlock => block as IMyTerminalBlock;
        public bool HasAuthoritativeState => hasAuthoritativeState;

        public ThermalBlockState CaptureState()
        {
            if (!hasAuthoritativeState || radiatorData == null)
                return null;

            return new ThermalBlockState
            {
                EntityId = EntityId,
                Kind = StateKind,
                CurrentDissipation = radiatorData.CurrentDissipation,
                MaxDissipation = radiatorData.MaxDissipation,
                CanSeeSky = radiatorData.CanSeeSky,
                MinimumColor = radiatorData.MinColor.PackedValue,
                MaximumColor = radiatorData.MaxColor.PackedValue,
                EnvironmentalMultiplier = dissipationMult
            };
        }

        public void ApplyState(ThermalBlockState state)
        {
            if (state == null || state.Kind != StateKind || radiatorData == null)
                return;

            hasAuthoritativeState = true;
            radiatorData.CurrentDissipation = state.CurrentDissipation;
            radiatorData.MaxDissipation = state.MaxDissipation;
            radiatorData.CanSeeSky = state.CanSeeSky;
            radiatorData.MinColor = FromPackedColor(state.MinimumColor);
            radiatorData.MaxColor = FromPackedColor(state.MaximumColor);
            dissipationMult = state.EnvironmentalMultiplier;
            Animate();
            TerminalBlock?.RefreshCustomInfo();
        }

        public void SaveAuthoritativeState()
        {
            if (block != null && radiatorData != null)
                RadiatorData.SaveData(block.EntityId, radiatorData);
        }

        public void ReloadAuthoritativeConfiguration()
        {
            if (block == null || radiatorData == null)
                return;
            bool configFound;
            RadiatorData.LoadConfigFileValues(ref radiatorData, block.BlockDefinition.SubtypeId, out configFound);
        }

        public bool SetColorFromPlayer(long playerIdentityId, bool isMinimumColor, uint packedColor)
        {
            if (!hasAuthoritativeState || block == null || radiatorData == null || playerIdentityId == 0
                || !block.HasPlayerAccessWithNobodyCheck(playerIdentityId))
                return false;

            if (isMinimumColor)
                radiatorData.MinColor = FromPackedColor(packedColor);
            else
                radiatorData.MaxColor = FromPackedColor(packedColor);
            if (MyAPIGateway.Utilities == null || !MyAPIGateway.Utilities.IsDedicated)
                Animate();
            TerminalBlock?.RefreshCustomInfo();
            return true;
        }

        private static Color FromPackedColor(uint packedColor)
        {
            var color = new Color();
            color.PackedValue = packedColor;
            return color;
        }
    }
}
