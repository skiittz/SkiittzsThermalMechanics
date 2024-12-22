using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.Scripting;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public static class ChatBot
    {
        public static string ChatBotName { get; set; }
        public static int MessageDelay {get; set; }
        private static int _messageAttemptCounter = 0;
        private static List<long> _disabledPlayerIds;
        private static List<long> _warningOnlyPlayerIds;
		private static List<long> _introducedPlayersThisSession = new List<long>();

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
		        MyAPIGateway.Utilities.ShowMessage(ChatBotName, message);
	        }

	        _messageAttemptCounter++;
	        if (_messageAttemptCounter >= MessageDelay)
		        _messageAttemptCounter = 0;

		}

		private const string disabledPlayersFileName = "ChatBotDisabledPlayerIds.xml";
		private const string warningOnlyPlayersFileName = "ChatBotWarningOnlyPlayerIds.xml";

		private static void SaveDisabledPlayers()
        {
			var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(disabledPlayersFileName, typeof(SkiittzThermalMechanicsSession));
            var content = _disabledPlayerIds;
            writer.Write(MyAPIGateway.Utilities.SerializeToXML(content));
            writer.Flush();
            writer.Close();
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

		public static void IntroduceMyself()
        {
            var playerId = Utilities.TryGetCurrentPlayerId();
            if (_disabledPlayerIds.Contains(playerId) || _introducedPlayersThisSession.Contains(playerId)) return;

            var message = new StringBuilder();
            message.AppendLine($"Hello new there!  I am {ChatBotName}, your trusty thermal monitoring software!");
            message.AppendLine($"I'm here to help you prevent your ships from overheating :)");
            message.AppendLine($"To see a list of available commands, type \"/{ChatBotName} help\"");
            
            MyAPIGateway.Utilities.ShowMessage(ChatBotName, message.ToString());
            _introducedPlayersThisSession.Add(playerId);
            MyAPIGateway.Session.OnSessionReady -= IntroduceMyself;
        }

        public static void HandleCommand(string command)
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
                    DisableMessagesForPlayer(MyAPIGateway.Session.Player.IdentityId);
                    break;
                case "ReEnable":
                    EnableMessagesForPlayer(MyAPIGateway.Session.Player.IdentityId);
                    break;
                case "Reload":
                    if (MyAPIGateway.Session.IsUserAdmin(MyAPIGateway.Session.Player.SteamUserId))
                    {
                        Configuration.Load();
                        var definitions = Configuration.BlockSettings.Select(x => x.Key);
                        MyAPIGateway.Utilities.ShowMessage(ChatBotName, $"configs reloaded for block types: {string.Join(",",definitions)}");
                    }
                    else
                        MyAPIGateway.Utilities.ShowMessage(ChatBotName, $"This command can only be run by an admin");
                    break;
                case "StopTutorial":
	                DisableTutorialMessagesForPlayer(MyAPIGateway.Session.Player.IdentityId);
	                break;
                case "StartTutorial":
	                DisableTutorialMessagesForPlayer(MyAPIGateway.Session.Player.IdentityId);
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
            {"spankemedaddy_iamnewb","StartTutorial"}
        };
        public static void InitConfigs(Dictionary<string, string> settings)
        {
            MessageDelay = settings.ContainsKey("MessageDelay")
                ? int.Parse(settings["MessageDelay"])
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
