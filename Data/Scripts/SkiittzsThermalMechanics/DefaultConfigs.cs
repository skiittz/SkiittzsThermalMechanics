using System.Collections.Generic;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public static partial class Configuration
    {
        
        public static IEnumerable<BlockType> DefaultBlockSettings()
        {
            #region Basic Radiators

            #region DLC
            yield return new BlockType
            {
                SubTypeId = "BasicSmallHeatRadiatorBlock",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MaxDissipation", Value = "0.1" },
                    new Setting { Name = "StepSize", Value = "0.0005" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "BasicLargeHeatRadiatorBlock",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MaxDissipation", Value = "1" },
                    new Setting { Name = "StepSize", Value = "0.005" }
                }
            };
            #endregion

            #region NonDlc

            yield return new BlockType
            {
                SubTypeId = "BasicSmallHeatRadiatorBlockUgly",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MaxDissipation", Value = "0.1" },
                    new Setting { Name = "StepSize", Value = "0.0005" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "BasicLargeHeatRadiatorBlockUgly",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MaxDissipation", Value = "1" },
                    new Setting { Name = "StepSize", Value = "0.005" }
                }
            };

            #endregion
            #endregion

            #region Radiators

            #region DLC
            yield return new BlockType
            {
                SubTypeId = "SmallHeatRadiatorBlock",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MaxDissipation", Value = "5" },
                    new Setting { Name = "StepSize", Value = "0.025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeHeatRadiatorBlock",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MaxDissipation", Value = "50" },
                    new Setting { Name = "StepSize", Value = "0.25" }
                }
            };
            #endregion

            #region NonDlc

            yield return new BlockType
            {
                SubTypeId = "SmallHeatRadiatorBlockUgly",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MaxDissipation", Value = "5" },
                    new Setting { Name = "StepSize", Value = "0.025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeHeatRadiatorBlockUgly",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MaxDissipation", Value = "50" },
                    new Setting { Name = "StepSize", Value = "0.25" }
                }
            };

            #endregion
            #endregion

            #region Batteries

            #region Vanilla
            yield return new BlockType
            {
                SubTypeId = "LargeBlockBatteryBlock",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "4800" },
                    new Setting { Name = "PassiveCooling", Value = "0.4" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockBatteryBlock",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "1600" },
                    new Setting { Name = "PassiveCooling", Value = "0.13" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockSmallBatteryBlock",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "80" },
                    new Setting { Name = "PassiveCooling", Value = "0.01" }
                }
            };
            #endregion

            #region Warfare

            yield return new BlockType
            {
                SubTypeId = "LargeBlockBatteryBlockWarfare2",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "4800" },
                    new Setting { Name = "PassiveCooling", Value = "0.4" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockBatteryBlockWarfare2",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "1600" },
                    new Setting { Name = "PassiveCooling", Value = "0.13" }
                }
            };

            #endregion
            #endregion

            #region Reactors
            #region Vanilla

            yield return new BlockType
            {
                SubTypeId = "SmallBlockSmallGenerator",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "50" },
                    new Setting { Name = "PassiveCooling", Value = "0.0008" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockLargeGenerator",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "1475" },
                    new Setting { Name = "PassiveCooling", Value = "0.02458" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockSmallGenerator",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "1500" },
                    new Setting { Name = "PassiveCooling", Value = "0.025" }
                }
            }; 
            
            yield return new BlockType
            {
                SubTypeId = "LargeBlockLargeGenerator",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "30000" },
                    new Setting { Name = "PassiveCooling", Value = "0.5" }
                }
            };
            #endregion
            #region Warfare 2

            yield return new BlockType
            {
                SubTypeId = "SmallBlockSmallGeneratorWarfare2",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "50" },
                    new Setting { Name = "PassiveCooling", Value = "0.0008" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockLargeGeneratorWarfare2",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "1475" },
                    new Setting { Name = "PassiveCooling", Value = "0.02458" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockSmallGeneratorWarfare2",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "1500" },
                    new Setting { Name = "PassiveCooling", Value = "0.025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockLargeGeneratorWarfare2",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "30000" },
                    new Setting { Name = "PassiveCooling", Value = "0.5" }
                }
            };
            #endregion
            #endregion

            #region H2 Engines

            yield return new BlockType
            {
                SubTypeId = "LargeHydrogenEngine",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "2500" },
                    new Setting { Name = "PassiveCooling", Value = "1" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallHydrogenEngine",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "250" },
                    new Setting { Name = "PassiveCooling", Value = "0.1" }
                }
            };

            #endregion

            #region Heatsinks

            #region DLC
            yield return new BlockType
            {
                SubTypeId = "LargeHeatSink",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "500000" },
                    new Setting { Name = "PassiveCooling", Value = "0.01" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallHeatSink",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "50000" },
                    new Setting { Name = "PassiveCooling", Value = "0.01" }
                }
            };
            #endregion

            #region NonDlc

            yield return new BlockType
            {
                SubTypeId = "LargeHeatSinkUgly",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "500000" },
                    new Setting { Name = "PassiveCooling", Value = "0.01" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallHeatSinkUgly",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "50000" },
                    new Setting { Name = "PassiveCooling", Value = "0.01" }
                }
            };

            #endregion
            #endregion

            #region Thrusters

            #region Vanilla

            yield return new BlockType
            {
                SubTypeId = "LargeBlockLargeHydrogenThrust",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.000001" },
                    new Setting { Name = "PassiveCooling", Value = "0.25" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockSmallHydrogenThrust",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.000001" },
                    new Setting { Name = "PassiveCooling", Value = "0.025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockLargeHydrogenThrust",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.000001" },
                    new Setting { Name = "PassiveCooling", Value = "0.025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockSmallHydrogenThrust",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.000001" },
                    new Setting { Name = "PassiveCooling", Value = "0.0025" }
                }
            };
            #endregion
            #region DLC

            yield return new BlockType
            {
                SubTypeId = "LargeBlockLargeHydrogenThrustIndustrial",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.000001" },
                    new Setting { Name = "PassiveCooling", Value = "0.25" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockSmallHydrogenThrustIndustrial",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.000001" },
                    new Setting { Name = "PassiveCooling", Value = "0.025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockLargeHydrogenThrustIndustrial",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.000001" },
                    new Setting { Name = "PassiveCooling", Value = "0.025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockSmallHydrogenThrustIndustrial",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.000001" },
                    new Setting { Name = "PassiveCooling", Value = "0.0025" }
                }
            };
            #endregion

            #endregion
        }

        public static ChatBotSettings DefaultChatBotSettings()
        {
            return new ChatBotSettings
            {
                Settings = new List<Setting>
                {
                    new Setting { Name = "ChatBotName", Value = "AssemblerDaddy_OreGasm69" },
                    new Setting { Name = "ChatFrequencyLimiter", Value = "20" },
                    new Setting { Name = "ChatBotCommand_StopMessages", Value = "stfu" },
                    new Setting { Name = "ChatBotCommand_Help", Value = "help" },
                    new Setting { Name = "ChatBotCommand_ReEnable", Value = "on" },
                    new Setting { Name = "ChatBotCommand_Reload", Value = "reload"},
                    new Setting {Name = "ChatBotCommand_StopTutorial", Value = "iamnotanewb"},
                    new Setting {Name = "ChatBotCommand_StartTutorial", Value = "spankemedaddy_iamnewb"}
				}
			};
        }

        public static IEnumerable<WeatherSetting> DefaultWeatherSettings()
        {
            yield return new WeatherSetting
            {
                WeatherType = "SnowLight",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.25"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "MarsSnow",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.25"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "SnowHeavy",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.1"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "FogLight",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.8"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "FogHeavy",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.7"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "AlienFogLight",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.8"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "AlienFogHeavy",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.7"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "MarsStormLight",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "2.0"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "MarsStormHeavy",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "2.0"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "RainLight",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.9"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "RainHeavy",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.8"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "AlienRainLight",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.8"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "AlienRainHeavy",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.7"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "SandStormLight",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "2.0"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "SandStormHeavy",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "2.0"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "ElectricStorm",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "2.0"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "ThunderstormLight",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.8"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "ThunderstormHeavy",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.7"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "AlienThunderstormLight",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.8"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "AlienThunderstormHeavy",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "0.7"
                    }
                }
            };

            yield return new WeatherSetting
            {
                WeatherType = "Dust",
                Settings = new List<Setting>
                {
                    new Setting
                    {
                        Name = "TempScale",
                        Value = "1.35"
                    }
                }
            };
        }
    }
}
