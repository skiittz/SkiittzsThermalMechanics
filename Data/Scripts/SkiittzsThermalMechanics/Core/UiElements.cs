using System;
using System.Text;
using Sandbox.ModAPI;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core
{
    public partial class ThrusterHeatData
    {
        public void AppendCustomThermalInfo(IMyThrust block, StringBuilder customInfo)
        {
            customInfo.Append($"Current Heat Level: {CurrentHeat}\n");
        }
    }
    public partial class PowerPlantHeatData
    {
	    private const float SecondsPer100Ticks = 1.667f;

		public void AppendCustomThermalInfo(IMyPowerProducer block, StringBuilder customInfo)
        {
            var remainingSeconds = RemainingSeconds();

            customInfo.Append($"Heat Level: {(CurrentHeat / HeatCapacity) * 100}%\n");
            customInfo.Append($"Time until {(LastHeatDelta <= 0 ? "cooled" : "overheat")}: {TimeUntilOverheatDisplay(Math.Abs(remainingSeconds))}\n");

            customInfo.Append($"Heat Generation: {ThermalFatigue * 100:F0}%\n");
        }
        private float RemainingSeconds()
        {
            var numerator = LastHeatDelta > 0 ? HeatCapacity - CurrentHeat : CurrentHeat;
            var denominator = LastHeatDelta == 0 ? 1 : Math.Abs(LastHeatDelta);
            return (numerator / denominator) * SecondsPer100Ticks;
        }
        private static string TimeUntilOverheatDisplay(float remainingSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(remainingSeconds);
            return timeSpan.ToString("hh\\:mm\\:ss");
        }
    }
}
