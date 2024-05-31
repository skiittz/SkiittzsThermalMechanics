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
        private static List<BlockType> configs;
        public static Dictionary<string, Dictionary<string, string>> BlockSettings;
        public static void Load()
        {
            if (IsLoaded) return;
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(SkiittzThermalMechanicsSession)))
            {
                var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(fileName,
                    typeof(SkiittzThermalMechanicsSession));
                var content = reader.ReadToEnd();
                reader.Close();

                configs =
                    MyAPIGateway.Utilities.SerializeFromXML<List<BlockType>>(content);
            }
            else
            {
                configs = Defaults().ToList();
                Save();
            }

            BlockSettings = configs.ToDictionary(x => x.SubTypeId, x => x.Settings.ToDictionary(y => y.Name, y => y.Setting));
            IsLoaded = true;

            if (BlockSettings.ContainsKey("ChatBot"))
                ChatBot.InitConfigs(BlockSettings["ChatBot"]);
            ChatBot.LoadDisabledPlayers();
        }

        public static void Save()
        {
            var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(fileName, typeof(SkiittzThermalMechanicsSession));
            var content = configs.ToList();
            writer.Write(MyAPIGateway.Utilities.SerializeToXML(content));
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

    public class BlockType
    {
        public string SubTypeId { get; set; }
        public List<BlockSetting> Settings { get; set; }
    }

    public class BlockSetting
    {
        public string Name { get; set; }
        public string Setting { get; set; }
    }
}
