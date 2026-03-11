using System.Linq;
using NUnit.Framework;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Configuration;

namespace SkiittzsThermalMechanics.Tests
{
    [TestFixture]
    public class ModSettingsTests
    {
        [Test]
        public void Default_SetsCurrentVersion()
        {
            var settings = ModSettings.Default();
            Assert.AreEqual(ModSettings.CurrentVersion, settings.ConfigVersion);
        }

        [Test]
        public void Default_HasBlockTypeSettings()
        {
            var settings = ModSettings.Default();
            Assert.IsNotNull(settings.BlockTypeSettings);
            Assert.IsTrue(settings.BlockTypeSettings.Count > 0);
        }

        [Test]
        public void Default_HasChatBotSettings()
        {
            var settings = ModSettings.Default();
            Assert.IsNotNull(settings.ChatBotSettings);
            Assert.IsNotNull(settings.ChatBotSettings.Settings);
            Assert.IsTrue(settings.ChatBotSettings.Settings.Count > 0);
        }

        [Test]
        public void Default_HasWeatherSettings()
        {
            var settings = ModSettings.Default();
            Assert.IsNotNull(settings.WeatherSettings);
            Assert.IsTrue(settings.WeatherSettings.Count > 0);
        }

        [Test]
        public void Default_HasGeneralSettings()
        {
            var settings = ModSettings.Default();
            Assert.IsNotNull(settings.GeneralSettings);
            Assert.IsTrue(settings.GeneralSettings.Count > 0);
        }

        [Test]
        public void Default_ContainsChatBotNameSetting()
        {
            var settings = ModSettings.Default();
            var chatBotName = settings.ChatBotSettings.Settings.SingleOrDefault(s => s.Name == "ChatBotName");
            Assert.IsNotNull(chatBotName);
            Assert.AreEqual("HotDaddy", chatBotName.Value);
        }

        [Test]
        public void Default_ContainsChatFrequencyLimiterSetting()
        {
            var settings = ModSettings.Default();
            var limiter = settings.ChatBotSettings.Settings.SingleOrDefault(s => s.Name == "ChatFrequencyLimiter");
            Assert.IsNotNull(limiter);
            Assert.AreEqual("360", limiter.Value);
        }

        [Test]
        public void Default_ContainsSmallGridShuntsToLargeGeneralSetting()
        {
            var settings = ModSettings.Default();
            var shunt = settings.GeneralSettings.SingleOrDefault(s => s.Name == "SmallGridShuntsToLarge");
            Assert.IsNotNull(shunt);
            Assert.AreEqual("true", shunt.Value);
        }

        [Test]
        public void Default_ContainsSpaceWeatherSetting()
        {
            var settings = ModSettings.Default();
            var space = settings.WeatherSettings.SingleOrDefault(w => w.WeatherType == "Space");
            Assert.IsNotNull(space);

            var dissipation = space.Settings.SingleOrDefault(s => s.Name == "DissipationScale");
            Assert.IsNotNull(dissipation);
            Assert.AreEqual("0.1", dissipation.Value);
        }

        [Test]
        public void Default_ContainsDefaultWeatherSetting()
        {
            var settings = ModSettings.Default();
            var defaultWeather = settings.WeatherSettings.SingleOrDefault(w => w.WeatherType == "Default");
            Assert.IsNotNull(defaultWeather);

            var dissipation = defaultWeather.Settings.SingleOrDefault(s => s.Name == "DissipationScale");
            Assert.IsNotNull(dissipation);
            Assert.AreEqual("1", dissipation.Value);
        }

        [Test]
        public void Default_ContainsLargeBlockBatteryBlock()
        {
            var settings = ModSettings.Default();
            var battery = settings.BlockTypeSettings.SingleOrDefault(b => b.SubTypeId == "LargeBlockBatteryBlock");
            Assert.IsNotNull(battery);

            var heatCapacity = battery.Settings.SingleOrDefault(s => s.Name == "HeatCapacity");
            Assert.IsNotNull(heatCapacity);
            Assert.AreEqual("4800", heatCapacity.Value);
        }

        [Test]
        public void Default_ContainsRadiatorBlocks()
        {
            var settings = ModSettings.Default();
            var radiator = settings.BlockTypeSettings.SingleOrDefault(b => b.SubTypeId == "SmallHeatRadiatorBlock");
            Assert.IsNotNull(radiator);

            var maxDissipation = radiator.Settings.SingleOrDefault(s => s.Name == "MaxDissipation");
            Assert.IsNotNull(maxDissipation);
            Assert.AreEqual("5", maxDissipation.Value);
        }

        [Test]
        public void Default_ContainsHeatSinkBlocks()
        {
            var settings = ModSettings.Default();
            var heatSink = settings.BlockTypeSettings.SingleOrDefault(b => b.SubTypeId == "LargeHeatSink");
            Assert.IsNotNull(heatSink);

            var heatCapacity = heatSink.Settings.SingleOrDefault(s => s.Name == "HeatCapacity");
            Assert.IsNotNull(heatCapacity);
            Assert.AreEqual("500000", heatCapacity.Value);
        }

        [Test]
        public void Default_ContainsThrusterBlocks()
        {
            var settings = ModSettings.Default();
            var thruster = settings.BlockTypeSettings.SingleOrDefault(b => b.SubTypeId == "LargeBlockLargeHydrogenThrust");
            Assert.IsNotNull(thruster);

            var mwHeat = thruster.Settings.SingleOrDefault(s => s.Name == "MwHeatPerNewtonThrust");
            Assert.IsNotNull(mwHeat);
            Assert.AreEqual("0.001", mwHeat.Value);
        }

        [Test]
        public void Default_AllBlockTypesHaveSettings()
        {
            var settings = ModSettings.Default();
            foreach (var blockType in settings.BlockTypeSettings)
            {
                Assert.IsNotNull(blockType.Settings, $"BlockType '{blockType.SubTypeId}' has null Settings");
                Assert.IsTrue(blockType.Settings.Count > 0, $"BlockType '{blockType.SubTypeId}' has no settings");
            }
        }

        [Test]
        public void Default_AllWeatherTypesHaveDissipationScale()
        {
            var settings = ModSettings.Default();
            foreach (var weather in settings.WeatherSettings)
            {
                var dissipation = weather.Settings.SingleOrDefault(s => s.Name == "DissipationScale");
                Assert.IsNotNull(dissipation, $"Weather '{weather.WeatherType}' missing DissipationScale");
            }
        }

        [Test]
        public void Default_AllWeatherTypesHaveSignalScale()
        {
            var settings = ModSettings.Default();
            foreach (var weather in settings.WeatherSettings)
            {
                var signal = weather.Settings.SingleOrDefault(s => s.Name == "SignalScale");
                Assert.IsNotNull(signal, $"Weather '{weather.WeatherType}' missing SignalScale");
            }
        }
    }
}
