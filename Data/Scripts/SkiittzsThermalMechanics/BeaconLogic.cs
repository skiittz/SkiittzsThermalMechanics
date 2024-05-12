using System;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon),false)]
    public class BeaconLogic : MyGameLogicComponent
    {
        private IMyBeacon block;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyBeacon)Entity;
        }

        public float ActiveCooling()
        {
            var powerBlocks = new List<IMyPowerProducer>();
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid);
            gts.GetBlocksOfType(powerBlocks, x =>
                x.IsWorking &&
                (x.BlockDefinition.ToString().Contains("HydrogenEngine")
                || x.BlockDefinition.ToString().Contains("Reactor") 
                || (x.BlockDefinition.ToString().Contains("Battery") )
                    )
                );

            var numberOfHeatSources = powerBlocks.Count;
            
            return (block.Radius / 100000) / numberOfHeatSources;
        }
    }
}
