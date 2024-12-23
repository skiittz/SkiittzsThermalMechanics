using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Battery;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.H2Generator;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.HeatSink;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Reactor;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public static class ScriptHookCreator
    {
        private static bool beaconsInitialized = false;
        private static bool reactorsInitialized = false;
        private static bool batteriesInitialized = false;
        private static bool h2EnginesInitialized = false;

        public static void AddBeaconHeatRatioControl()
        {
            if (beaconsInitialized) return;
            beaconsInitialized = true;
            var heatPercent = MyAPIGateway.TerminalControls.CreateProperty<float, IMyBeacon>(Utilities.HeatRatioControlId);
            heatPercent.Getter = GetHeatRatio;
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(heatPercent);
        }

        public static void AddReactorHeatRatioControl()
        {
            if (reactorsInitialized) return;
            reactorsInitialized = true;
            var heatPercent = MyAPIGateway.TerminalControls.CreateProperty<float, IMyReactor>(Utilities.HeatRatioControlId);
            heatPercent.Getter = GetHeatRatio;
            MyAPIGateway.TerminalControls.AddControl<IMyReactor>(heatPercent);
        }

        public static void AddBatteryHeatRatioControl()
        {
            if(batteriesInitialized) return;
            batteriesInitialized = true;
            var heatPercent = MyAPIGateway.TerminalControls.CreateProperty<float, IMyBatteryBlock>(Utilities.HeatRatioControlId);
            heatPercent.Getter = GetHeatRatio;
            MyAPIGateway.TerminalControls.AddControl<IMyBatteryBlock>(heatPercent);
        }

        public static void AddH2HeatRatioControl()
        {
            if (h2EnginesInitialized) return;
            h2EnginesInitialized = true;
            var heatPercent = MyAPIGateway.TerminalControls.CreateProperty<float, IMyTerminalBlock>(Utilities.HeatRatioControlId);
            heatPercent.Getter = GetHeatRatio;
            MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(heatPercent);
        }

        private static float GetHeatRatio(IMyTerminalBlock block)
        {
            var heatSinkLogic = block.GameLogic.GetAs<HeatSinkLogic>();
            if (heatSinkLogic != null) return heatSinkLogic.HeatSinkData.HeatRatio;

            var reactorLogic = block.GameLogic.GetAs<ReactorLogic>();
            if (reactorLogic != null) return reactorLogic.heatData.HeatRatio;

            var batteryLogic = block.GameLogic.GetAs<BatteryLogic>();
            if (batteryLogic != null) return batteryLogic.heatData.HeatRatio;

            var h2EngineLogic = block.GameLogic.GetAs<H2EngineLogic>();
            if (h2EngineLogic != null) return h2EngineLogic.heatData.HeatRatio;

            return 0f;
        }
    }
}
