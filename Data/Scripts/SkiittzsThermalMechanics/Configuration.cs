using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public static class Configuration
    {
        private const string fileName = "BlockSettings.xml";
        public static bool IsLoaded = false;
        public static Dictionary<string,BlockType> BlockSettings;
        public static void Load()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage("BlockSettings",
                    typeof(Dictionary<string, BlockType>)))
            {
                var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(fileName,
                    typeof(List<BlockType>));
                var content = reader.ReadToEnd();
                reader.Close();

                BlockSettings =
                    MyAPIGateway.Utilities.SerializeFromXML<List<BlockType>>(content)
                        .ToDictionary(x => x.SubTypeId);
            }
            else
            {
                BlockSettings = Defaults().ToDictionary(x => x.SubTypeId);
                Save();
            }

            IsLoaded = true;
        }

        public static void Save()
        {
            var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(fileName, typeof(SkiittzThermalMechanicsSession));
            var content = BlockSettings.Select(x => x.Value).ToList();
            writer.Write(MyAPIGateway.Utilities.SerializeToXML(content));
            writer.Flush();
            writer.Close();
        }

        public static IEnumerable<BlockType> Defaults()
        {
            yield return new BlockType
            {
                SubTypeId = "SmallHeatRadiatorBlock",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting{ Name = "MaxDissipation", Setting = "5"},
                    new BlockSetting{ Name = "StepSize", Setting = ".025"}
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeHeatRadiatorBlock",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting{ Name = "MaxDissipation", Setting = "50"},
                    new BlockSetting{ Name = "StepSize", Setting = ".25"}
                }
            };
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
