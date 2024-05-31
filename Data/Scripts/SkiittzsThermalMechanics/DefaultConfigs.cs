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

            #region Heatsinks

            yield return new BlockType
            {
                SubTypeId = "LargeHeatSink",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "500000" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.01" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallHeatSink",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "HeatCapacity", Setting = "50000" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.01" }
                }
            };
            #endregion

            #region Thrusters

            #region Vanilla

            yield return new BlockType
            {
                SubTypeId = "LargeBlockLargeHydrogenThrust",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "MwHeatPerNewtonThrust", Setting = "0.000001" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.25" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockSmallHydrogenThrust",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "MwHeatPerNewtonThrust", Setting = "0.000001" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockLargeHydrogenThrust",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "MwHeatPerNewtonThrust", Setting = "0.000001" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockSmallHydrogenThrust",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "MwHeatPerNewtonThrust", Setting = "0.000001" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.0025" }
                }
            };
            #endregion
            #region Vanilla

            yield return new BlockType
            {
                SubTypeId = "LargeBlockLargeHydrogenThrustIndustrial",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "MwHeatPerNewtonThrust", Setting = "0.000001" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.25" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockSmallHydrogenThrustIndustrial",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "MwHeatPerNewtonThrust", Setting = "0.000001" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockLargeHydrogenThrustIndustrial",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "MwHeatPerNewtonThrust", Setting = "0.000001" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockSmallHydrogenThrustIndustrial",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting { Name = "MwHeatPerNewtonThrust", Setting = "0.000001" },
                    new BlockSetting { Name = "PassiveCooling", Setting = "0.0025" }
                }
            };
            #endregion

            yield return new BlockType
            {
                SubTypeId = "ChatBot",
                Settings = new List<BlockSetting>
                {
                    new BlockSetting{ Name = "ChatBotName", Setting = "ChattyMcChatface" },
                    new BlockSetting{ Name = "ChatFrequencyLimiter", Setting = "20"},
                    new BlockSetting{ Name = "ChatBotCommand_StopMessages", Setting = "stfu"},
                    new BlockSetting{ Name = "ChatBotCommand_Help", Setting = "help"},
                    new BlockSetting{ Name = "ChatBotCommand_ReEnable", Setting = "on"}
                }
            };

            #endregion
        }
    }
}
