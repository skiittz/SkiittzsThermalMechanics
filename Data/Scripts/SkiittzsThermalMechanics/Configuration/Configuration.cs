using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Configuration
{
    public static partial class Configuration
    {
        private const string fileName = "Settings.xml";
        private const string playerDisabledHudFileName = "PlayerDisabledHud.xml";

		public static bool IsLoaded = false;
        private static ModSettings configs;
        public static Dictionary<string, Dictionary<string, string>> BlockSettings;
        public static Dictionary<string, float> DissipationModifiers;
        public static Dictionary<string, float> SignalModifiers;
		private static bool debugMode = false;
        public static bool DebugMode => debugMode;
        private static List<long> _disabledHudPlayerIds;

		public static void ToggleDebugMode()
        {
			debugMode = !debugMode;
		}

        public static void Load()
        {
            if (IsLoaded) return;
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(SkiittzThermalMechanicsSession)))
            {
                var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(fileName,
                    typeof(SkiittzThermalMechanicsSession));
                var content = reader.ReadToEnd();
                reader.Close();

                try
                {
                    configs = MyAPIGateway.Utilities.SerializeFromXML<ModSettings>(content);

                    if (ModSettings.CurrentVersion > configs.ConfigVersion)
                    {
	                    var latestConfig = ModSettings.Default();
						configs = configs.UpgradeTo(latestConfig);
                        Save();
                    }
                }
                catch
                {
                    configs = ModSettings.Default();
                }
            }
            else
            {
                configs = ModSettings.Default();
                Save();
            }

            BlockSettings = configs.BlockTypeSettings.ToDictionary(x => x.SubTypeId, x => x.Settings.ToDictionary(y => y.Name, y => y.Value));
            DissipationModifiers =
                configs.WeatherSettings
                    .Select(x => new {x.WeatherType, x.Settings.Single(y => y.Name == "DissipationScale").Value})
                    .ToDictionary(x => x.WeatherType, x => float.Parse(x.Value));
			SignalModifiers =
				configs.WeatherSettings
					.Select(x => new { x.WeatherType, x.Settings.Single(y => y.Name == "SignalScale").Value })
					.ToDictionary(x => x.WeatherType, x => float.Parse(x.Value));

			IsLoaded = true;

            if (configs.ChatBotSettings != null)
                ChatBot.ChatBot.InitConfigs(configs.ChatBotSettings.Settings.ToDictionary(x => x.Name, x => x.Value));
            ChatBot.ChatBot.LoadDisabledPlayers();
            ChatBot.ChatBot.LoadWarningOnlyPlayers();
            ChatBot.ChatBot.LoadPlayerChatBotNameOverrides();
            LoadDisabledHudPlayers();
        }
        
        public static void Save()
        {
            var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(fileName, typeof(SkiittzThermalMechanicsSession));
            writer.Write(MyAPIGateway.Utilities.SerializeToXML(configs));
            writer.Flush();
            writer.Close();

            SavePlayersDisableHud();
        }

        
        public static void SavePlayersDisableHud()
        {
	        var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(playerDisabledHudFileName, typeof(SkiittzThermalMechanicsSession));
	        var content = _disabledHudPlayerIds;
	        writer.Write(MyAPIGateway.Utilities.SerializeToXML(content));
	        writer.Flush();
	        writer.Close();
        }

        public static void LoadDisabledHudPlayers()
        {
	        if (MyAPIGateway.Utilities.FileExistsInWorldStorage(playerDisabledHudFileName, typeof(SkiittzThermalMechanicsSession)))
	        {
		        var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(playerDisabledHudFileName, typeof(SkiittzThermalMechanicsSession));
		        var content = reader.ReadToEnd();
		        reader.Close();
		        _disabledHudPlayerIds = MyAPIGateway.Utilities.SerializeFromXML<List<long>>(content);
	        }
	        else
		        _disabledHudPlayerIds = new List<long>();
        }

        public static void DisableHudForPlayer(long playerId)
        {
	        _disabledHudPlayerIds.Add(playerId);
        }

        public static void EnabledHudForPlayer(long playerId)
        {
	        _disabledHudPlayerIds.Remove(playerId);
        }

        public static void ToggleHudForPlayer(long playerId)
        {
	        if (PlayerHudIsDisabled(playerId))
		        EnabledHudForPlayer(playerId);
	        else
		        DisableHudForPlayer(playerId);
        }

        public static bool PlayerHudIsDisabled(long playerId)
        {
	        return _disabledHudPlayerIds != null && _disabledHudPlayerIds.Contains(playerId);
        }

		public static bool TryGetBlockSettingValue(string type, string configName, out string value)
        {
            value = string.Empty;
            if (!BlockSettings.ContainsKey(type))
                return false;
            if (!BlockSettings[type].ContainsKey(configName))
                return false;

            value = BlockSettings[type][configName];
            return true;
        }

        public static bool TryGetBlockSettingValue(string type, string configName, out bool value)
        {
	        value = false;
	        if (!BlockSettings.ContainsKey(type))
		        return false;
	        if (!BlockSettings[type].ContainsKey(configName))
		        return false;

	        return bool.TryParse(BlockSettings[type][configName], out value);
        }

		public static bool TryGetBlockSettingValue(string type, string configName, out float value)
        {
            value = 0f;
            if (!BlockSettings.ContainsKey(type))
                return false;
            if (!BlockSettings[type].ContainsKey(configName))
                return false;

            string strVal;
            if (!TryGetBlockSettingValue(type, configName, out strVal))
                return false;

            if(float.TryParse(strVal, out value))
                return true;

            return false;
        }

        public static bool TryGetGeneralSettingValue(string name, out bool value)
        {
	        value = false;
	        var setting = configs.GeneralSettings.SingleOrDefault(x => x.Name == name);
	        return setting != null && bool.TryParse(setting.Value, out value);
        }
    }

    public class BlockType
    {
        public string SubTypeId { get; set; }
        public List<Setting> Settings { get; set; }
    }

    public class ChatBotSettings
    {
        public List<Setting> Settings { get; set; }
    }

    public class WeatherSetting
    {
        public string WeatherType { get; set; }
        public List<Setting> Settings { get; set; }
    }

    public class Setting
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
