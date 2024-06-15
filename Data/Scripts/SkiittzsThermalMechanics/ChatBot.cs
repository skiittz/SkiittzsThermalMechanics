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
        private static List<long> _introducedPlayersThisSession = new List<long>();

        public static void WarnPlayer(IMyTerminalBlock block, string message)
        {
            var playerId = Utilities.TryGetCurrentPlayerId();
            if (block == null || !block.IsOwnedByCurrentPlayer() || _disabledPlayerIds.Contains(playerId))
                return;
            if (_messageAttemptCounter == 0)
            {
                MyAPIGateway.Utilities.ShowMessage(ChatBotName, $"{block.CubeGrid.CustomName}-{block.CustomName}: {message}");
            }

            _messageAttemptCounter++;
            if (_messageAttemptCounter == MessageDelay)
                _messageAttemptCounter = 0;
        }

        public static void WarnPlayer(IMyCubeGrid grid, string message)
        {
            var playerId = Utilities.TryGetCurrentPlayerId();
            if (!grid.BigOwners.Contains(playerId) || _disabledPlayerIds.Contains(playerId))
                return;
            if (_messageAttemptCounter == 0)
            {
                MyAPIGateway.Utilities.ShowMessage(ChatBotName, $"{grid.CustomName}: {message}");
            }

            _messageAttemptCounter++;
            if (_messageAttemptCounter == MessageDelay)
                _messageAttemptCounter = 0;
        }

        private const string fileName = "ChatBotDisabledPlayerIds.xml";
        private static void SaveDisabledPlayers()
        {
            var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(fileName, typeof(SkiittzThermalMechanicsSession));
            var content = _disabledPlayerIds;
            writer.Write(MyAPIGateway.Utilities.SerializeToXML(content));
            writer.Flush();
            writer.Close();
        }

        public static void LoadDisabledPlayers()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(SkiittzThermalMechanicsSession)))
            {
                var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(fileName, typeof(SkiittzThermalMechanicsSession));
                var content = reader.ReadToEnd();
                reader.Close();
                _disabledPlayerIds = MyAPIGateway.Utilities.SerializeFromXML<List<long>>(content);
            }
            else
                _disabledPlayerIds = new List<long>();
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
                        message.AppendLine($"{prefix}:Turn off messages from {ChatBotName}");
                        break;
                    case "ReEnable":
                        message.AppendLine($"{prefix}:Turn on messages from {ChatBotName}");
                        break;
                }
            }
            MyAPIGateway.Utilities.ShowMessage(ChatBotName, message.ToString());

        }

        private static Dictionary<string, string> commandMappings = new Dictionary<string, string>
        {
            {"stfu","StopMessages"},
            {"help","Help"}
        };
        public static void InitConfigs(Dictionary<string, string> settings)
        {
            MessageDelay = settings.ContainsKey("MessageDelay")
                ? int.Parse(settings["MessageDelay"])
                : 0;
            ChatBotName = settings.ContainsKey("ChatBotName")
                ? settings["ChatBotName"]
                : "ArseBot8000";
            foreach (var command in settings.Where(x => x.Key.StartsWith("ChatBotCommand_")))
            {
                commandMappings[command.Value] = command.Key.Replace("ChatBotCommand_", "");
            }

            MyAPIGateway.Session.OnSessionReady += IntroduceMyself;
        }
    }
}
