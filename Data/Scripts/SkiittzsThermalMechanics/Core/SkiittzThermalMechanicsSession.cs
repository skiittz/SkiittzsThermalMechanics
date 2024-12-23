using VRage.Game;
using VRage.Game.Components;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class SkiittzThermalMechanicsSession : MySessionComponentBase
    {
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            Configuration.Configuration.Load();
        }
    }
}
