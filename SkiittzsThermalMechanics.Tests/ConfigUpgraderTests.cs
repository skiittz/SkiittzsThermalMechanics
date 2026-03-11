using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Configuration;

namespace SkiittzsThermalMechanics.Tests
{
    [TestFixture]
    public class ConfigUpgraderTests
    {
        private ModSettings CreateSettings(float version, List<BlockType> blocks, ChatBotSettings chatBot, List<WeatherSetting> weather, List<Setting> general)
        {
            return new ModSettings
            {
                ConfigVersion = version,
                BlockTypeSettings = blocks ?? new List<BlockType>(),
                ChatBotSettings = chatBot ?? new ChatBotSettings { Settings = new List<Setting>() },
                WeatherSettings = weather ?? new List<WeatherSetting>(),
                GeneralSettings = general ?? new List<Setting>()
            };
        }

        #region BlockTypeSettings Upgrade Tests

        [Test]
        public void UpgradeTo_PreservesOldBlockSettingValues()
        {
            var original = CreateSettings(1.0f,
                new List<BlockType>
                {
                    new BlockType
                    {
                        SubTypeId = "LargeBlockBatteryBlock",
                        Settings = new List<Setting>
                        {
                            new Setting { Name = "HeatCapacity", Value = "9999" },
                            new Setting { Name = "PassiveCooling", Value = "0.8" }
                        }
                    }
                },
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>(),
                new List<Setting>());

            var newConfig = CreateSettings(2.0f,
                new List<BlockType>
                {
                    new BlockType
                    {
                        SubTypeId = "LargeBlockBatteryBlock",
                        Settings = new List<Setting>
                        {
                            new Setting { Name = "HeatCapacity", Value = "4800" },
                            new Setting { Name = "PassiveCooling", Value = "0.4" },
                            new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
                        }
                    }
                },
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>(),
                new List<Setting>());

            var result = original.UpgradeTo(newConfig);

            var battery = result.BlockTypeSettings.Single(x => x.SubTypeId == "LargeBlockBatteryBlock");
            Assert.AreEqual("9999", battery.Settings.Single(s => s.Name == "HeatCapacity").Value);
            Assert.AreEqual("0.8", battery.Settings.Single(s => s.Name == "PassiveCooling").Value);
        }

        [Test]
        public void UpgradeTo_AddsNewBlockSettings_WithDefaultValues()
        {
            var original = CreateSettings(1.0f,
                new List<BlockType>
                {
                    new BlockType
                    {
                        SubTypeId = "LargeBlockBatteryBlock",
                        Settings = new List<Setting>
                        {
                            new Setting { Name = "HeatCapacity", Value = "9999" }
                        }
                    }
                },
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>(),
                new List<Setting>());

            var newConfig = CreateSettings(2.0f,
                new List<BlockType>
                {
                    new BlockType
                    {
                        SubTypeId = "LargeBlockBatteryBlock",
                        Settings = new List<Setting>
                        {
                            new Setting { Name = "HeatCapacity", Value = "4800" },
                            new Setting { Name = "HeatGenerationMultiplier", Value = "1.0" }
                        }
                    }
                },
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>(),
                new List<Setting>());

            var result = original.UpgradeTo(newConfig);

            var battery = result.BlockTypeSettings.Single(x => x.SubTypeId == "LargeBlockBatteryBlock");
            Assert.AreEqual("9999", battery.Settings.Single(s => s.Name == "HeatCapacity").Value);
            Assert.AreEqual("1.0", battery.Settings.Single(s => s.Name == "HeatGenerationMultiplier").Value);
        }

        [Test]
        public void UpgradeTo_NewBlockType_KeepsDefaults()
        {
            var original = CreateSettings(1.0f,
                new List<BlockType>(),
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>(),
                new List<Setting>());

            var newConfig = CreateSettings(2.0f,
                new List<BlockType>
                {
                    new BlockType
                    {
                        SubTypeId = "NewBlock",
                        Settings = new List<Setting>
                        {
                            new Setting { Name = "Prop", Value = "DefaultVal" }
                        }
                    }
                },
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>(),
                new List<Setting>());

            var result = original.UpgradeTo(newConfig);

            var block = result.BlockTypeSettings.Single(x => x.SubTypeId == "NewBlock");
            Assert.AreEqual("DefaultVal", block.Settings.Single(s => s.Name == "Prop").Value);
        }

        #endregion

        #region ChatBotSettings Upgrade Tests

