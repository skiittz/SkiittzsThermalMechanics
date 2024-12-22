using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public static partial class Configuration
    {
        private const string fileName = "Settings.xml";
        public static bool IsLoaded = false;
        private static ModSettings configs;
        public static Dictionary<string, Dictionary<string, string>> BlockSettings;
        public static Dictionary<string, float> WeatherSettings;
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
                    configs =
                        MyAPIGateway.Utilities.SerializeFromXML<ModSettings>(content);
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
            WeatherSettings =
                configs.WeatherSettings
                    .Select(x => new {x.WeatherType, x.Settings.Single(y => y.Name == "TempScale").Value})
                    .ToDictionary(x => x.WeatherType, x => float.Parse(x.Value));
            IsLoaded = true;

            if (configs.ChatBotSettings != null)
                ChatBot.InitConfigs(configs.ChatBotSettings.Settings.ToDictionary(x => x.Name, x => x.Value));
            ChatBot.LoadDisabledPlayers();
            ChatBot.LoadWarningOnlyPlayers();
            ChatBot.LoadPlayerChatBotNameOverrides();
        }
        
        public static void Save()
        {
            var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(fileName, typeof(SkiittzThermalMechanicsSession));
            writer.Write(MyAPIGateway.Utilities.SerializeToXML(configs));
            writer.Flush();
            writer.Close();
        }

        public static bool TryGetValue(string type, string configName, out string value)
        {
            value = string.Empty;
            if (!BlockSettings.ContainsKey(type))
                return false;
            if (!BlockSettings[type].ContainsKey(configName))
                return false;

            value = BlockSettings[type][configName];
            return true;
        }

        public static bool TryGetValue(string type, string configName, out float value)
        {
            value = 0f;
            if (!BlockSettings.ContainsKey(type))
                return false;
            if (!BlockSettings[type].ContainsKey(configName))
                return false;

            string strVal;
            if (!TryGetValue(type, configName, out strVal))
                return false;

            if(float.TryParse(strVal, out value))
                return true;

            return false;
        }
    }

    public class ModSettings
    {
        public List<BlockType> BlockTypeSettings { get; set; }
        public ChatBotSettings ChatBotSettings { get; set; }
        public List<WeatherSetting> WeatherSettings { get; set; }

        public static ModSettings Default()
        {
            return new ModSettings
            {
                BlockTypeSettings = Configuration.DefaultBlockSettings().ToList(),
                ChatBotSettings = Configuration.DefaultChatBotSettings(),
                WeatherSettings = Configuration.DefaultWeatherSettings().ToList()
            };
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
