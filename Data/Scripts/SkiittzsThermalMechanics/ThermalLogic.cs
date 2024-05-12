using System;
using System.Text;

namespace SkiittzsThermalMechanics
{
    public static class ThermalLogic
    {
        public static readonly bool DebugMode = true;

        public static void AppendCustomThermalInfo(StringBuilder customInfo, float heatCapacity, float currentHeat, float lastHeatDelta)
        {
            customInfo.Append($"Heat Level: {(currentHeat / heatCapacity) * 100}%\n");
            var remainingSeconds = SecondsUntilOverheat(heatCapacity, currentHeat, lastHeatDelta);
            customInfo.Append($"Time until {(remainingSeconds < 0 ? "cooled" : "overheat")}: {TimeUntilOverheatDisplay(remainingSeconds)}\n");

            if (DebugMode)
            {
                customInfo.Append("DEBUG INFO:\n");
                customInfo.Append($"Current Heat: {currentHeat}\n");
                customInfo.Append($"Heat Capacity: {heatCapacity}\n");
                customInfo.Append($"Last Heat Delta: {lastHeatDelta}\n");
            }
        }

        private static float SecondsUntilOverheat(float heatCapacity, float currentHeat, float lastHeatDelta)
        {
            return ((heatCapacity - currentHeat) / lastHeatDelta) / 1.667f;
        }

        private static string TimeUntilOverheatDisplay(float remainingSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(remainingSeconds);

            return timeSpan.ToString("hh\\:mm\\:ss");
        }
    }
}
