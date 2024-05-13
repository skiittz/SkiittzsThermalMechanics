using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public static class Utilities
    {
        public static BeaconLogic GetBeaconLogic(IMyCubeGrid grid)
        {
            var beacons = new List<IMyBeacon>();
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
            gts.GetBlocksOfType(beacons, x => x.IsWorking);

            var beacon = beacons.OrderByDescending(x => x.Radius).FirstOrDefault();
            if (beacon == null)
                return null;

            return beacon.GameLogic.GetAs<BeaconLogic>();
        }
    }
}
