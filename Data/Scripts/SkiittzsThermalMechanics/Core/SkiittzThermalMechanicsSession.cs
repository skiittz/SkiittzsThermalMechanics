using VRage.Game;
using VRage.Game.Components;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.HeatSink;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Networking;

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
            Utilities.ResetSessionCaches();
            ThermalAuthority.Reset();
            ThermalNetwork.Initialize();

            if (ThermalAuthority.IsServer)
                Configuration.Configuration.Load();
            else
                Configuration.Configuration.InitializeClient();
        }

        public override void BeforeStart()
        {
            base.BeforeStart();
            ThermalNetwork.RequestFullSync();
        }

        public override void UpdateAfterSimulation()
        {
            ThermalNetwork.Update();
            _tickCounter++;
            if (_tickCounter % 100 == 0)
            {
                Utilities.TickGridCaches();
            }
        }

        public override void SaveData()
        {
            if (ThermalAuthority.IsServer)
                ThermalAuthority.SaveAll();
            base.SaveData();
        }

        protected override void UnloadData()
        {
            IsSessionUnloading = true;
            if (ThermalAuthority.IsServer)
                ThermalAuthority.SaveAll();

            HeatSinkLogic.ResetSessionState();
            Utilities.ResetSessionCaches();
            ThermalNetwork.Unload();
            ThermalAuthority.Reset();
            ChatBot.ChatBot.ResetSession();
            Configuration.Configuration.ResetSession();
            base.UnloadData();
        }
    }
}
