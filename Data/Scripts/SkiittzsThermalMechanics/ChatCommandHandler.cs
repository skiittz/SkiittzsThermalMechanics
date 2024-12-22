using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using VRage.Game;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
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
                var items = messageText.Replace($"/{chatBotName} ", "").Split(' ');

				var command = items[0];
				var args = items.Skip(1);
                ChatBot.HandleCommand(command, args.Any() ? args.ToArray() : null); 
            }
        }
    }
}
