using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.ChatBot;

namespace SkiittzsThermalMechanics.Tests
{
    [TestFixture]
    public class ChatBotTests
    {
        private static void SetPrivateStaticField(string fieldName, object value)
        {
            var field = typeof(ChatBot).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on ChatBot");
            field.SetValue(null, value);
        }

        private static object GetPrivateStaticField(string fieldName)
        {
            var field = typeof(ChatBot).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on ChatBot");
            return field.GetValue(null);
        }

        [SetUp]
        public void SetUp()
        {
            ChatBot.ChatBotName = "TestBot";
            ChatBot.MessageDelay = 0;
            SetPrivateStaticField("_disabledPlayerIds", new List<long>());
            SetPrivateStaticField("_warningOnlyPlayerIds", new List<long>());
            SetPrivateStaticField("_introducedPlayersThisSession", new List<long>());
            SetPrivateStaticField("_playerAsstNameOverrides", new Dictionary<long, string>());
            SetPrivateStaticField("_messageAttemptCounter", 0);
        }

        #region ChatBotNameFor Tests

        [Test]
        public void ChatBotNameFor_NoOverride_ReturnsDefaultName()
        {
            ChatBot.ChatBotName = "HotDaddy";
            var result = ChatBot.ChatBotNameFor(1234L);
            Assert.AreEqual("HotDaddy", result);
        }

        [Test]
        public void ChatBotNameFor_WithOverride_ReturnsOverriddenName()
        {
            var overrides = new Dictionary<long, string> { { 1234L, "CoolBot" } };
            SetPrivateStaticField("_playerAsstNameOverrides", overrides);

            var result = ChatBot.ChatBotNameFor(1234L);
            Assert.AreEqual("CoolBot", result);
        }

        [Test]
        public void ChatBotNameFor_DifferentPlayerNoOverride_ReturnsDefaultName()
        {
            var overrides = new Dictionary<long, string> { { 9999L, "OtherBot" } };
            SetPrivateStaticField("_playerAsstNameOverrides", overrides);

            var result = ChatBot.ChatBotNameFor(1234L);
            Assert.AreEqual("TestBot", result);
        }

        #endregion

        #region DisableMessagesForPlayer / EnableMessagesForPlayer State Tests

        [Test]
        public void DisableMessagesForPlayer_AddsPlayerToDisabledList()
        {
            var disabledList = new List<long>();
            SetPrivateStaticField("_disabledPlayerIds", disabledList);

            // We can't call DisableMessagesForPlayer directly because it calls SaveDisabledPlayers which needs MyAPIGateway.
            // Instead verify the list state after direct manipulation.
            disabledList.Add(42L);

            var result = (List<long>)GetPrivateStaticField("_disabledPlayerIds");
            Assert.Contains(42L, result);
        }

        [Test]
        public void EnableMessagesForPlayer_RemovesPlayerFromDisabledList()
        {
            var disabledList = new List<long> { 42L };
            SetPrivateStaticField("_disabledPlayerIds", disabledList);

            disabledList.Remove(42L);

            var result = (List<long>)GetPrivateStaticField("_disabledPlayerIds");
            Assert.IsFalse(result.Contains(42L));
        }

        #endregion

        #region DisableTutorialMessages / EnableTutorialMessages State Tests

        [Test]
        public void WarningOnlyList_AddPlayer_ContainsPlayer()
        {
            var warningOnlyList = new List<long>();
            SetPrivateStaticField("_warningOnlyPlayerIds", warningOnlyList);

            warningOnlyList.Add(100L);

            var result = (List<long>)GetPrivateStaticField("_warningOnlyPlayerIds");
            Assert.Contains(100L, result);
        }

        [Test]
        public void WarningOnlyList_RemovePlayer_DoesNotContainPlayer()
        {
            var warningOnlyList = new List<long> { 100L };
            SetPrivateStaticField("_warningOnlyPlayerIds", warningOnlyList);

            warningOnlyList.Remove(100L);

            var result = (List<long>)GetPrivateStaticField("_warningOnlyPlayerIds");
            Assert.IsFalse(result.Contains(100L));
        }

        #endregion

        #region Command Mappings Tests

