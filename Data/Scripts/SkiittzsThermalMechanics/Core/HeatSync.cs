using Sandbox.ModAPI;
using System.IO;
using VRage.Game.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Reactor;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Battery;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.H2Generator;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.H2Thruster;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core
{
    public static class HeatSync
    {
        private const ushort MessageId = 50000;

        public static void Init()
        {
            try
            {
                MyAPIGateway.Multiplayer.RegisterMessageHandler(MessageId, HandleMessage);
            }
            catch { }
        }

        public static void Unload()
        {
            try
            {
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(MessageId, HandleMessage);
            }
            catch { }
        }

        public static void SendHeatUpdate(long entityId, float currentHeat)
        {
            // Only the authoritative server (including host) should send updates
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            try
            {
                using (var ms = new MemoryStream())
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(entityId);
                    writer.Write(currentHeat);
                    writer.Flush();
                    var bytes = ms.ToArray();
                    MyAPIGateway.Multiplayer.SendMessageToOthers(MessageId, bytes);
                }
            }
            catch { }
        }

        private static void HandleMessage(byte[] bytes)
        {
            // Dedicated server does not need to process client messages
            if (MyAPIGateway.Multiplayer.IsServer && MyAPIGateway.Utilities.IsDedicated)
                return;

            try
            {
                using (var ms = new MemoryStream(bytes))
                using (var reader = new BinaryReader(ms))
                {
                    var entityId = reader.ReadInt64();
                    var currentHeat = reader.ReadSingle();

                    IMyEntity ent;
                    if (!MyAPIGateway.Entities.TryGetEntityById(entityId, out ent))
                        return;

                    var block = ent as IMyCubeBlock;
                    if (block == null) return;

                    // Try known logic types and update their heat data
                    var rLogic = block.GameLogic.GetAs<ReactorLogic>();
                    if (rLogic?.heatData != null)
                    {
                        rLogic.heatData.CurrentHeat = currentHeat;
                        block.RefreshCustomInfo();
                        return;
                    }

                    var bLogic = block.GameLogic.GetAs<Battery.BatteryLogic>();
                    if (bLogic?.heatData != null)
                    {
                        bLogic.heatData.CurrentHeat = currentHeat;
                        block.RefreshCustomInfo();
                        return;
                    }

                    var h2Logic = block.GameLogic.GetAs<H2EngineLogic>();
                    if (h2Logic?.heatData != null)
                    {
                        h2Logic.heatData.CurrentHeat = currentHeat;
                        block.RefreshCustomInfo();
                        return;
                    }

                    var thrusterLogic = block.GameLogic.GetAs<HydrogenThrusterLogic>();
                    if (thrusterLogic?.heatData != null)
                    {
                        thrusterLogic.heatData.CurrentHeat = currentHeat;
                        block.RefreshCustomInfo();
                        return;
                    }
                }
            }
            catch { }
        }
    }
}
