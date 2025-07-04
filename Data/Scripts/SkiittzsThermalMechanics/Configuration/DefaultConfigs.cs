using System.Collections.Generic;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Configuration
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
                    new Setting { Name = "StepSize", Value = "0.0005" },
                    new Setting { Name = "ForwardFace", Value = "Up"}
                }
            };

            yield return new BlockType
            {
                SubTypeId = "BasicLargeHeatRadiatorBlock",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MaxDissipation", Value = "1" },
                    new Setting { Name = "StepSize", Value = "0.005" },
                    new Setting { Name = "ForwardFace", Value = "Up"}
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
                    new Setting { Name = "StepSize", Value = "0.0005" },
                    new Setting { Name = "ForwardFace", Value = "Forward"}
				}
            };

            yield return new BlockType
            {
                SubTypeId = "BasicLargeHeatRadiatorBlockUgly",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MaxDissipation", Value = "1" },
                    new Setting { Name = "StepSize", Value = "0.005" },
                    new Setting { Name = "ForwardFace", Value = "Forward"}
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
                    new Setting { Name = "StepSize", Value = "0.025" },
                    new Setting { Name = "ForwardFace", Value = "Up"}
				}
            };

            yield return new BlockType
            {
                SubTypeId = "LargeHeatRadiatorBlock",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MaxDissipation", Value = "50" },
                    new Setting { Name = "StepSize", Value = "0.25" },
                    new Setting { Name = "ForwardFace", Value = "Up"}
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
                    new Setting { Name = "StepSize", Value = "0.025" },
                    new Setting { Name = "ForwardFace", Value = "Forward"}
				}
            };

            yield return new BlockType
            {
                SubTypeId = "LargeHeatRadiatorBlockUgly",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MaxDissipation", Value = "50" },
                    new Setting { Name = "StepSize", Value = "0.25" },
                    new Setting { Name = "ForwardFace", Value = "Forward"}
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
                    new Setting { Name = "HeatCapacity", Value = "4800", Description = "How much heat capacity before overheating begins"},
                    new Setting { Name = "PassiveCooling", Value = "0.4", Description = "How much heat can be passively dissipated without a sink or radiator"},
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0", Description = "Multiplier that scales heat production."}
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockBatteryBlock",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "1600" },
                    new Setting { Name = "PassiveCooling", Value = "0.13" },
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
				}
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockSmallBatteryBlock",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "80" },
                    new Setting { Name = "PassiveCooling", Value = "0.01" },
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
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
                    new Setting { Name = "PassiveCooling", Value = "0.4" },
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
				}
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockBatteryBlockWarfare2",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "1600" },
                    new Setting { Name = "PassiveCooling", Value = "0.13" },
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
				}
            };

			#endregion

			#region Prototech
			yield return new BlockType
			{
				SubTypeId = "LargeBlockPrototechBattery",
				Settings = new List<Setting>
				{
					new Setting { Name = "HeatCapacity", Value = "4800" },
					new Setting { Name = "PassiveCooling", Value = "0.4" },
					new Setting { Name = "HeatGenerationMultiplier", Value = "0.8" }
				}
			};

			yield return new BlockType
			{
				SubTypeId = "SmallBlockPrototechBattery",
				Settings = new List<Setting>
				{
					new Setting { Name = "HeatCapacity", Value = "1600" },
					new Setting { Name = "PassiveCooling", Value = "0.13" },
					new Setting { Name = "HeatGenerationMultiplier", Value = "0.8" }
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
                    new Setting { Name = "PassiveCooling", Value = "0.0008" },
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
				}
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockLargeGenerator",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "1475" },
                    new Setting { Name = "PassiveCooling", Value = "0.02458" },
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
				}
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockSmallGenerator",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "1500" },
                    new Setting { Name = "PassiveCooling", Value = "0.025" },
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
				}
            }; 
            
            yield return new BlockType
            {
                SubTypeId = "LargeBlockLargeGenerator",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "30000" },
                    new Setting { Name = "PassiveCooling", Value = "0.5" },
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
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
                    new Setting { Name = "PassiveCooling", Value = "0.0008" },
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
				}
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockLargeGeneratorWarfare2",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "1475" },
                    new Setting { Name = "PassiveCooling", Value = "0.02458" },
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
				}
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockSmallGeneratorWarfare2",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "1500" },
                    new Setting { Name = "PassiveCooling", Value = "0.025" },
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
				}
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockLargeGeneratorWarfare2",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "30000" },
                    new Setting { Name = "PassiveCooling", Value = "0.5" },
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
				}
            };
            #endregion
            #endregion

            #region H2 Engines

            #region Vanilla
			yield return new BlockType
            {
                SubTypeId = "LargeHydrogenEngine",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "2500" },
                    new Setting { Name = "PassiveCooling", Value = "1" },
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
				}
            };

            yield return new BlockType
            {
                SubTypeId = "SmallHydrogenEngine",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "250" },
                    new Setting { Name = "PassiveCooling", Value = "0.1" },
                    new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
				}
            };
			#endregion
			#region Prototech
			yield return new BlockType
			{
				SubTypeId = "LargePrototechReactor",
				Settings = new List<Setting>
				{
					new Setting { Name = "HeatCapacity", Value = "30000" },
					new Setting { Name = "PassiveCooling", Value = "0.5" },
					new Setting { Name = "HeatGenerationMultiplier", Value = "0.8" }
				}
			};
			#endregion
			#endregion

			#region Heatsinks

			#region DLC
			yield return new BlockType
            {
                SubTypeId = "LargeHeatSink",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "500000" },
                    new Setting { Name = "PassiveCooling", Value = "0.01" },
                    new Setting { Name = "SignalDecay", Value = "0.9992", Description = "each tick, signal radius = (current radius * signal decay) + newly vented heat (must be less than 1 or signals will NEVER reduce)"}
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallHeatSink",
                Settings = new List<Setting>
                {
                    new Setting { Name = "HeatCapacity", Value = "50000" },
                    new Setting { Name = "PassiveCooling", Value = "0.01" },
                    new Setting { Name = "SignalDecay", Value = "0.9992", Description = "each tick, signal radius = (current radius * signal decay) + newly vented heat (must be less than 1 or signals will NEVER reduce)"}
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
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.01" },
                    new Setting { Name = "PassiveCooling", Value = "0.0025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockSmallHydrogenThrust",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.01" },
                    new Setting { Name = "PassiveCooling", Value = "0.00025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockLargeHydrogenThrust",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.01" },
                    new Setting { Name = "PassiveCooling", Value = "0.00025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockSmallHydrogenThrust",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.01" },
                    new Setting { Name = "PassiveCooling", Value = "0.000025" }
                }
            };
            #endregion
            #region DLC

            yield return new BlockType
            {
                SubTypeId = "LargeBlockLargeHydrogenThrustIndustrial",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.01" },
                    new Setting { Name = "PassiveCooling", Value = "0.0025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "LargeBlockSmallHydrogenThrustIndustrial",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.01" },
                    new Setting { Name = "PassiveCooling", Value = "0.00025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockLargeHydrogenThrustIndustrial",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.01" },
                    new Setting { Name = "PassiveCooling", Value = "0.00025" }
                }
            };

            yield return new BlockType
            {
                SubTypeId = "SmallBlockSmallHydrogenThrustIndustrial",
                Settings = new List<Setting>
                {
                    new Setting { Name = "MwHeatPerNewtonThrust", Value = "0.01" },
                    new Setting { Name = "PassiveCooling", Value = "0.000025" }
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
                    new Setting { Name = "ChatBotName", Value = "HotDaddy" },
                    new Setting { Name = "ChatFrequencyLimiter", Value = "360", Description = "Number of cycles that must pass before another message is allowed"},
                    new Setting { Name = "ChatBotCommand_StopMessages", Value = "stfu" },
                    new Setting { Name = "ChatBotCommand_Help", Value = "help" },
                    new Setting { Name = "ChatBotCommand_ReEnable", Value = "on" },
                    new Setting { Name = "ChatBotCommand_Reload", Value = "reload"},
                    new Setting {Name = "ChatBotCommand_StopTutorial", Value = "iamnotanewb"},
                    new Setting {Name = "ChatBotCommand_StartTutorial", Value = "spankmedaddy_iamnewb"},
                    new Setting {Name = "ChatBotCommand_ToggleDebug", Value="debug"},
                    new Setting {Name = "ChatBotCommand_ToggleHud", Value = "togglehud"}
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
	                    Name = "DissipationScale",
	                    Value = "4",
	                    Description = "Multiplier applied to heat dissipation"
                    },
					new Setting
					{
						Name = "SignalScale",
						Value = "4",
						Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "4",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "4",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "10",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "10",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "1.25",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.25",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "1.43",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.43",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "1.25",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.25",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "1.43",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.43",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "0.5",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.0",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "0.5",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.0",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "1.11",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.0",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "1.25",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.15",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "1.25",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.15",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "1.43",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.25",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "0.75",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "0.75",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "0.5",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "0.5",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "0.5",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.0",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "1.25",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.0",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "1.43",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.0",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "1.25",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.0",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "1.43",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "1.0",
	                    Description = "Multiplier applied to signal range"
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
	                    Name = "DissipationScale",
	                    Value = "0.75",
	                    Description = "Multiplier applied to heat dissipation"
                    },
                    new Setting
                    {
	                    Name = "SignalScale",
	                    Value = "0.75",
	                    Description = "Multiplier applied to signal range"
                    }
				}
            };

			yield return new WeatherSetting
			{
				WeatherType = "Space",
				Settings = new List<Setting>
				{
					new Setting
					{
						Name = "DissipationScale",
						Value = "0.1",
						Description = "Multiplier applied to heat dissipation"
					},
					new Setting
					{
						Name = "SignalScale",
						Value = "10",
						Description = "Multiplier applied to signal range"
					}
				}
			};

			yield return new WeatherSetting
			{
				WeatherType = "Default",
				Settings = new List<Setting>
				{
					new Setting
					{
						Name = "DissipationScale",
						Value = "1",
						Description = "Multiplier applied to heat dissipation"
					},
					new Setting
					{
						Name = "SignalScale",
						Value = "1",
						Description = "Multiplier applied to signal range"
					}
				}
			};
		}

        public static IEnumerable<Setting> DefaultGeneralSettings()
        {
	        yield return new Setting
	        {
		        Name = "SmallGridShuntsToLarge",
		        Description =
			        "true/false - when true, small grids docked to a large grid will shunt their accumulated heat into the large grid.",
		        Value = "true"
	        };
        }
    }
}
