using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public static class Utilities
    {
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
    }
}
