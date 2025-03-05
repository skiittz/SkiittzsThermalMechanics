using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.ChatBot;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.HeatSink;
using IMyCubeBlock = VRage.Game.ModAPI.IMyCubeBlock;
using IMyCubeGrid = VRage.Game.ModAPI.IMyCubeGrid;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core
{
    public static class Utilities
    {
        public static readonly string CurrentHeatControlId = "CurrentHeat";
        public static readonly string HeatRatioControlId = "HeatRatio";

        public static HeatSinkLogic GetHeatSinkLogic(IMyCubeGrid grid)
        {
            if(grid == null)
				return null;

			var beacons = new List<IMyBeacon>();
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
            gts.GetBlocksOfType(beacons, x => x.IsWorking && x.BlockDefinition.SubtypeName.Contains("HeatSink") && x.CubeGrid.EntityId == grid.EntityId);
            beacons = beacons.OrderByDescending(x => x.Radius).ToList();

            if(!beacons.Any())
	            ChatBot.ChatBot.WarnPlayer(grid, "It seems you have no active heatsinks on this grid - Power generators will take damage if they get too hot, I recommend building a heat sink to protect them!", MessageSeverity.Tutorial);

            if (beacons.Count > 1)
                for (int i = 1; i < beacons.Count; i++)
                    beacons[i].Enabled = false;
            
            var beacon = beacons.FirstOrDefault();
            if (beacon == null)
                return null;

            beacon.Enabled = true;
            return beacon.GameLogic.GetAs<HeatSinkLogic>();
        }

        public static bool IsOwnedByAPlayer(this IMyCubeBlock block)
        {
            if (block == null || MyAPIGateway.Session == null) return false;

            var ownerId = block.OwnerId;
            var currentPlayerId = Utilities.TryGetCurrentPlayerId();
            if (ownerId == currentPlayerId) return true;

            var faction = MyAPIGateway.Session?.Factions?.TryGetPlayerFaction(ownerId);
            return faction == null || !faction.IsEveryoneNpc();
        }

        public static bool IsOwnedByCurrentPlayer(this IMyCubeBlock block)
        {
            if (block == null || MyAPIGateway.Session == null) return false;

            var ownerId = block.OwnerId;
            var currentPlayerId = Utilities.TryGetCurrentPlayerId();
            if (ownerId == currentPlayerId) return true;

            return false;
        }

        public static float LowerBoundedBy(this float input, float bound)
        {
            return Math.Max(input, bound);
        }

        public static float UpperBoundedBy(this float input, float bound)
        {
            return Math.Min(input, bound);
        }

        public static long TryGetCurrentPlayerId()
        {
            return MyAPIGateway.Session?.Player?.IdentityId ?? 0;
        }

        public static string DebugString(string message)
        {
            return Configuration.Configuration.DebugMode ? message : string.Empty;
        }
    }
}
