using System;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VRage;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame;
using VRage.Scripting;
using IMyCubeBlock = VRage.Game.ModAPI.IMyCubeBlock;
using IMyCubeGrid = VRage.Game.ModAPI.IMyCubeGrid;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public static class Utilities
    {
        public static readonly string CurrentHeatControlId = "CurrentHeat";

        public static string SaveDataFilePath = $"{MyAPIGateway.Utilities.GamePaths.SavesPath}\\SkiittzThermalMechanics";

        public static HeatSinkLogic GetHeatSinkLogic(IMyCubeGrid grid)
        {
            var beacons = new List<IMyBeacon>();
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
            gts.GetBlocksOfType(beacons, x => x.IsWorking && x.BlockDefinition.SubtypeName.Contains("HeatSink"));
            beacons = beacons.OrderByDescending(x => x.Radius).ToList();


            if (beacons.Count > 1)
                for (int i = 1; i < beacons.Count; i++)
                    beacons[i].Enabled = false;

            var beacon = beacons.FirstOrDefault();
            if (beacon == null)
                return null;

            beacon.Enabled = true;
            return beacon.GameLogic.GetAs<HeatSinkLogic>();
        }

        public static bool IsPlayerOwned(this IMyCubeBlock block)
        {
            var ownerId = block.OwnerId;
            var faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(ownerId);
            return faction == null || !faction.IsEveryoneNpc();
        }

        public static bool IsPlayerOwnedGrid(this IMyCubeGrid grid)
        {
            if (grid != null)
            {
                foreach (var block in grid.GetFatBlocks<IMyCubeBlock>())
                {
                    var ownerId = block.OwnerId;
                    if (ownerId == 0 || MyAPIGateway.Players.TryGetSteamId(block.OwnerId) == 0)
                        return false;

                    var faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(ownerId);
                    if (faction != null && faction.IsEveryoneNpc())
                        return false;
                }
            }

            return true;
        }

        public static void AddHeatDataToCustomData(this IMyTerminalBlock block, float heatRatio)
        {
            Dictionary<string, string> customData = new Dictionary<string, string>();
            var existingData = block.CustomData
                .Split('\n');

            foreach (var item in existingData)
            {
                if (item.Contains(":"))
                {
                    var components = item.Split(':');
                    customData.Add(components[0], components[1]);
                }
                else
                    customData.Add(item, string.Empty);
            }
            customData["HeatRatio"] = heatRatio.ToString();

            var stringBuilder = new StringBuilder();
            foreach (var item in customData)
            {
                if (string.IsNullOrEmpty(item.Key))
                    continue;
                if (string.IsNullOrEmpty(item.Value))
                    stringBuilder.Append($"{item.Key}\n");
                else
                    stringBuilder.Append($"{item.Key}:{item.Value}\n");
            }

            block.CustomData = stringBuilder.ToString();
        }
        //private void test()
        //{
        //    var powerProducers = new List<IMyPowerProducer>();
        //    var beacons = new List<IMyBeacon>();
        //    IMyGridTerminalSystem GridTerminalSystem;
        //    GridTerminalSystem.GetBlocksOfType(powerProducers);
        //    GridTerminalSystem.GetBlocksOfType(beacons);

        //    var blocks = powerProducers.Select(x => x as IMyTerminalBlock)
        //        .Union(beacons.Select(x => x as IMyTerminalBlock));
        //    foreach (var block in blocks)
        //    {
        //        var customData = block.CustomData;
        //        var lines = customData.Split('\n');
        //        foreach (var line in lines)
        //        {
        //            if (line.Contains(":"))
        //            {
        //                var items = line.Split(':');
        //                if (items[0] == "HeatRatio")
        //                    Echo($"{block.CustomName}-HeatRatio:{items[1]}");
        //            }
        //        }
        //    }
        //}
    }
}
