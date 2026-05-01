using System;
using System.Linq;
using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Game;
using VRage.Game.Components;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.ChatBot
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class ChatCommandHandler : MySessionComponentBase
    {
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
            base.UnloadData();
        }

        private void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            var chatBotName = ChatBot.ChatBotNameFor(Utilities.TryGetCurrentPlayerId());
            if (messageText.StartsWith($"/{chatBotName}", StringComparison.OrdinalIgnoreCase))
            {
                sendToOthers = false;

                // Calculate the index right after the '/{chatBotName}' portion so we can extract the command part
                var afterBotIndex = 1 + chatBotName.Length; // skip '/' and bot name
                if (messageText.Length > afterBotIndex && messageText[afterBotIndex] == ' ')
                    afterBotIndex++; // skip a single space if present

                var payload = messageText.Length > afterBotIndex ? messageText.Substring(afterBotIndex) : string.Empty;
                payload = payload.Trim();

                // If no payload present, mimic previous behavior which would result in an empty command string
                var items = string.IsNullOrEmpty(payload)
                    ? new string[] { string.Empty }
                    : payload.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Normalize command to lower-case so command matching is case-insensitive
                var command = items[0].ToLowerInvariant();
                var args = items.Skip(1);
                ChatBot.HandleCommand(command, args.Any() ? args.ToArray() : null);
            }
        }
    }
}
