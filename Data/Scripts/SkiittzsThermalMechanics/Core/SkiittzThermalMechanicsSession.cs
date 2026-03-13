using VRage.Game;
using VRage.Game.Components;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class SkiittzThermalMechanicsSession : MySessionComponentBase
    {
        private int _tickCounter = 0;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            Configuration.Configuration.Load();
        }

        public override void UpdateAfterSimulation()
        {
            _tickCounter++;
            if (_tickCounter % 100 == 0)
            {
                Utilities.TickGridCaches();
            }
        }
    }
}