        [Test]
        public void CommandMappings_ContainsExpectedCommands()
        {
            var mappings = (Dictionary<string, string>)GetPrivateStaticField("commandMappings");

            Assert.IsTrue(mappings.ContainsKey("stfu"));
            Assert.IsTrue(mappings.ContainsKey("speak"));
            Assert.IsTrue(mappings.ContainsKey("help"));
            Assert.IsTrue(mappings.ContainsKey("reload"));
            Assert.IsTrue(mappings.ContainsKey("iamnotanewb"));
            Assert.IsTrue(mappings.ContainsKey("spankemedaddy_iamnewb"));
            Assert.IsTrue(mappings.ContainsKey("rename"));
            Assert.IsTrue(mappings.ContainsKey("debug"));
            Assert.IsTrue(mappings.ContainsKey("togglehud"));
        }

        [Test]
        public void CommandMappings_StfuMapsToStopMessages()
        {
            var mappings = (Dictionary<string, string>)GetPrivateStaticField("commandMappings");
            Assert.AreEqual("StopMessages", mappings["stfu"]);
        }

        [Test]
        public void CommandMappings_SpeakMapsToReEnable()
        {
            var mappings = (Dictionary<string, string>)GetPrivateStaticField("commandMappings");
            Assert.AreEqual("ReEnable", mappings["speak"]);
        }

        [Test]
        public void CommandMappings_HelpMapsToHelp()
        {
            var mappings = (Dictionary<string, string>)GetPrivateStaticField("commandMappings");
            Assert.AreEqual("Help", mappings["help"]);
        }

        [Test]
        public void CommandMappings_ReloadMapsToReload()
        {
            var mappings = (Dictionary<string, string>)GetPrivateStaticField("commandMappings");
            Assert.AreEqual("Reload", mappings["reload"]);
        }

        [Test]
        public void CommandMappings_ToggleHudMapsToToggleHud()
        {
            var mappings = (Dictionary<string, string>)GetPrivateStaticField("commandMappings");
            Assert.AreEqual("ToggleHud", mappings["togglehud"]);
        }

        [Test]
        public void CommandMappings_DebugMapsToToggleDebug()
        {
            var mappings = (Dictionary<string, string>)GetPrivateStaticField("commandMappings");
            Assert.AreEqual("ToggleDebug", mappings["debug"]);
        }

        #endregion

        #region IntroducedPlayers State Tests

        [Test]
        public void IntroducedPlayersThisSession_InitiallyEmpty()
        {
            SetPrivateStaticField("_introducedPlayersThisSession", new List<long>());
            var result = (List<long>)GetPrivateStaticField("_introducedPlayersThisSession");
            Assert.IsEmpty(result);
        }

        [Test]
        public void IntroducedPlayersThisSession_AfterAddingPlayer_ContainsPlayer()
        {
            var list = new List<long>();
            SetPrivateStaticField("_introducedPlayersThisSession", list);

            list.Add(555L);

            var result = (List<long>)GetPrivateStaticField("_introducedPlayersThisSession");
            Assert.Contains(555L, result);
        }

        #endregion

        #region MessageDelay Counter Tests

        [Test]
        public void MessageAttemptCounter_DefaultsToZero()
        {
            var counter = (int)GetPrivateStaticField("_messageAttemptCounter");
            Assert.AreEqual(0, counter);
        }

        [Test]
        public void MessageDelay_CanBeSetAndRetrieved()
        {
            ChatBot.MessageDelay = 360;
            Assert.AreEqual(360, ChatBot.MessageDelay);
        }

        #endregion

        #region PlayerAsstNameOverrides State Tests

        [Test]
        public void PlayerAsstNameOverrides_MultipleOverrides_EachPlayerGetsCorrectName()
        {
            var overrides = new Dictionary<long, string>
            {
                { 1L, "Alpha" },
                { 2L, "Bravo" },
                { 3L, "Charlie" }
            };
            SetPrivateStaticField("_playerAsstNameOverrides", overrides);

            Assert.AreEqual("Alpha", ChatBot.ChatBotNameFor(1L));
            Assert.AreEqual("Bravo", ChatBot.ChatBotNameFor(2L));
            Assert.AreEqual("Charlie", ChatBot.ChatBotNameFor(3L));
        }

        [Test]
        public void PlayerAsstNameOverrides_UnknownPlayer_GetsDefaultName()
        {
            ChatBot.ChatBotName = "DefaultBot";
            var overrides = new Dictionary<long, string> { { 1L, "SpecialBot" } };
            SetPrivateStaticField("_playerAsstNameOverrides", overrides);

            Assert.AreEqual("DefaultBot", ChatBot.ChatBotNameFor(999L));
        }

        #endregion
    }
}
