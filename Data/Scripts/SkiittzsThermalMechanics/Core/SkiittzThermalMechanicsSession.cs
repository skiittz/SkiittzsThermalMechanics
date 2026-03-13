using VRage.Game;
using VRage.Game.Components;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class SkiittzThermalMechanicsSession : MySessionComponentBase
    {
        private int _tickCounter = 0;
        public static bool IsSessionUnloading { get; private set; } = false;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            IsSessionUnloading = false;
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

        protected override void UnloadData()
        {
            IsSessionUnloading = true;
            base.UnloadData();
        }
    }
}
