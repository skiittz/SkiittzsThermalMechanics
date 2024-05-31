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
            if (messageText.StartsWith($"/{ChatBot.ChatBotName}", StringComparison.OrdinalIgnoreCase))
            {
                sendToOthers = false;
                var command = messageText.Replace($"/{ChatBot.ChatBotName} ", "");
                ChatBot.HandleCommand(command);
            }
        }
    }
}