        [Test]
        public void UpgradeTo_PreservesOldChatBotSettingValues()
        {
            var original = CreateSettings(1.0f,
                new List<BlockType>(),
                new ChatBotSettings
                {
                    Settings = new List<Setting>
                    {
                        new Setting { Name = "ChatBotName", Value = "MyCustomBot" }
                    }
                },
                new List<WeatherSetting>(),
                new List<Setting>());

            var newConfig = CreateSettings(2.0f,
                new List<BlockType>(),
                new ChatBotSettings
                {
                    Settings = new List<Setting>
                    {
                        new Setting { Name = "ChatBotName", Value = "HotDaddy" },
                        new Setting { Name = "ChatFrequencyLimiter", Value = "360" }
                    }
                },
                new List<WeatherSetting>(),
                new List<Setting>());

            var result = original.UpgradeTo(newConfig);

            Assert.AreEqual("MyCustomBot", result.ChatBotSettings.Settings.Single(s => s.Name == "ChatBotName").Value);
            Assert.AreEqual("360", result.ChatBotSettings.Settings.Single(s => s.Name == "ChatFrequencyLimiter").Value);
        }

        [Test]
        public void UpgradeTo_NewChatBotSetting_KeepsDefault()
        {
            var original = CreateSettings(1.0f,
                new List<BlockType>(),
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>(),
                new List<Setting>());

            var newConfig = CreateSettings(2.0f,
                new List<BlockType>(),
                new ChatBotSettings
                {
                    Settings = new List<Setting>
                    {
                        new Setting { Name = "NewSetting", Value = "NewDefault" }
                    }
                },
                new List<WeatherSetting>(),
                new List<Setting>());

            var result = original.UpgradeTo(newConfig);

            Assert.AreEqual("NewDefault", result.ChatBotSettings.Settings.Single(s => s.Name == "NewSetting").Value);
        }

        #endregion

        #region WeatherSettings Upgrade Tests

        [Test]
        public void UpgradeTo_PreservesOldWeatherSettingValues()
        {
            var original = CreateSettings(1.0f,
                new List<BlockType>(),
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>
                {
                    new WeatherSetting
                    {
                        WeatherType = "SnowLight",
                        Settings = new List<Setting>
                        {
                            new Setting { Name = "DissipationScale", Value = "8" }
                        }
                    }
                },
                new List<Setting>());

            var newConfig = CreateSettings(2.0f,
                new List<BlockType>(),
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>
                {
                    new WeatherSetting
                    {
                        WeatherType = "SnowLight",
                        Settings = new List<Setting>
                        {
                            new Setting { Name = "DissipationScale", Value = "4" },
                            new Setting { Name = "SignalScale", Value = "4" }
                        }
                    }
                },
                new List<Setting>());

            var result = original.UpgradeTo(newConfig);

            var snow = result.WeatherSettings.Single(w => w.WeatherType == "SnowLight");
            Assert.AreEqual("8", snow.Settings.Single(s => s.Name == "DissipationScale").Value);
            Assert.AreEqual("4", snow.Settings.Single(s => s.Name == "SignalScale").Value);
        }

        [Test]
        public void UpgradeTo_NewWeatherType_KeepsDefaults()
        {
            var original = CreateSettings(1.0f,
                new List<BlockType>(),
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>(),
                new List<Setting>());

            var newConfig = CreateSettings(2.0f,
                new List<BlockType>(),
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>
                {
                    new WeatherSetting
                    {
                        WeatherType = "NewWeather",
                        Settings = new List<Setting>
                        {
                            new Setting { Name = "DissipationScale", Value = "2.5" }
                        }
                    }
                },
                new List<Setting>());

            var result = original.UpgradeTo(newConfig);

            var weather = result.WeatherSettings.Single(w => w.WeatherType == "NewWeather");
            Assert.AreEqual("2.5", weather.Settings.Single(s => s.Name == "DissipationScale").Value);
        }

        #endregion

        #region GeneralSettings Upgrade Tests

        [Test]
        public void UpgradeTo_PreservesOldGeneralSettingValues()
        {
            var original = CreateSettings(1.0f,
                new List<BlockType>(),
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>(),
                new List<Setting>
                {
                    new Setting { Name = "SmallGridShuntsToLarge", Value = "false" }
                });

            var newConfig = CreateSettings(2.0f,
                new List<BlockType>(),
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>(),
                new List<Setting>
                {
                    new Setting { Name = "SmallGridShuntsToLarge", Value = "true" },
                    new Setting { Name = "NewSetting", Value = "default" }
                });

            var result = original.UpgradeTo(newConfig);

            Assert.AreEqual("false", result.GeneralSettings.Single(s => s.Name == "SmallGridShuntsToLarge").Value);
            Assert.AreEqual("default", result.GeneralSettings.Single(s => s.Name == "NewSetting").Value);
        }

        [Test]
        public void UpgradeTo_ReturnsNewConfigObject()
        {
            var original = CreateSettings(1.0f,
                new List<BlockType>(),
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>(),
                new List<Setting>());

            var newConfig = CreateSettings(2.0f,
                new List<BlockType>(),
                new ChatBotSettings { Settings = new List<Setting>() },
                new List<WeatherSetting>(),
                new List<Setting>());

            var result = original.UpgradeTo(newConfig);

            Assert.AreSame(newConfig, result);
        }

        #endregion
    }
}
