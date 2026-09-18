using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Networking;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core
{
    public interface IThermalStateProvider
    {
        long EntityId { get; }
        ThermalBlockKind StateKind { get; }
        IMyTerminalBlock TerminalBlock { get; }
        bool HasAuthoritativeState { get; }
        ThermalBlockState CaptureState();
        void ApplyState(ThermalBlockState state);
        void SaveAuthoritativeState();
        void ReloadAuthoritativeConfiguration();
    }

    public interface IRadiatorStateProvider : IThermalStateProvider
    {
        bool SetColorFromPlayer(long playerIdentityId, bool isMinimumColor, uint packedColor);
    }

    public static class ThermalAuthority
    {
        private const int MaximumPendingStates = 4096;
        private static readonly Dictionary<long, IThermalStateProvider> Providers =
            new Dictionary<long, IThermalStateProvider>();
        private static readonly Dictionary<long, ThermalBlockState> PendingClientStates =
            new Dictionary<long, ThermalBlockState>();
        private static readonly Dictionary<long, ThermalBlockState> LastSentStates =
            new Dictionary<long, ThermalBlockState>();
        private static readonly Dictionary<long, DateTime> LastStateHeartbeats =
            new Dictionary<long, DateTime>();
        private static readonly Dictionary<long, long> LastAppliedSequences = new Dictionary<long, long>();
        private static long _nextStateSequence;

        public static bool IsServer
        {
            get
            {
                if (MyAPIGateway.Multiplayer != null)
                    return MyAPIGateway.Multiplayer.IsServer;
                return MyAPIGateway.Session == null || MyAPIGateway.Session.IsServer;
            }
        }

        public static void Register(IThermalStateProvider provider)
        {
            if (provider == null || provider.EntityId == 0)
                return;

            Providers[provider.EntityId] = provider;

            if (IsServer)
            {
                Sync(provider, true);
                return;
            }

            ThermalBlockState pendingState;
            if (PendingClientStates.TryGetValue(provider.EntityId, out pendingState))
            {
                provider.ApplyState(pendingState);
                PendingClientStates.Remove(provider.EntityId);
                return;
            }

            if (provider.TerminalBlock != null && provider.TerminalBlock.CubeGrid != null)
                ThermalNetwork.RequestGridState(provider.TerminalBlock.CubeGrid.EntityId);
        }

        public static void Unregister(long entityId)
        {
            IThermalStateProvider provider;
            if (!IsServer && Providers.TryGetValue(entityId, out provider) && provider.TerminalBlock != null
                && provider.TerminalBlock.CubeGrid != null)
                ThermalNetwork.ForgetClientGrid(provider.TerminalBlock.CubeGrid.EntityId);

            Providers.Remove(entityId);
            PendingClientStates.Remove(entityId);
            LastSentStates.Remove(entityId);
            LastStateHeartbeats.Remove(entityId);
            LastAppliedSequences.Remove(entityId);
            ThermalNetwork.DiscardQueuedState(entityId);
        }

        public static void Sync(IThermalStateProvider provider, bool reliable = false)
        {
            if (!IsServer || provider == null)
                return;

            var state = CaptureSequencedState(provider);
            if (state == null)
                return;

            ThermalBlockState lastState;
            DateTime lastHeartbeat;
            var now = DateTime.UtcNow;
            var heartbeatDue = !LastStateHeartbeats.TryGetValue(provider.EntityId, out lastHeartbeat)
                               || (now - lastHeartbeat).TotalSeconds >= 15;
            if (!reliable && !heartbeatDue && LastSentStates.TryGetValue(provider.EntityId, out lastState)
                && state.HasSameValues(lastState))
                return;

            LastSentStates[provider.EntityId] = state;
            LastStateHeartbeats[provider.EntityId] = now;
            ThermalNetwork.QueueStateForRelevantPlayers(provider, state, reliable);
        }

        public static ThermalBlockState CaptureSequencedState(IThermalStateProvider provider)
        {
            if (!IsServer || provider == null)
                return null;

            var state = provider.CaptureState();
            if (state != null)
                state.Sequence = ++_nextStateSequence;
            return state;
        }

        public static void SendFullSyncTo(ulong recipientSteamId)
        {
            if (!IsServer)
                return;

            ThermalNetwork.SendProviderStatesToPlayer(recipientSteamId, Providers.Values.ToList());
        }

        public static void SendGridStateTo(ulong recipientSteamId, long gridEntityId)
        {
            if (!IsServer || gridEntityId == 0)
                return;

            var matchingProviders = Providers.Values.Where(provider => provider.TerminalBlock != null
                && provider.TerminalBlock.CubeGrid != null
                && provider.TerminalBlock.CubeGrid.EntityId == gridEntityId).ToList();
            ThermalNetwork.SendProviderStatesToPlayer(recipientSteamId, matchingProviders);
        }

        public static void ApplyClientState(ThermalBlockState state)
        {
            if (IsServer || state == null || state.EntityId == 0)
                return;

            long lastSequence;
            if (state.Sequence > 0 && LastAppliedSequences.TryGetValue(state.EntityId, out lastSequence)
                && state.Sequence <= lastSequence)
                return;
            if (state.Sequence > 0)
                LastAppliedSequences[state.EntityId] = state.Sequence;

            IThermalStateProvider provider;
            if (Providers.TryGetValue(state.EntityId, out provider))
            {
                if (provider.StateKind == state.Kind)
                    provider.ApplyState(state);
                return;
            }

            if (PendingClientStates.Count >= MaximumPendingStates)
                PendingClientStates.Clear();
            PendingClientStates[state.EntityId] = state;
        }

        public static bool TrySetRadiatorColor(long entityId, long playerIdentityId, bool isMinimumColor, uint packedColor)
        {
            if (!IsServer)
                return false;

            IThermalStateProvider provider;
            if (!Providers.TryGetValue(entityId, out provider))
                return false;

            var radiator = provider as IRadiatorStateProvider;
            if (radiator == null || !radiator.SetColorFromPlayer(playerIdentityId, isMinimumColor, packedColor))
                return false;

            Sync(provider, true);
            return true;
        }

        public static void SaveAll()
        {
            if (!IsServer)
                return;

            foreach (var provider in Providers.Values.ToList())
            {
                try
                {
                    provider.SaveAuthoritativeState();
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine($"SkiittzsThermalMechanics: Failed to save entity {provider.EntityId}: {exception}");
                }
            }
        }

        public static void ReloadAllConfigurations()
        {
            if (!IsServer)
                return;

            foreach (var provider in Providers.Values.ToList())
            {
                try
                {
                    provider.ReloadAuthoritativeConfiguration();
                    Sync(provider, true);
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine($"SkiittzsThermalMechanics: Failed to reload entity {provider.EntityId}: {exception}");
                }
            }
        }

        public static void Reset()
        {
            Providers.Clear();
            PendingClientStates.Clear();
            LastSentStates.Clear();
            LastStateHeartbeats.Clear();
            LastAppliedSequences.Clear();
            _nextStateSequence = 0;
        }
    }
}
