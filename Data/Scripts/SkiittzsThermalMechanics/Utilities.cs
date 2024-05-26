using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame;
using VRage.Scripting;
using IMyCubeBlock = VRage.Game.ModAPI.IMyCubeBlock;
using IMyCubeGrid = VRage.Game.ModAPI.IMyCubeGrid;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public static class Utilities
    {
        public static readonly string HeatRatioControlId = "HeatRatio";
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
    }
}
