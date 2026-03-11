using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Configuration;

namespace SkiittzsThermalMechanics.Tests
{
    [TestFixture]
    public class ConfigurationTests
    {
        private static void SetStaticField(string fieldName, object value)
        {
            var field = typeof(Configuration).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
            if (field == null)
            {
                var prop = typeof(Configuration).GetProperty(fieldName, BindingFlags.Public | BindingFlags.Static);
                prop?.SetValue(null, value);
                return;
            }
            field.SetValue(null, value);
        }

        [SetUp]
        public void SetUp()
        {
            Configuration.BlockSettings = new Dictionary<string, Dictionary<string, string>>
            {
                {
                    "LargeBlockBatteryBlock", new Dictionary<string, string>
                    {
                        { "HeatCapacity", "4800" },
                        { "PassiveCooling", "0.4" },
                        { "HeatGenerationMultiplier", "1.0" }
                    }
                },
                {
                    "SmallBlockBatteryBlock", new Dictionary<string, string>
                    {
                        { "HeatCapacity", "1600" },
                        { "PassiveCooling", "0.13" }
                    }
                },
                {
                    "BoolTestBlock", new Dictionary<string, string>
                    {
                        { "IsEnabled", "true" },
                        { "InvalidBool", "notabool" }
                    }
                }
            };

            // Initialize _disabledHudPlayerIds via reflection
            var disabledField = typeof(Configuration).GetField("_disabledHudPlayerIds", BindingFlags.NonPublic | BindingFlags.Static);
            disabledField?.SetValue(null, new List<long>());
        }

        #region TryGetBlockSettingValue(string) Tests

        [Test]
        public void TryGetBlockSettingValue_String_ExistingKey_ReturnsTrue()
        {
            string value;
            var result = Configuration.TryGetBlockSettingValue("LargeBlockBatteryBlock", "HeatCapacity", out value);
            Assert.IsTrue(result);
            Assert.AreEqual("4800", value);
        }

        [Test]
        public void TryGetBlockSettingValue_String_NonExistentBlockType_ReturnsFalse()
        {
            string value;
            var result = Configuration.TryGetBlockSettingValue("NonExistentBlock", "HeatCapacity", out value);
            Assert.IsFalse(result);
            Assert.AreEqual(string.Empty, value);
        }

        [Test]
        public void TryGetBlockSettingValue_String_NonExistentSetting_ReturnsFalse()
        {
            string value;
            var result = Configuration.TryGetBlockSettingValue("LargeBlockBatteryBlock", "NonExistentSetting", out value);
            Assert.IsFalse(result);
            Assert.AreEqual(string.Empty, value);
        }

        [Test]
        public void TryGetBlockSettingValue_String_ReturnsCorrectValue()
        {
            string value;
            Configuration.TryGetBlockSettingValue("SmallBlockBatteryBlock", "PassiveCooling", out value);
            Assert.AreEqual("0.13", value);
        }

        #endregion

        #region TryGetBlockSettingValue(float) Tests

        [Test]
        public void TryGetBlockSettingValue_Float_ExistingKey_ReturnsTrue()
        {
            float value;
            var result = Configuration.TryGetBlockSettingValue("LargeBlockBatteryBlock", "HeatCapacity", out value);
            Assert.IsTrue(result);
            Assert.AreEqual(4800f, value);
        }

        [Test]
        public void TryGetBlockSettingValue_Float_NonExistentBlockType_ReturnsFalse()
        {
            float value;
            var result = Configuration.TryGetBlockSettingValue("NonExistentBlock", "HeatCapacity", out value);
            Assert.IsFalse(result);
            Assert.AreEqual(0f, value);
        }

        [Test]
        public void TryGetBlockSettingValue_Float_NonExistentSetting_ReturnsFalse()
        {
            float value;
            var result = Configuration.TryGetBlockSettingValue("LargeBlockBatteryBlock", "FakeParam", out value);
            Assert.IsFalse(result);
            Assert.AreEqual(0f, value);
        }

        [Test]
        public void TryGetBlockSettingValue_Float_ParsesCorrectly()
        {
            float value;
            Configuration.TryGetBlockSettingValue("LargeBlockBatteryBlock", "PassiveCooling", out value);
            Assert.AreEqual(0.4f, value, 0.0001f);
        }

        #endregion

        #region TryGetBlockSettingValue(bool) Tests

