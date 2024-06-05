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
using Sandbox.ModAPI.Interfaces.Terminal;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public static class Utilities
    {
        public static readonly string CurrentHeatControlId = "CurrentHeat";
        public static readonly string HeatRatioControlId = "HeatRatio";

        public static HeatSinkLogic GetHeatSinkLogic(IMyCubeGrid grid)
        {
            var beacons = new List<IMyBeacon>();
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
            gts.GetBlocksOfType(beacons, x => x.IsWorking && x.BlockDefinition.SubtypeName.Contains("HeatSink"));
            beacons = beacons.OrderByDescending(x => x.Radius).ToList();

            if(!beacons.Any())
                ChatBot.WarnPlayer(grid, "It seems you have no heatsinks on this grid - Power generators will take damage if they get too hot, I recommend building a heat sink to protect them!");

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
            //Logger.Instance.LogDebug("IsPlayerOwned", block as IMyTerminalBlock);
            var ownerId = block.OwnerId;
            var faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(ownerId);
            return faction == null || !faction.IsEveryoneNpc();
        }

        public static float LowerBoundedBy(this float input, float bound)
        {
            return Math.Max(input, bound);
        }

        public static float UpperBoundedBy(this float input, float bound)
        {
            return Math.Min(input, bound);
        }
    }
}
