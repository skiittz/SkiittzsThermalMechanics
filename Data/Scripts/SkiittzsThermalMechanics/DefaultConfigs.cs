using System.Collections.Generic;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public static partial class Configuration
    {
        #region Radiators

        public static IEnumerable<BlockType> Defaults()
        {
            yield return new BlockType
            {
                SubTypeId = "SmallHeatRadiatorBlock",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "MaxDissipation", Setting = "5" },
                    new BlockSetting { Name = "StepSize", Setting = ".025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeHeatRadiatorBlock",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "MaxDissipation", Setting = "50" },
                    new BlockSetting { Name = "StepSize", Setting = ".25" }
                }
            };

            #endregion

        #region Batteries

            #region Vanilla
            yield return new BlockType
            {
                SubTypeId = "LargeBlockBatteryBlock",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "4800" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.4" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockBatteryBlock",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "1600" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.13" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockSmallBatteryBlock",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "80" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.01" }
                }
            };
            #endregion

            #region Warfare

            yield return new BlockType
            {
                SubTypeId = "LargeBlockBatteryBlockWarfare2",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "4800" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.4" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockBatteryBlockWarfare2",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "1600" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.13" }
                }
            };

            #endregion
        #endregion
        }
    }
}
