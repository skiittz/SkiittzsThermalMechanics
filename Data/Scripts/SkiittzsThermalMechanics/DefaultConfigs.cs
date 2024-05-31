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

            #region Reactors
            #region Vanilla

            yield return new BlockType
            {
                SubTypeId = "SmallBlockSmallGenerator",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "50" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.0008" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockLargeGenerator",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "1475" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.02458" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockSmallGenerator",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "1500" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.025" }
                }
            }; 
            
            yield return new BlockType
            {
                SubTypeId = "LargeBlockLargeGenerator",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "30000" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.5" }
                }
            };
            #endregion
            #region Warfare 2

            yield return new BlockType
            {
                SubTypeId = "SmallBlockSmallGeneratorWarfare2",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "50" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.0008" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockLargeGeneratorWarfare2",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "1475" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.02458" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockSmallGeneratorWarfare2",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "1500" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockLargeGeneratorWarfare2",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "30000" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.5" }
                }
            };
            #endregion
            #endregion

            #region H2 Engines

            yield return new BlockType
            {
                SubTypeId = "LargeHydrogenEngine",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "2500" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "1" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallHydrogenEngine",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "250" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.1" }
                }
            };

            #endregion
        }
    }
}
