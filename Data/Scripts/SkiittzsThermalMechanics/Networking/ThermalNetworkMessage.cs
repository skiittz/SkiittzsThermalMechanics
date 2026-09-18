using System.Collections.Generic;
using ProtoBuf;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Networking
{
    public enum ThermalMessageType
    {
        FullSyncRequest = 1,
        StateUpdate = 2,
        PlayerCommand = 3,
        ChatMessage = 4,
        PlayerSettings = 5,
        RadiatorColorRequest = 6,
        StateRequest = 7
    }

    public enum ThermalBlockKind
    {
        Battery = 1,
        Reactor = 2,
        HydrogenEngine = 3,
        HydrogenThruster = 4,
        HeatSink = 5,
        Radiator = 6
    }

    [ProtoContract]
    public sealed class ThermalNetworkMessage
    {
        public const int CurrentProtocolVersion = 1;

        [ProtoMember(1)] public int ProtocolVersion { get; set; } = CurrentProtocolVersion;
        [ProtoMember(2)] public ThermalMessageType Type { get; set; }
        [ProtoMember(3)] public ThermalBlockState State { get; set; }
        [ProtoMember(4)] public string Command { get; set; }
        [ProtoMember(5)] public List<string> Arguments { get; set; }
        [ProtoMember(6)] public string Author { get; set; }
        [ProtoMember(7)] public string Text { get; set; }
        [ProtoMember(8)] public ThermalPlayerSettings PlayerSettings { get; set; }
        [ProtoMember(9)] public long EntityId { get; set; }
        [ProtoMember(10)] public bool IsMinimumColor { get; set; }
        [ProtoMember(11)] public uint PackedColor { get; set; }
        [ProtoMember(12)] public List<ThermalBlockState> States { get; set; }
    }

    [ProtoContract]
    public sealed class ThermalBlockState
    {
        [ProtoMember(1)] public long EntityId { get; set; }
        [ProtoMember(2)] public ThermalBlockKind Kind { get; set; }
        [ProtoMember(3)] public float CurrentHeat { get; set; }
        [ProtoMember(4)] public float HeatCapacity { get; set; }
        [ProtoMember(5)] public float LastHeatDelta { get; set; }
        [ProtoMember(6)] public int OverheatCycles { get; set; }
        [ProtoMember(7)] public bool IsUnknownSubtype { get; set; }
        [ProtoMember(8)] public float VentingHeat { get; set; }
        [ProtoMember(9)] public bool IsSmallGrid { get; set; }
        [ProtoMember(10)] public bool ShuntToParent { get; set; }
        [ProtoMember(11)] public float SignalRadius { get; set; }
        [ProtoMember(12)] public float SignalDecay { get; set; }
        [ProtoMember(13)] public float CurrentDissipation { get; set; }
        [ProtoMember(14)] public float MaxDissipation { get; set; }
        [ProtoMember(15)] public bool CanSeeSky { get; set; }
        [ProtoMember(16)] public uint MinimumColor { get; set; }
        [ProtoMember(17)] public uint MaximumColor { get; set; }
        [ProtoMember(18)] public float EnvironmentalMultiplier { get; set; }
        [ProtoMember(19)] public long Sequence { get; set; }

        public bool HasSameValues(ThermalBlockState other)
        {
            if (other == null)
                return false;

            return EntityId == other.EntityId
                   && Kind == other.Kind
                   && CurrentHeat.Equals(other.CurrentHeat)
                   && HeatCapacity.Equals(other.HeatCapacity)
                   && LastHeatDelta.Equals(other.LastHeatDelta)
                   && OverheatCycles == other.OverheatCycles
                   && IsUnknownSubtype == other.IsUnknownSubtype
                   && VentingHeat.Equals(other.VentingHeat)
                   && IsSmallGrid == other.IsSmallGrid
                   && ShuntToParent == other.ShuntToParent
                   && SignalRadius.Equals(other.SignalRadius)
                   && SignalDecay.Equals(other.SignalDecay)
                   && CurrentDissipation.Equals(other.CurrentDissipation)
                   && MaxDissipation.Equals(other.MaxDissipation)
                   && CanSeeSky == other.CanSeeSky
                   && MinimumColor == other.MinimumColor
                   && MaximumColor == other.MaximumColor
                   && EnvironmentalMultiplier.Equals(other.EnvironmentalMultiplier);
        }
    }

    [ProtoContract]
    public sealed class ThermalPlayerSettings
    {
        [ProtoMember(1)] public string ChatBotName { get; set; }
        [ProtoMember(2)] public bool HudDisabled { get; set; }
        [ProtoMember(3)] public bool MessagesDisabled { get; set; }
        [ProtoMember(4)] public bool TutorialMessagesDisabled { get; set; }
        [ProtoMember(5)] public bool DebugMode { get; set; }
    }
}