        [Test]
        public void TryGetBlockSettingValue_Bool_ValidTrue_ReturnsTrue()
        {
            bool value;
            var result = Configuration.TryGetBlockSettingValue("BoolTestBlock", "IsEnabled", out value);
            Assert.IsTrue(result);
            Assert.IsTrue(value);
        }

        [Test]
        public void TryGetBlockSettingValue_Bool_InvalidBool_ReturnsFalse()
        {
            bool value;
            var result = Configuration.TryGetBlockSettingValue("BoolTestBlock", "InvalidBool", out value);
            Assert.IsFalse(result);
        }

        [Test]
        public void TryGetBlockSettingValue_Bool_NonExistentBlock_ReturnsFalse()
        {
            bool value;
            var result = Configuration.TryGetBlockSettingValue("FakeBlock", "IsEnabled", out value);
            Assert.IsFalse(result);
            Assert.IsFalse(value);
        }

        #endregion

        #region ToggleDebugMode Tests

        [Test]
        public void ToggleDebugMode_TogglesFromFalseToTrue()
        {
            SetStaticField("debugMode", false);
            Configuration.ToggleDebugMode();
            Assert.IsTrue(Configuration.DebugMode);
        }

        [Test]
        public void ToggleDebugMode_TogglesFromTrueToFalse()
        {
            SetStaticField("debugMode", true);
            Configuration.ToggleDebugMode();
            Assert.IsFalse(Configuration.DebugMode);
        }

        [Test]
        public void ToggleDebugMode_TwiceReturnsToOriginal()
        {
            SetStaticField("debugMode", false);
            Configuration.ToggleDebugMode();
            Configuration.ToggleDebugMode();
            Assert.IsFalse(Configuration.DebugMode);
        }

        #endregion

        #region PlayerHudIsDisabled Tests

        [Test]
        public void PlayerHudIsDisabled_PlayerNotInList_ReturnsFalse()
        {
            Assert.IsFalse(Configuration.PlayerHudIsDisabled(999L));
        }

        [Test]
        public void PlayerHudIsDisabled_PlayerInList_ReturnsTrue()
        {
            Configuration.DisableHudForPlayer(123L);
            Assert.IsTrue(Configuration.PlayerHudIsDisabled(123L));
        }

        [Test]
        public void PlayerHudIsDisabled_NullList_ReturnsFalse()
        {
            var field = typeof(Configuration).GetField("_disabledHudPlayerIds", BindingFlags.NonPublic | BindingFlags.Static);
            field?.SetValue(null, null);
            Assert.IsFalse(Configuration.PlayerHudIsDisabled(123L));
        }

        #endregion

        #region ToggleHudForPlayer Tests

        [Test]
        public void ToggleHudForPlayer_EnabledToDisabled()
        {
            Assert.IsFalse(Configuration.PlayerHudIsDisabled(42L));
            Configuration.ToggleHudForPlayer(42L);
            Assert.IsTrue(Configuration.PlayerHudIsDisabled(42L));
        }

        [Test]
        public void ToggleHudForPlayer_DisabledToEnabled()
        {
            Configuration.DisableHudForPlayer(42L);
            Assert.IsTrue(Configuration.PlayerHudIsDisabled(42L));
            Configuration.ToggleHudForPlayer(42L);
            Assert.IsFalse(Configuration.PlayerHudIsDisabled(42L));
        }

        [Test]
        public void ToggleHudForPlayer_TwiceReturnsToOriginal()
        {
            Assert.IsFalse(Configuration.PlayerHudIsDisabled(42L));
            Configuration.ToggleHudForPlayer(42L);
            Configuration.ToggleHudForPlayer(42L);
            Assert.IsFalse(Configuration.PlayerHudIsDisabled(42L));
        }

        #endregion

        #region DisableHudForPlayer / EnabledHudForPlayer Tests

        [Test]
        public void DisableHudForPlayer_AddsPlayer()
        {
            Configuration.DisableHudForPlayer(77L);
            Assert.IsTrue(Configuration.PlayerHudIsDisabled(77L));
        }

        [Test]
        public void EnabledHudForPlayer_RemovesPlayer()
        {
            Configuration.DisableHudForPlayer(77L);
            Configuration.EnabledHudForPlayer(77L);
            Assert.IsFalse(Configuration.PlayerHudIsDisabled(77L));
        }

        [Test]
        public void EnabledHudForPlayer_PlayerNotInList_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => Configuration.EnabledHudForPlayer(999L));
        }

        #endregion
    }
}
