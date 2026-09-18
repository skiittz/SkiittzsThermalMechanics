using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.ChatBot;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Networking
{
    public static class ThermalNetwork
    {
        // Deliberately non-round to reduce the chance of colliding with another mod's channel.
        private const ushort ChannelId = 49183;
        private const int FullSyncRetryTicks = 300;
        private const int MaximumCommandLength = 64;
        private const int MaximumArgumentLength = 64;
        private const int MaximumArguments = 8;
        private const int MaximumPacketBytes = 8192;
        private const int MaximumStatesPerPacket = 16;

        private static readonly Dictionary<ulong, DateTime> LastFullSyncRequests =
            new Dictionary<ulong, DateTime>();
        private static readonly Dictionary<ulong, DateTime> LastCommandRequests =
            new Dictionary<ulong, DateTime>();
        private static readonly Dictionary<ulong, RequestRateWindow> ColorRequestWindows =
            new Dictionary<ulong, RequestRateWindow>();
        private static readonly Dictionary<ulong, RequestRateWindow> StateRequestWindows =
            new Dictionary<ulong, RequestRateWindow>();
        private static readonly Dictionary<long, QueuedState> QueuedStates =
            new Dictionary<long, QueuedState>();
        private static readonly HashSet<long> RequestedClientGrids = new HashSet<long>();
        private static bool _initialized;
        private static int _clientSyncRetryCounter;

        public static bool ClientHudDisabled { get; private set; }
        public static bool ClientDebugMode { get; private set; }
        public static bool HasReceivedPlayerSettings { get; private set; }

        public static void Initialize()
        {
            if (_initialized || MyAPIGateway.Multiplayer == null)
                return;

            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(ChannelId, OnSecureMessageReceived);
            _initialized = true;
            _clientSyncRetryCounter = 0;
        }

        public static void Update()
        {
            if (!_initialized)
                Initialize();
            if (!_initialized)
                return;

            if (ThermalAuthority.IsServer)
            {
                FlushQueuedStates();
                return;
            }

            if (HasReceivedPlayerSettings)
                return;

            _clientSyncRetryCounter++;
            if (_clientSyncRetryCounter < FullSyncRetryTicks)
                return;

            _clientSyncRetryCounter = 0;
            RequestFullSync();
        }

        public static void RequestFullSync()
        {
            if (!_initialized || ThermalAuthority.IsServer)
                return;

            SendToServer(new ThermalNetworkMessage { Type = ThermalMessageType.FullSyncRequest }, true);
        }

        public static void SendCommand(string command, string[] arguments)
        {
            var message = new ThermalNetworkMessage
            {
                Type = ThermalMessageType.PlayerCommand,
                Command = command,
                Arguments = arguments == null ? new List<string>() : new List<string>(arguments)
            };

            if (ThermalAuthority.IsServer)
            {
                HandleClientMessage(message, LocalSteamId());
                return;
            }

            SendToServer(message, true);
        }

        public static void RequestRadiatorColor(long entityId, bool isMinimumColor, uint packedColor)
        {
            var message = new ThermalNetworkMessage
            {
                Type = ThermalMessageType.RadiatorColorRequest,
                EntityId = entityId,
                IsMinimumColor = isMinimumColor,
                PackedColor = packedColor
            };

            if (ThermalAuthority.IsServer)
            {
                HandleClientMessage(message, LocalSteamId());
                return;
            }

            SendToServer(message, true);
        }

        public static void RequestGridState(long gridEntityId)
        {
            if (!_initialized || ThermalAuthority.IsServer || gridEntityId == 0
                || !RequestedClientGrids.Add(gridEntityId))
                return;

            SendToServer(new ThermalNetworkMessage
            {
                Type = ThermalMessageType.StateRequest,
                EntityId = gridEntityId
            }, true);
        }

        public static void ForgetClientGrid(long gridEntityId)
        {
            if (!ThermalAuthority.IsServer)
                RequestedClientGrids.Remove(gridEntityId);
        }

        public static void QueueStateForRelevantPlayers(IThermalStateProvider provider, ThermalBlockState state,
            bool reliable)
        {
            if (!ThermalAuthority.IsServer || provider == null || state == null)
                return;

            QueuedState queued;
            if (QueuedStates.TryGetValue(provider.EntityId, out queued))
            {
                queued.Provider = provider;
                queued.State = state;
                queued.Reliable |= reliable;
                return;
            }

            QueuedStates[provider.EntityId] = new QueuedState
            {
                Provider = provider,
                State = state,
                Reliable = reliable
            };
        }

        public static void DiscardQueuedState(long entityId)
        {
            QueuedStates.Remove(entityId);
        }

        public static void SendProviderStatesToPlayer(ulong recipientSteamId,
            IEnumerable<IThermalStateProvider> providers)
        {
            if (!ThermalAuthority.IsServer || providers == null || MyAPIGateway.Players == null)
                return;

            var identityId = MyAPIGateway.Players.TryGetIdentityId(recipientSteamId);
            var player = identityId == 0 ? null : MyAPIGateway.Players.TryGetIdentityId(identityId);
            if (player == null || player.IsBot)
                return;

            var states = new List<ThermalBlockState>();
            foreach (var provider in providers)
            {
                try
                {
                    if (provider == null || !IsPlayerRelevant(player, provider.TerminalBlock))
                        continue;
                    var state = ThermalAuthority.CaptureSequencedState(provider);
                    if (state != null)
                        states.Add(state);
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine($"SkiittzsThermalMechanics: Failed to capture sync state: {exception}");
                }
            }

            SendStateBatches(recipientSteamId, states, true);
        }

        public static void SendChatMessage(ulong recipientSteamId, string author, string text)
        {
            if (!ThermalAuthority.IsServer || string.IsNullOrEmpty(text))
                return;

            SendToPlayer(new ThermalNetworkMessage
            {
                Type = ThermalMessageType.ChatMessage,
                Author = author ?? string.Empty,
                Text = text
            }, recipientSteamId, true);
        }

        public static void SendPlayerSettings(ulong recipientSteamId, ThermalPlayerSettings settings)
        {
            if (!ThermalAuthority.IsServer || settings == null)
                return;

            SendToPlayer(new ThermalNetworkMessage
            {
                Type = ThermalMessageType.PlayerSettings,
                PlayerSettings = settings
            }, recipientSteamId, true);
        }

        public static void Unload()
        {
            if (_initialized && MyAPIGateway.Multiplayer != null)
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(ChannelId, OnSecureMessageReceived);

            _initialized = false;
            _clientSyncRetryCounter = 0;
            ClientHudDisabled = false;
            ClientDebugMode = false;
            HasReceivedPlayerSettings = false;
            LastFullSyncRequests.Clear();
            LastCommandRequests.Clear();
            ColorRequestWindows.Clear();
            StateRequestWindows.Clear();
            QueuedStates.Clear();
            RequestedClientGrids.Clear();
        }

        private static void OnSecureMessageReceived(ushort handlerId, byte[] payload, ulong senderSteamId,
            bool sentFromServer)
        {
            if (handlerId != ChannelId || payload == null || payload.Length == 0
                || payload.Length > MaximumPacketBytes)
                return;

            ThermalNetworkMessage message;
            try
            {
                message = MyAPIGateway.Utilities.SerializeFromBinary<ThermalNetworkMessage>(payload);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine($"SkiittzsThermalMechanics: Rejected malformed network packet: {exception.Message}");
                return;
            }

            if (message == null || message.ProtocolVersion != ThermalNetworkMessage.CurrentProtocolVersion)
                return;

            if (ThermalAuthority.IsServer)
            {
                // State and presentation packets are server-to-client only. A client cannot spoof one.
                if (sentFromServer)
                    return;

                MyAPIGateway.Utilities.InvokeOnGameThread(() => HandleClientMessage(message, senderSteamId));
                return;
            }

            // Secure handlers tell us whether the transport authenticated the sender as the server.
            if (!sentFromServer)
                return;

            MyAPIGateway.Utilities.InvokeOnGameThread(() => HandleServerMessage(message));
        }

        private static void HandleClientMessage(ThermalNetworkMessage message, ulong senderSteamId)
        {
            if (!ThermalAuthority.IsServer || message == null || senderSteamId == 0
                || MyAPIGateway.Players == null)
                return;

            var playerIdentityId = MyAPIGateway.Players.TryGetIdentityId(senderSteamId);
            if (playerIdentityId == 0)
                return;
            var player = MyAPIGateway.Players.TryGetIdentityId(playerIdentityId);
            if (player == null || player.IsBot)
                return;

            switch (message.Type)
            {
                case ThermalMessageType.FullSyncRequest:
                    if (IsRateLimited(LastFullSyncRequests, senderSteamId, 2000))
                        return;
                    ThermalAuthority.SendFullSyncTo(senderSteamId);
                    ChatBot.ChatBot.SendPlayerSettingsTo(senderSteamId, playerIdentityId);
                    break;

                case ThermalMessageType.PlayerCommand:
                    if (IsRateLimited(LastCommandRequests, senderSteamId, 250)
                        || !IsValidCommand(message.Command, message.Arguments))
                        return;
                    ChatBot.ChatBot.HandleServerCommand(senderSteamId, playerIdentityId, message.Command,
                        message.Arguments == null ? new string[0] : message.Arguments.ToArray());
                    break;

                case ThermalMessageType.RadiatorColorRequest:
                    if (message.EntityId == 0
                        || IsWindowRateLimited(ColorRequestWindows, senderSteamId, 30, 1000))
                        return;
                    ThermalAuthority.TrySetRadiatorColor(message.EntityId, playerIdentityId,
                        message.IsMinimumColor, message.PackedColor);
                    break;

                case ThermalMessageType.StateRequest:
                    if (message.EntityId == 0
                        || IsWindowRateLimited(StateRequestWindows, senderSteamId, 30, 1000))
                        return;
                    VRage.ModAPI.IMyEntity requestedEntity;
                    if (MyAPIGateway.Entities == null
                        || !MyAPIGateway.Entities.TryGetEntityById(message.EntityId, out requestedEntity))
                        return;
                    var requestedGrid = requestedEntity as IMyCubeGrid;
                    if (requestedGrid == null || !IsPlayerRelevant(player, requestedGrid))
                        return;
                    ThermalAuthority.SendGridStateTo(senderSteamId, message.EntityId);
                    break;
            }
        }

        private static void HandleServerMessage(ThermalNetworkMessage message)
        {
            if (message == null)
                return;

            switch (message.Type)
            {
                case ThermalMessageType.StateUpdate:
                    if (message.States != null)
                    {
                        foreach (var state in message.States)
                            ThermalAuthority.ApplyClientState(state);
                    }
                    else
                        ThermalAuthority.ApplyClientState(message.State);
                    break;

                case ThermalMessageType.ChatMessage:
                    ChatBot.ChatBot.DisplayNetworkMessage(message.Author, message.Text);
                    break;

                case ThermalMessageType.PlayerSettings:
                    ApplyPlayerSettings(message.PlayerSettings);
                    break;
            }
        }

        private static void ApplyPlayerSettings(ThermalPlayerSettings settings)
        {
            if (settings == null)
                return;

            ClientHudDisabled = settings.HudDisabled;
            ClientDebugMode = settings.DebugMode;
            HasReceivedPlayerSettings = true;
            ChatBot.ChatBot.ApplyClientSettings(settings);
        }

        private static void FlushQueuedStates()
        {
            if (QueuedStates.Count == 0 || MyAPIGateway.Players == null)
                return;

            var queuedStates = new List<QueuedState>(QueuedStates.Values);
            QueuedStates.Clear();

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, player => player != null && !player.IsBot);
            var reliableByPlayer = new Dictionary<ulong, List<ThermalBlockState>>();
            var unreliableByPlayer = new Dictionary<ulong, List<ThermalBlockState>>();

            foreach (var queued in queuedStates)
            {
                try
                {
                    if (queued.Provider == null || queued.State == null)
                        continue;
                    foreach (var player in players)
                    {
                        if (!IsPlayerRelevant(player, queued.Provider.TerminalBlock))
                            continue;
                        AddStateForPlayer(queued.Reliable ? reliableByPlayer : unreliableByPlayer,
                            player.SteamUserId, queued.State);
                    }
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine($"SkiittzsThermalMechanics: Failed to queue sync state: {exception}");
                }
            }

            SendStateBatches(reliableByPlayer, true);
            SendStateBatches(unreliableByPlayer, false);
        }

        private static void AddStateForPlayer(Dictionary<ulong, List<ThermalBlockState>> statesByPlayer,
            ulong recipientSteamId, ThermalBlockState state)
        {
            List<ThermalBlockState> states;
            if (!statesByPlayer.TryGetValue(recipientSteamId, out states))
            {
                states = new List<ThermalBlockState>();
                statesByPlayer[recipientSteamId] = states;
            }
            states.Add(state);
        }

        private static void SendStateBatches(Dictionary<ulong, List<ThermalBlockState>> statesByPlayer,
            bool reliable)
        {
            foreach (var states in statesByPlayer)
                SendStateBatches(states.Key, states.Value, reliable);
        }

        private static void SendStateBatches(ulong recipientSteamId, List<ThermalBlockState> states, bool reliable)
        {
            if (states == null || states.Count == 0
                || recipientSteamId == LocalSteamId() && !IsDedicated())
                return; // A listen-server host already owns the authoritative object instances.

            for (var index = 0; index < states.Count; index += MaximumStatesPerPacket)
            {
                var count = Math.Min(MaximumStatesPerPacket, states.Count - index);
                SendToPlayer(new ThermalNetworkMessage
                {
                    Type = ThermalMessageType.StateUpdate,
                    States = states.GetRange(index, count)
                }, recipientSteamId, reliable);
            }
        }

        private static void SendToServer(ThermalNetworkMessage message, bool reliable)
        {
            if (MyAPIGateway.Multiplayer == null)
                return;

            try
            {
                var payload = MyAPIGateway.Utilities.SerializeToBinary(message);
                if (payload == null || payload.Length == 0 || payload.Length > MaximumPacketBytes)
                    return;
                MyAPIGateway.Multiplayer.SendMessageToServer(ChannelId, payload, reliable);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine($"SkiittzsThermalMechanics: Failed to send packet to server: {exception.Message}");
            }
        }

        private static void SendToPlayer(ThermalNetworkMessage message, ulong recipientSteamId, bool reliable)
        {
            // SendMessageTo does not need a network round trip for the local listen-server player.
            if (recipientSteamId != 0 && recipientSteamId == LocalSteamId() && !IsDedicated())
            {
                HandleServerMessage(message);
                return;
            }

            if (MyAPIGateway.Multiplayer == null || recipientSteamId == 0)
                return;

            try
            {
                var payload = MyAPIGateway.Utilities.SerializeToBinary(message);
                if (payload == null || payload.Length == 0 || payload.Length > MaximumPacketBytes)
                    return;
                MyAPIGateway.Multiplayer.SendMessageTo(ChannelId, payload, recipientSteamId, reliable);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine($"SkiittzsThermalMechanics: Failed to send packet to {recipientSteamId}: {exception.Message}");
            }
        }

        private static bool IsPlayerRelevant(IMyPlayer player, IMyTerminalBlock block)
        {
            if (player == null || block == null || block.CubeGrid == null)
                return false;

            if (block.OwnerId == player.IdentityId)
                return true;

            return IsPlayerRelevant(player, block.CubeGrid);
        }

        private static bool IsPlayerRelevant(IMyPlayer player, IMyCubeGrid grid)
        {
            if (player == null || grid == null)
                return false;

            foreach (var ownerId in grid.BigOwners)
                if (ownerId == player.IdentityId)
                    return true;

            var viewDistance = 15000d;
            if (MyAPIGateway.Session != null && MyAPIGateway.Session.SessionSettings != null
                && MyAPIGateway.Session.SessionSettings.ViewDistance > 0)
                viewDistance = MyAPIGateway.Session.SessionSettings.ViewDistance;

            var syncRange = Math.Max(5000d, viewDistance * 1.25d);
            return Vector3D.DistanceSquared(player.GetPosition(), grid.GetPosition()) <= syncRange * syncRange;
        }

        private static bool IsValidCommand(string command, List<string> arguments)
        {
            if (string.IsNullOrWhiteSpace(command) || command.Length > MaximumCommandLength)
                return false;

            if (arguments == null)
                return true;
            if (arguments.Count > MaximumArguments)
                return false;

            foreach (var argument in arguments)
                if (argument == null || argument.Length > MaximumArgumentLength)
                    return false;

            return true;
        }

        private static bool IsRateLimited(Dictionary<ulong, DateTime> timestamps, ulong senderSteamId,
            int minimumIntervalMilliseconds)
        {
            var now = DateTime.UtcNow;
            DateTime lastRequest;
            if (timestamps.TryGetValue(senderSteamId, out lastRequest)
                && (now - lastRequest).TotalMilliseconds < minimumIntervalMilliseconds)
                return true;

            timestamps[senderSteamId] = now;
            return false;
        }

        private static bool IsWindowRateLimited(Dictionary<ulong, RequestRateWindow> windows, ulong senderSteamId,
            int maximumRequests, int windowMilliseconds)
        {
            var now = DateTime.UtcNow;
            RequestRateWindow window;
            if (!windows.TryGetValue(senderSteamId, out window)
                || (now - window.StartedUtc).TotalMilliseconds >= windowMilliseconds)
            {
                windows[senderSteamId] = new RequestRateWindow { StartedUtc = now, Count = 1 };
                return false;
            }

            if (window.Count >= maximumRequests)
                return true;
            window.Count++;
            return false;
        }

        private static ulong LocalSteamId()
        {
            if (MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null)
                return MyAPIGateway.Session.Player.SteamUserId;
            return MyAPIGateway.Multiplayer == null ? 0 : MyAPIGateway.Multiplayer.MyId;
        }

        private static bool IsDedicated()
        {
            return MyAPIGateway.Utilities != null && MyAPIGateway.Utilities.IsDedicated;
        }

        private sealed class QueuedState
        {
            public IThermalStateProvider Provider;
            public ThermalBlockState State;
            public bool Reliable;
        }

        private sealed class RequestRateWindow
        {
            public DateTime StartedUtc;
            public int Count;
        }
    }
}
