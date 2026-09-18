using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Networking;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.ChatBot
{
    public static class ChatBot
    {
        private const string DisabledPlayersFileName = "ChatBotDisabledPlayerIds.xml";
        private const string WarningOnlyPlayersFileName = "ChatBotWarningOnlyPlayerIds.xml";
        private const string PlayerChatBotNameOverridesFileName = "PlayerChatBotNameOverrides.xml";
        private const int MaximumChatBotNameLength = 32;

        public static string ChatBotName { get; set; } = "HotDaddy";
        public static int MessageDelay { get; set; }

        // Retained as a summary timestamp for compatibility with existing diagnostics/tests.
        private static DateTime _lastMessageTime = DateTime.MinValue;
        private static readonly Dictionary<long, DateTime> LastMessageTimesByPlayer =
            new Dictionary<long, DateTime>();
        private static List<long> _disabledPlayerIds = new List<long>();
        private static List<long> _warningOnlyPlayerIds = new List<long>();
        private static List<long> _introducedPlayersThisSession = new List<long>();
        private static Dictionary<long, string> _playerAsstNameOverrides = new Dictionary<long, string>();
        private static string _clientChatBotName = "HotDaddy";
        private static bool _clientMessagesDisabled;

        private static Dictionary<string, string> commandMappings = DefaultCommandMappings();

        public static string ChatBotNameFor(long playerId)
        {
            if (!ThermalAuthority.IsServer && !string.IsNullOrEmpty(_clientChatBotName))
                return _clientChatBotName;

            string overriddenName;
            return _playerAsstNameOverrides != null
                   && _playerAsstNameOverrides.TryGetValue(playerId, out overriddenName)
                ? overriddenName
                : ChatBotName;
        }

        public static void WarnPlayer(IMyTerminalBlock block, string message, MessageSeverity messageSeverity)
        {
            if (!ThermalAuthority.IsServer || block == null || block.CubeGrid == null)
                return;

            var recipientIds = new HashSet<long>();
            foreach (var ownerId in block.CubeGrid.BigOwners)
                recipientIds.Add(ownerId);
            if (block.OwnerId != 0)
                recipientIds.Add(block.OwnerId);
            foreach (var recipientId in recipientIds)
            {
                var player = ConnectedHumanPlayer(recipientId);
                if (player == null)
                    continue;
                WarnPlayer(player, $"{block.CubeGrid.CustomName}-{block.CustomName}: {message}", messageSeverity);
            }
        }

        public static void WarnPlayer(IMyCubeGrid grid, string message, MessageSeverity messageSeverity)
        {
            if (!ThermalAuthority.IsServer || grid == null)
                return;

            foreach (var recipientId in grid.BigOwners)
            {
                var player = ConnectedHumanPlayer(recipientId);
                if (player != null)
                    WarnPlayer(player, $"{grid.CustomName}: {message}", messageSeverity);
            }
        }

        private static void WarnPlayer(IMyPlayer player, string message, MessageSeverity messageSeverity)
        {
            if (player == null || IsMessagesDisabled(player.IdentityId))
                return;
            if (messageSeverity == MessageSeverity.Tutorial && IsTutorialDisabled(player.IdentityId))
                return;

            DateTime lastMessage;
            var now = DateTime.UtcNow;
            if (MessageDelay > 0
                && LastMessageTimesByPlayer.TryGetValue(player.IdentityId, out lastMessage)
                && (now - lastMessage).TotalSeconds < MessageDelay)
                return;

            ThermalNetwork.SendChatMessage(player.SteamUserId, ChatBotNameFor(player.IdentityId), message);
            LastMessageTimesByPlayer[player.IdentityId] = now;
            _lastMessageTime = now;
        }

        public static void HandleCommand(string command, object[] args = null)
        {
            var stringArguments = args == null
                ? new string[0]
                : args.Select(value => value == null ? string.Empty : value.ToString()).ToArray();
            ThermalNetwork.SendCommand(command, stringArguments);
        }

        public static void HandleServerCommand(ulong senderSteamId, long playerIdentityId, string command,
            string[] arguments)
        {
            if (!ThermalAuthority.IsServer || playerIdentityId == 0 || string.IsNullOrWhiteSpace(command))
                return;

            command = command.ToLowerInvariant();
            string action;
            if (!commandMappings.TryGetValue(command, out action))
            {
                Reply(senderSteamId, playerIdentityId, "I dont know what that means.....");
                return;
            }

            switch (action)
            {
                case "Help":
                    Reply(senderSteamId, playerIdentityId, BuildHelp(playerIdentityId));
                    break;
                case "StopMessages":
                    DisableMessagesForPlayer(playerIdentityId);
                    Reply(senderSteamId, playerIdentityId, "All messages stopped");
                    SendPlayerSettingsTo(senderSteamId, playerIdentityId);
                    break;
                case "ReEnable":
                    EnableMessagesForPlayer(playerIdentityId);
                    Reply(senderSteamId, playerIdentityId, "All messages re-enabled");
                    SendPlayerSettingsTo(senderSteamId, playerIdentityId);
                    break;
                case "Reload":
                    if (!IsAdmin(senderSteamId))
                    {
                        Reply(senderSteamId, playerIdentityId, "This command can only be run by an admin");
                        break;
                    }
                    Configuration.Configuration.Load(true);
                    ThermalAuthority.ReloadAllConfigurations();
                    Reply(senderSteamId, playerIdentityId,
                        $"configs reloaded for {Configuration.Configuration.BlockSettings.Count} block types");
                    SendPlayerSettingsToAll();
                    break;
                case "StopTutorial":
                    DisableTutorialMessagesForPlayer(playerIdentityId);
                    Reply(senderSteamId, playerIdentityId, "Tutorial messages stopped");
                    SendPlayerSettingsTo(senderSteamId, playerIdentityId);
                    break;
                case "StartTutorial":
                    EnableTutorialMessagesForPlayer(playerIdentityId);
                    Reply(senderSteamId, playerIdentityId, "Tutorial messages re-enabled");
                    SendPlayerSettingsTo(senderSteamId, playerIdentityId);
                    break;
                case "Rename":
                    var requestedName = arguments == null ? string.Empty : string.Join(" ", arguments).Trim();
                    if (!IsValidChatBotName(requestedName))
                    {
                        Reply(senderSteamId, playerIdentityId,
                            $"Name must be 1-{MaximumChatBotNameLength} letters, numbers, hyphens, or underscores.");
                        break;
                    }
                    RenameChatBot(playerIdentityId, requestedName);
                    Reply(senderSteamId, playerIdentityId,
                        $"Ok, ill respond to /{requestedName} from here on out.");
                    SendPlayerSettingsTo(senderSteamId, playerIdentityId);
                    break;
                case "ToggleHud":
                    Configuration.Configuration.ToggleHudForPlayer(playerIdentityId);
                    SendPlayerSettingsTo(senderSteamId, playerIdentityId);
                    break;
                case "ToggleDebug":
                    if (!IsAdmin(senderSteamId))
                    {
                        Reply(senderSteamId, playerIdentityId, "This command can only be run by an admin");
                        break;
                    }
                    Configuration.Configuration.ToggleDebugMode();
                    Reply(senderSteamId, playerIdentityId,
                        $"Debug mode is now {(Configuration.Configuration.DebugMode ? "enabled" : "disabled")}");
                    SendPlayerSettingsToAll();
                    break;
            }
        }

        public static void DisplayNetworkMessage(string author, string message)
        {
            if (MyAPIGateway.Utilities == null || MyAPIGateway.Utilities.IsDedicated || string.IsNullOrEmpty(message))
                return;
            MyAPIGateway.Utilities.ShowMessage(author ?? string.Empty, message);
        }

        public static void ApplyClientSettings(ThermalPlayerSettings settings)
        {
            if (settings == null)
                return;
            _clientChatBotName = string.IsNullOrWhiteSpace(settings.ChatBotName) ? "HotDaddy" : settings.ChatBotName;
            _clientMessagesDisabled = settings.MessagesDisabled;
            IntroduceMyself();
        }

        public static void SendPlayerSettingsTo(ulong recipientSteamId, long playerIdentityId)
        {
            if (!ThermalAuthority.IsServer)
                return;

            ThermalNetwork.SendPlayerSettings(recipientSteamId, new ThermalPlayerSettings
            {
                ChatBotName = ChatBotNameFor(playerIdentityId),
                HudDisabled = Configuration.Configuration.PlayerHudIsDisabled(playerIdentityId),
                MessagesDisabled = IsMessagesDisabled(playerIdentityId),
                TutorialMessagesDisabled = IsTutorialDisabled(playerIdentityId),
                DebugMode = Configuration.Configuration.DebugMode
            });
        }

        public static void SendPlayerSettingsToAll()
        {
            if (!ThermalAuthority.IsServer)
                return;
            foreach (var player in ConnectedHumanPlayers())
                SendPlayerSettingsTo(player.SteamUserId, player.IdentityId);
        }

        public static void IntroduceMyself()
        {
            if (MyAPIGateway.Utilities == null || MyAPIGateway.Utilities.IsDedicated
                || MyAPIGateway.Session == null || MyAPIGateway.Session.Player == null)
                return;

            var playerId = MyAPIGateway.Session.Player.IdentityId;
            if (_clientMessagesDisabled || IsMessagesDisabled(playerId)
                || _introducedPlayersThisSession.Contains(playerId))
                return;

            var chatBotName = ChatBotNameFor(playerId);
            var message = new StringBuilder();
            message.AppendLine($"Hello new there!  I am {chatBotName}, your trusty thermal monitoring software!");
            message.AppendLine("I'm here to help you prevent your ships from overheating :)");
            message.AppendLine($"To see a list of available commands, type \"/{chatBotName} help\"");
            MyAPIGateway.Utilities.ShowMessage(chatBotName, message.ToString());
            _introducedPlayersThisSession.Add(playerId);
            MyAPIGateway.Session.OnSessionReady -= IntroduceMyself;
        }

        public static void DisableMessagesForPlayer(long playerId)
        {
            if (!ThermalAuthority.IsServer)
                return;
            if (!_disabledPlayerIds.Contains(playerId))
                _disabledPlayerIds.Add(playerId);
            SaveDisabledPlayers();
        }

        public static void EnableMessagesForPlayer(long playerId)
        {
            if (!ThermalAuthority.IsServer)
                return;
            _disabledPlayerIds.Remove(playerId);
            SaveDisabledPlayers();
        }

        public static void DisableTutorialMessagesForPlayer(long playerId)
        {
            if (!ThermalAuthority.IsServer)
                return;
            if (!_warningOnlyPlayerIds.Contains(playerId))
                _warningOnlyPlayerIds.Add(playerId);
            SaveWarningOnlyPlayers();
        }

        public static void EnableTutorialMessagesForPlayer(long playerId)
        {
            if (!ThermalAuthority.IsServer)
                return;
            _warningOnlyPlayerIds.Remove(playerId);
            SaveWarningOnlyPlayers();
        }

        public static void RenameChatBot(long playerId, string newName)
        {
            if (!ThermalAuthority.IsServer || !IsValidChatBotName(newName))
                return;
            _playerAsstNameOverrides[playerId] = newName;
            SavePlayerChatBotNameOverrides();
        }

        public static void ToggleHudForPlayer(long playerId)
        {
            Configuration.Configuration.ToggleHudForPlayer(playerId);
        }

        public static void InitConfigs(Dictionary<string, string> settings)
        {
            commandMappings = DefaultCommandMappings();
            settings = settings ?? new Dictionary<string, string>();

            int configuredDelay;
            MessageDelay = settings.ContainsKey("ChatFrequencyLimiter")
                           && int.TryParse(settings["ChatFrequencyLimiter"], out configuredDelay)
                ? configuredDelay
                : 0;
            string configuredName;
            ChatBotName = settings.TryGetValue("ChatBotName", out configuredName)
                          && IsValidChatBotName(configuredName) ? configuredName : "HotDaddy";

            foreach (var command in settings.Where(setting => setting.Key.StartsWith("ChatBotCommand_")
                                                               && !string.IsNullOrWhiteSpace(setting.Value)))
                commandMappings[command.Value.ToLowerInvariant()] = command.Key.Replace("ChatBotCommand_", string.Empty);

            if (MyAPIGateway.Session != null && (MyAPIGateway.Utilities == null || !MyAPIGateway.Utilities.IsDedicated))
            {
                MyAPIGateway.Session.OnSessionReady -= IntroduceMyself;
                MyAPIGateway.Session.OnSessionReady += IntroduceMyself;
            }
        }

        public static void LoadDisabledPlayers()
        {
            _disabledPlayerIds = LoadList(DisabledPlayersFileName);
        }

        public static void LoadWarningOnlyPlayers()
        {
            _warningOnlyPlayerIds = LoadList(WarningOnlyPlayersFileName);
        }

        public static void LoadPlayerChatBotNameOverrides()
        {
            _playerAsstNameOverrides = new Dictionary<long, string>();
            if (!ThermalAuthority.IsServer)
                return;

            try
            {
                if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(PlayerChatBotNameOverridesFileName,
                        typeof(SkiittzThermalMechanicsSession)))
                    return;
                string content;
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(PlayerChatBotNameOverridesFileName,
                           typeof(SkiittzThermalMechanicsSession)))
                    content = reader.ReadToEnd();
                var results = MyAPIGateway.Utilities.SerializeFromXML<List<ChatBotOverride>>(content)
                              ?? new List<ChatBotOverride>();
                foreach (var item in results)
                    if (item != null && IsValidChatBotName(item.Name))
                        _playerAsstNameOverrides[item.PlayerId] = item.Name;
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine($"SkiittzsThermalMechanics: Failed to load chatbot names: {exception.Message}");
            }
        }

        public static void PrintUnknownCommand()
        {
            DisplayNetworkMessage(ChatBotName, "I dont know what that means.....");
        }

        public static void PrintHelp()
        {
            var playerId = Utilities.TryGetCurrentPlayerId();
            DisplayNetworkMessage(ChatBotNameFor(playerId), BuildHelp(playerId));
        }

        public static void ResetSession()
        {
            if (MyAPIGateway.Session != null)
                MyAPIGateway.Session.OnSessionReady -= IntroduceMyself;
            _disabledPlayerIds = new List<long>();
            _warningOnlyPlayerIds = new List<long>();
            _introducedPlayersThisSession = new List<long>();
            _playerAsstNameOverrides = new Dictionary<long, string>();
            LastMessageTimesByPlayer.Clear();
            _lastMessageTime = DateTime.MinValue;
            _clientChatBotName = "HotDaddy";
            _clientMessagesDisabled = false;
            ChatBotName = "HotDaddy";
            MessageDelay = 0;
            commandMappings = DefaultCommandMappings();
        }

        private static void Reply(ulong recipientSteamId, long playerIdentityId, string message)
        {
            ThermalNetwork.SendChatMessage(recipientSteamId, ChatBotNameFor(playerIdentityId), message);
        }

        private static bool IsAdmin(ulong steamUserId)
        {
            return MyAPIGateway.Session != null && MyAPIGateway.Session.IsUserAdmin(steamUserId);
        }

        private static bool IsMessagesDisabled(long playerId)
        {
            return _disabledPlayerIds != null && _disabledPlayerIds.Contains(playerId);
        }

        private static bool IsTutorialDisabled(long playerId)
        {
            return _warningOnlyPlayerIds != null && _warningOnlyPlayerIds.Contains(playerId);
        }

        private static List<IMyPlayer> ConnectedHumanPlayers()
        {
            var players = new List<IMyPlayer>();
            if (MyAPIGateway.Players != null)
                MyAPIGateway.Players.GetPlayers(players, player => player != null && !player.IsBot);
            return players;
        }

        private static IMyPlayer ConnectedHumanPlayer(long identityId)
        {
            if (identityId == 0 || MyAPIGateway.Players == null)
                return null;
            var player = MyAPIGateway.Players.TryGetIdentityId(identityId);
            return player == null || player.IsBot ? null : player;
        }

        private static bool IsValidChatBotName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > MaximumChatBotNameLength)
                return false;
            foreach (var character in name)
                if (!char.IsLetterOrDigit(character) && character != '-' && character != '_')
                    return false;
            return true;
        }

        private static string BuildHelp(long playerId)
        {
            var message = new StringBuilder();
            foreach (var commandMapping in commandMappings.OrderBy(mapping => mapping.Value))
            {
                var prefix = $"/{ChatBotNameFor(playerId)} {commandMapping.Key}";
                switch (commandMapping.Value)
                {
                    case "Help": message.AppendLine($"{prefix}: Displays info about available commands"); break;
                    case "StopMessages": message.AppendLine($"{prefix}: Turn off all thermal messages"); break;
                    case "ReEnable": message.AppendLine($"{prefix}: Turn on thermal messages"); break;
                    case "StopTutorial": message.AppendLine($"{prefix}: Turn off tutorial messages (warnings remain enabled)"); break;
                    case "StartTutorial": message.AppendLine($"{prefix}: Turn tutorial messages back on"); break;
                    case "Rename": message.AppendLine($"{prefix}: Change the assistant's name"); break;
                    case "ToggleHud": message.AppendLine($"{prefix}: Toggle the heat HUD"); break;
                }
            }
            return message.ToString();
        }

        private static Dictionary<string, string> DefaultCommandMappings()
        {
            return new Dictionary<string, string>
            {
                { "stfu", "StopMessages" }, { "speak", "ReEnable" }, { "help", "Help" },
                { "reload", "Reload" }, { "iamnotanewb", "StopTutorial" },
                { "spankemedaddy_iamnewb", "StartTutorial" }, { "rename", "Rename" },
                { "debug", "ToggleDebug" }, { "togglehud", "ToggleHud" }
            };
        }

        private static List<long> LoadList(string fileName)
        {
            if (!ThermalAuthority.IsServer)
                return new List<long>();
            try
            {
                if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName,
                        typeof(SkiittzThermalMechanicsSession)))
                    return new List<long>();
                string content;
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(fileName,
                           typeof(SkiittzThermalMechanicsSession)))
                    content = reader.ReadToEnd();
                return MyAPIGateway.Utilities.SerializeFromXML<List<long>>(content) ?? new List<long>();
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine($"SkiittzsThermalMechanics: Failed to load {fileName}: {exception.Message}");
                return new List<long>();
            }
        }

        private static void SaveDisabledPlayers()
        {
            SaveList(DisabledPlayersFileName, _disabledPlayerIds);
        }

        private static void SaveWarningOnlyPlayers()
        {
            SaveList(WarningOnlyPlayersFileName, _warningOnlyPlayerIds);
        }

        private static void SaveList(string fileName, List<long> values)
        {
            if (!ThermalAuthority.IsServer)
                return;
            try
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(fileName,
                           typeof(SkiittzThermalMechanicsSession)))
                {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(values ?? new List<long>()));
                    writer.Flush();
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine($"SkiittzsThermalMechanics: Failed to save {fileName}: {exception.Message}");
            }
        }

        private static void SavePlayerChatBotNameOverrides()
        {
            if (!ThermalAuthority.IsServer)
                return;
            try
            {
                var content = _playerAsstNameOverrides
                    .Select(item => new ChatBotOverride { PlayerId = item.Key, Name = item.Value }).ToList();
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(PlayerChatBotNameOverridesFileName,
                           typeof(SkiittzThermalMechanicsSession)))
                {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(content));
                    writer.Flush();
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine($"SkiittzsThermalMechanics: Failed to save chatbot names: {exception.Message}");
            }
        }

        public class ChatBotOverride
        {
            public long PlayerId { get; set; }
            public string Name { get; set; }
        }
    }

    public enum MessageSeverity
    {
        Tutorial,
        Warning
    }
}
