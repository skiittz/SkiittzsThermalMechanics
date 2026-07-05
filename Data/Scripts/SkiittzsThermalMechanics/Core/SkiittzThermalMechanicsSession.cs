using VRage.Game;
using VRage.Game.Components;
using Sandbox.ModAPI;
using System;
using VRage.Utils;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class SkiittzThermalMechanicsSession : MySessionComponentBase
    {
        private int _tickCounter = 0;
        public static bool IsSessionUnloading { get; private set; } = false;

        // Message IDs for config sync
        private const ushort ConfigRequestMessageId = 19531;
        private const ushort ConfigPayloadMessageId = 19532;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            IsSessionUnloading = false;

            // Register handlers for config sync
            try
            {
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(ConfigPayloadMessageId, OnConfigPayloadReceived_Secure);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(ConfigRequestMessageId, OnConfigRequestReceived_Secure);
            }
            catch (Exception)
            {
                // Swallow - registration may throw if multiplayer not available yet
            }

            // Determine if this instance is a dedicated server
            var isDedicated = false;
            try
            {
                isDedicated = MyAPIGateway.Utilities.IsDedicated;
            }
            catch (Exception)
            {
                // If the API isn't available for some reason, assume not dedicated
                isDedicated = false;
            }

            if (MyAPIGateway.Multiplayer.IsServer)
            {
                // Server loads the authoritative config from world storage
                Configuration.Configuration.Load();

                // Only broadcast the config when running as a dedicated server
                if (isDedicated)
                {
                    BroadcastConfigToClients();
                }
                else
                {
                    MyLog.Default.WriteLine("SkiittzsThermalMechanics: Running on non-dedicated server; using local configs.");
                }
            }
            else
            {
                // Client must request config from server and wait for payload before loading.
                // If the server is non-dedicated it will not broadcast, so the client will continue using local configs.
                RequestConfigFromServer();
            }
        }

        private void RequestConfigFromServer()
        {
            try
            {
                // Send an empty request packet to server asking for config
                MyAPIGateway.Multiplayer.SendMessageToServer(ConfigRequestMessageId, new byte[0]);
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"SkiittzsThermalMechanics: Failed to send config request to server: {e.Message}");
            }
        }

        private void BroadcastConfigToClients()
        {
            try
            {
                // Read the config file from world storage and broadcast its contents
                const string fileName = "Settings.xml";
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(SkiittzThermalMechanicsSession)))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(fileName, typeof(SkiittzThermalMechanicsSession));
                    var content = reader.ReadToEnd();
                    reader.Close();

                    var bytes = System.Text.Encoding.UTF8.GetBytes(content);
                    // Send payload to all clients
                    MyAPIGateway.Multiplayer.SendMessageToOthers(ConfigPayloadMessageId, bytes);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"SkiittzsThermalMechanics: Failed to broadcast config to clients: {e.Message}");
            }
        }

        // Replace obsolete OnConfigRequestReceived with secure version
        private void OnConfigRequestReceived_Secure(ushort id, byte[] data, ulong sender, bool fromServer)
        {
            BroadcastConfigToClients();
        }

        // Replace obsolete OnConfigPayloadReceived with secure version
        private void OnConfigPayloadReceived_Secure(ushort id, byte[] data, ulong sender, bool fromServer)
        {
            try
            {
                if (data == null || data.Length == 0) return;
                var content = System.Text.Encoding.UTF8.GetString(data);

                const string fileName = "Settings.xml";
                var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(fileName, typeof(SkiittzThermalMechanicsSession));
                writer.Write(content);
                writer.Flush();
                writer.Close();

                // Now load the configuration from the received file
                Configuration.Configuration.Load(forceReload: true);
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"SkiittzsThermalMechanics: Failed to process received config payload: {e.Message}");
            }
        }

        public override void UpdateAfterSimulation()
        {
            _tickCounter++;
            if (_tickCounter % 100 == 0)
            {
                Utilities.TickGridCaches();
            }
        }

        protected override void UnloadData()
        {
            IsSessionUnloading = true;

            // Unregister message handlers
            try
            {
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(ConfigPayloadMessageId, OnConfigPayloadReceived_Secure);
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(ConfigRequestMessageId, OnConfigRequestReceived_Secure);
            }
            catch (Exception)
            {
                // ignore
            }

            base.UnloadData();
        }
    }
}
