using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class SkiittzThermalMechanicsSession : MySessionComponentBase
    {
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            Logger.Instance.LogDebug("Initializing session component");

            Configuration.Load();
        }
    }
}
