using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Game.ModAPI;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.ChatBot
{
    public static class ChatBot
    {
        public static string ChatBotName { get; set; }
        public static int MessageDelay {get; set; }
        private static int _messageAttemptCounter = 0;
        private static List<long> _disabledPlayerIds;
		private static List<long> _warningOnlyPlayerIds;
		private static List<long> _introducedPlayersThisSession = new List<long>();
		private static Dictionary<long, string> _playerAsstNameOverrides = new Dictionary<long, string>();

		public static string ChatBotNameFor(long playerId)
		{
			return _playerAsstNameOverrides.ContainsKey(playerId) ? _playerAsstNameOverrides[playerId] : ChatBotName;
		}

        public static void WarnPlayer(IMyTerminalBlock block, string message, MessageSeverity messageSeverity)
        {
            if (block == null || !block.IsOwnedByCurrentPlayer())
                return;

            WarnPlayer($"{block.CubeGrid.CustomName}-{block.CustomName}: {message}", messageSeverity);
        }

        public static void WarnPlayer(IMyCubeGrid grid, string message, MessageSeverity messageSeverity)
        {
            var playerId = Utilities.TryGetCurrentPlayerId();
            if (!grid.BigOwners.Contains(playerId))
                return;
            WarnPlayer( playerId, $"{grid.CustomName}: {message}", messageSeverity);
        }

        private static void WarnPlayer(string message, MessageSeverity messageSeverity)
        {
	        var playerId = Utilities.TryGetCurrentPlayerId();
			WarnPlayer(playerId, message, messageSeverity);
		}

		private static void WarnPlayer(long playerId, string message, MessageSeverity messageSeverity)
        {
			if (_disabledPlayerIds.Contains(playerId))
		        return;

			if (messageSeverity == MessageSeverity.Tutorial && _warningOnlyPlayerIds.Contains(playerId))
				return;

	        if (_messageAttemptCounter == 0)
	        {
		        MyAPIGateway.Utilities.ShowMessage(ChatBotNameFor(playerId), message);
	        }

	        _messageAttemptCounter++;
	        if (_messageAttemptCounter >= MessageDelay)
		        _messageAttemptCounter = 0;

		}

		private const string disabledPlayersFileName = "ChatBotDisabledPlayerIds.xml";
		private const string warningOnlyPlayersFileName = "ChatBotWarningOnlyPlayerIds.xml";
		private const string playerChatBotNameOverridesFileName = "PlayerChatBotNameOverrides.xml";


		private static void SaveDisabledPlayers()
        {
			var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(disabledPlayersFileName, typeof(SkiittzThermalMechanicsSession));
            var content = _disabledPlayerIds;
            writer.Write(MyAPIGateway.Utilities.SerializeToXML(content));
            writer.Flush();
            writer.Close();
        }

        private static void SavePlayerChatBotNameOverrides()
        {
	        var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(playerChatBotNameOverridesFileName, typeof(SkiittzThermalMechanicsSession));
	        var content = _playerAsstNameOverrides
		        .Select(x => new ChatBotOverride { PlayerId = x.Key, Name = x.Value})
		        .ToList();
	        writer.Write(MyAPIGateway.Utilities.SerializeToXML(content));
	        writer.Flush();
	        writer.Close();
        }

        public class ChatBotOverride
        {
            public long PlayerId { get; set; }
            public string Name { get; set; }
        }

        private static void SaveWarningOnlyPlayers()
        {
	        var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(warningOnlyPlayersFileName, typeof(SkiittzThermalMechanicsSession));
	        var content = _warningOnlyPlayerIds;
	        writer.Write(MyAPIGateway.Utilities.SerializeToXML(content));
	        writer.Flush();
	        writer.Close();
        }

		public static void LoadDisabledPlayers()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(disabledPlayersFileName, typeof(SkiittzThermalMechanicsSession)))
            {
                var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(disabledPlayersFileName, typeof(SkiittzThermalMechanicsSession));
                var content = reader.ReadToEnd();
                reader.Close();
                _disabledPlayerIds = MyAPIGateway.Utilities.SerializeFromXML<List<long>>(content);
            }
            else
                _disabledPlayerIds = new List<long>();
        }

        public static void LoadWarningOnlyPlayers()
        {
	        if (MyAPIGateway.Utilities.FileExistsInWorldStorage(warningOnlyPlayersFileName, typeof(SkiittzThermalMechanicsSession)))
	        {
		        var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(warningOnlyPlayersFileName, typeof(SkiittzThermalMechanicsSession));
		        var content = reader.ReadToEnd();
		        reader.Close();
		        _warningOnlyPlayerIds = MyAPIGateway.Utilities.SerializeFromXML<List<long>>(content);
	        }
	        else
		        _warningOnlyPlayerIds = new List<long>();
        }

        public static void LoadPlayerChatBotNameOverrides()
        {
	        _playerAsstNameOverrides = new Dictionary<long, string>();
			if (MyAPIGateway.Utilities.FileExistsInWorldStorage(playerChatBotNameOverridesFileName, typeof(SkiittzThermalMechanicsSession)))
	        {
		        var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(playerChatBotNameOverridesFileName, typeof(SkiittzThermalMechanicsSession));
		        var content = reader.ReadToEnd();
		        reader.Close();
		        var results = MyAPIGateway.Utilities.SerializeFromXML<List<ChatBotOverride>>(content);
		        foreach (var item in results)
		        {
                    _playerAsstNameOverrides.Add(item.PlayerId, item.Name);
		        }
	        }
        }

        public static void ToggleHudForPlayer(long playerId)
        {
            Configuration.Configuration.ToggleHudForPlayer(playerId);
        }

		public static void DisableMessagesForPlayer(long playerId)
        {
            _disabledPlayerIds.Add(playerId);
            SaveDisabledPlayers();
        }

        public static void EnableMessagesForPlayer(long playerId)
        {
            _disabledPlayerIds.Remove(playerId);
            SaveDisabledPlayers();
        }

        public static void DisableTutorialMessagesForPlayer(long playerId)
        {
            _warningOnlyPlayerIds.Add(playerId);
            SaveWarningOnlyPlayers();
        }

        public static void EnableTutorialMessagesForPlayer(long playerId)
        {
	        _warningOnlyPlayerIds.Remove(playerId);
	        SaveWarningOnlyPlayers();
        }

        public static void RenameChatBot(long playerId, string newName)
        {
            _playerAsstNameOverrides[playerId] = newName;
            SavePlayerChatBotNameOverrides();
            MyAPIGateway.Utilities.ShowMessage(newName, $"Ok, ill respond to /{newName} from here on out.");
        }

		public static void IntroduceMyself()
        {
            var playerId = Utilities.TryGetCurrentPlayerId();
            var chatBotName = ChatBotNameFor(playerId);
            if (_disabledPlayerIds.Contains(playerId) || _introducedPlayersThisSession.Contains(playerId)) return;

            var message = new StringBuilder();
            message.AppendLine($"Hello new there!  I am {chatBotName}, your trusty thermal monitoring software!");
            message.AppendLine($"I'm here to help you prevent your ships from overheating :)");
            message.AppendLine($"To see a list of available commands, type \"/{chatBotName} help\"");
            
            MyAPIGateway.Utilities.ShowMessage(chatBotName, message.ToString());
            _introducedPlayersThisSession.Add(playerId);
            MyAPIGateway.Session.OnSessionReady -= IntroduceMyself;
        }

        public static void HandleCommand(string command, object[] args = null)
        {
            if (!commandMappings.ContainsKey(command))
            {
                PrintUnknownCommand();
                return;
            }
            
			switch (commandMappings[command])
            {
                case "Help":
                    PrintHelp();
                    break;
                case "StopMessages":
                    DisableMessagesForPlayer(Utilities.TryGetCurrentPlayerId());
                    MyAPIGateway.Utilities.ShowMessage(ChatBotName, "All messages stopped");
                    break;
                case "ReEnable":
                    EnableMessagesForPlayer(Utilities.TryGetCurrentPlayerId());
                    MyAPIGateway.Utilities.ShowMessage(ChatBotName, "All messages re-enabled");
					break;
                case "Reload":
                    if (MyAPIGateway.Session.IsUserAdmin(MyAPIGateway.Session.Player.SteamUserId))
                    {
	                    Configuration.Configuration.Load();
                        var definitions = Configuration.Configuration.BlockSettings.Select(x => x.Key);
                        MyAPIGateway.Utilities.ShowMessage(ChatBotName, $"configs reloaded for block types: {string.Join(",",definitions)}");
                    }
                    else
                        MyAPIGateway.Utilities.ShowMessage(ChatBotName, $"This command can only be run by an admin");
                    break;
                case "StopTutorial":
	                DisableTutorialMessagesForPlayer(Utilities.TryGetCurrentPlayerId());
	                MyAPIGateway.Utilities.ShowMessage(ChatBotName, "Tutorial messages stopped");
					break;
                case "StartTutorial":
	                EnableTutorialMessagesForPlayer(Utilities.TryGetCurrentPlayerId());
	                MyAPIGateway.Utilities.ShowMessage(ChatBotName, "Tutorial messages re-enabled");
					break;
                case "Rename":
                    RenameChatBot(Utilities.TryGetCurrentPlayerId(), args?[0]?.ToString());
	                break;
                case "ToggleHud":
                    ToggleHudForPlayer(Utilities.TryGetCurrentPlayerId());
                    break;
                case "ToggleDebug":
	                if (MyAPIGateway.Session.IsUserAdmin(MyAPIGateway.Session.Player.SteamUserId))
	                {
		                Configuration.Configuration.ToggleDebugMode();
		                MyAPIGateway.Utilities.ShowMessage(ChatBotName,
			                $"Debug mode is now {(Configuration.Configuration.DebugMode ? "enabled" : "disabled")}");
	                }
	                else
						MyAPIGateway.Utilities.ShowMessage(ChatBotName, $"This command can only be run by an admin");
					break;
				default:
                    PrintUnknownCommand();
                    break;
            }
        }


        public static void PrintUnknownCommand()
        {
            MyAPIGateway.Utilities.ShowMessage(ChatBotName, $"I dont know what that means.....");
        }

        public static void PrintHelp()
        {
            var message = new StringBuilder();
            foreach (var commandMapping in commandMappings.OrderBy(x => x.Value))
            {
                var prefix = $"/{ChatBotName} {commandMapping.Key}";
                switch (commandMapping.Value)
                {
                    case "Help":
                        message.AppendLine($"{prefix}: Displays info about available commands");
                        break;
                    case "StopMessages":
                        message.AppendLine($"{prefix}:Turn off all messages from {ChatBotName}");
                        break;
                    case "ReEnable":
                        message.AppendLine($"{prefix}:Turn on messages from {ChatBotName}");
                        break;
                    case "StopTutorial":
	                    message.AppendLine($"{prefix}:Turn off tutorial messages from {ChatBotName} (warnings will still display)");
	                    break;
                    case "StartTutorial":
	                    message.AppendLine($"{prefix}:Turn on tutorial messages from {ChatBotName}");
	                    break;
                    case "Rename":
	                    message.AppendLine($"{prefix}:Change the assistant's name");
	                    break;
                    case "ToggleHud":
	                    message.AppendLine($"{prefix}:Toggle heat HUD");
	                    break;
				}
			}
            MyAPIGateway.Utilities.ShowMessage(ChatBotName, message.ToString());

        }

        private static Dictionary<string, string> commandMappings = new Dictionary<string, string>
        {
            {"stfu","StopMessages"},
            {"speak","ReEnable"},
            {"help","Help"},
            {"reload","Reload"},
            {"iamnotanewb", "StopTutorial"},
            {"spankemedaddy_iamnewb","StartTutorial"},
            {"rename","Rename"},
            {"debug","ToggleDebug"},
            {"togglehud","ToggleHud"}
        };
        public static void InitConfigs(Dictionary<string, string> settings)
        {
            MessageDelay = settings.ContainsKey("ChatFrequencyLimiter")
                ? int.Parse(settings["ChatFrequencyLimiter"])
                : 0;
            ChatBotName = settings.ContainsKey("ChatBotName")
                ? settings["ChatBotName"]
                : "HotDaddy";
            foreach (var command in settings.Where(x => x.Key.StartsWith("ChatBotCommand_")))
            {
                commandMappings[command.Value] = command.Key.Replace("ChatBotCommand_", "");
            }

            MyAPIGateway.Session.OnSessionReady += IntroduceMyself;
        }
    }

    public enum MessageSeverity
    {
        Tutorial,
        Warning
    }
}
