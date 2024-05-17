using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common.ObjectBuilders;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.Components;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_UpgradeModule), false, new []
    {
        "LargeHeatRadiatorBlock", "SmallHeatRadiatorBlock"
    })]
    public class HeatRadiatorLogic : MyGameLogicComponent
    {
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
        }
    }
}
