using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Game.ModAPI;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.HeatSink
{
    // Server-owned GPS markers. Each refresh replaces the previous cycle's markers.
    public static class ThermalSpikeBroadcaster
    {
        private const float MaximumBeaconRadius = 500000f;
        private static readonly HashSet<HeatSinkLogic> Sinks = new HashSet<HeatSinkLogic>();
        private static readonly Dictionary<long, List<int>> MarkerHashes = new Dictionary<long, List<int>>();
        private static readonly Random Random = new Random();
        private static long elapsedTicks;
        private static bool firstUpdate = true;

        public static void Register(HeatSinkLogic sink)
        {
            if (ThermalAuthority.IsServer && sink != null)
                Sinks.Add(sink);
        }

        public static void Unregister(HeatSinkLogic sink)
        {
            Sinks.Remove(sink);
        }

        public static void Update()
        {
            if (!ThermalAuthority.IsServer || MyAPIGateway.Session == null) return;

            float seconds;
            if (!Configuration.Configuration.TryGetGeneralSettingValue("ThermalSpikeCycleSeconds", out seconds)
                || seconds <= 0f)
                seconds = 900f;
            // Called every 100 simulation ticks.
            elapsedTicks += 100;
            if (firstUpdate && elapsedTicks < 200) return; // Let heat sinks calculate their initial signal radius.
            if (!firstUpdate && elapsedTicks < (double)seconds * 60) return;
            firstUpdate = false;
            elapsedTicks = 0;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, player => player != null && !player.IsBot);
            foreach (var entry in MarkerHashes)
                foreach (var hash in entry.Value)
                    MyAPIGateway.Session.GPS.RemoveGps(entry.Key, hash);
            MarkerHashes.Clear();

            foreach (var sink in Sinks)
            {
                var beacon = sink.Entity as IMyBeacon;
                if (beacon == null || beacon.Closed || !beacon.IsWorking || sink.HeatSinkData == null
                    || sink.HeatSinkData.SignalRadius < MaximumBeaconRadius) continue;

                var location = ApproximatePosition(beacon.GetPosition());
                foreach (var player in players)
                {
                    var gps = MyAPIGateway.Session.GPS.Create("Thermal Spike",
                        "Approximate location of a heat sink broadcasting at maximum range.", location, true);
                    MyAPIGateway.Session.GPS.AddGps(player.IdentityId, gps);
                    List<int> hashes;
                    if (!MarkerHashes.TryGetValue(player.IdentityId, out hashes))
                        MarkerHashes[player.IdentityId] = hashes = new List<int>();
                    hashes.Add(gps.Hash);
                }
            }
        }

        private static Vector3D ApproximatePosition(Vector3D center)
        {
            var azimuth = Random.NextDouble() * Math.PI * 2;
            var z = Random.NextDouble() * 2 - 1;
            var radius = MaximumBeaconRadius * Math.Pow(Random.NextDouble(), 1.0 / 3.0);
            var xy = Math.Sqrt(1 - z * z);
            return center + new Vector3D(radius * xy * Math.Cos(azimuth),
                radius * xy * Math.Sin(azimuth), radius * z);
        }

        public static void Reset()
        {
            if (ThermalAuthority.IsServer && MyAPIGateway.Session != null)
                foreach (var entry in MarkerHashes)
                    foreach (var hash in entry.Value)
                        MyAPIGateway.Session.GPS.RemoveGps(entry.Key, hash);
            MarkerHashes.Clear();
            Sinks.Clear();
            elapsedTicks = 0;
            firstUpdate = true;
        }
    }
}
