using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Radiator;

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
            customInfo.Append($"Est. Radiators Needed: {EstimateRadiatorsNeeded(block)}\n");
        }
        private float RemainingSeconds()
        {
            var numerator = LastHeatDelta > 0 ? HeatCapacity - CurrentHeat : CurrentHeat;
            var denominator = LastHeatDelta == 0 ? 1 : Math.Abs(LastHeatDelta);
            return (numerator / denominator) * SecondsPer100Ticks;
        }
		private static string TimeUntilOverheatDisplay(float remainingSeconds)
		{
			double convertedSeconds;
			TimeSpan timeSpan;

			if (double.TryParse(remainingSeconds.ToString(), out convertedSeconds))
				timeSpan = TimeSpan.FromSeconds(convertedSeconds);
			else timeSpan = TimeSpan.FromSeconds(0);

			return timeSpan.ToString("hh\\:mm\\:ss");
		}

		private string EstimateRadiatorsNeeded(IMyPowerProducer block)
		{
			var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid);

			var totalHeatGeneration = GetTotalHeatGeneration(gts, block);
			float totalDissipation;
			float avgMaxDissipation;
			GetRadiatorDissipation(gts, out totalDissipation, out avgMaxDissipation);

			if (avgMaxDissipation <= 0)
				return "N/A (no radiators found)";

			var deficit = totalHeatGeneration - totalDissipation;
			if (deficit <= 0)
				return "0";

			var additionalNeeded = (int)Math.Ceiling(deficit / avgMaxDissipation);
			return additionalNeeded.ToString();
		}

		private static float GetTotalHeatGeneration(IMyGridTerminalSystem gts, IMyPowerProducer referenceBlock)
		{
			var producers = new List<IMyPowerProducer>();
			gts.GetBlocksOfType(producers, x => x.IsWorking && x.IsSameConstructAs(referenceBlock));

			var additionalGeneratorCount = Math.Max(producers.Count - 1, 0);
			var spamPenalty = 1 + (additionalGeneratorCount / 100f);

			float totalHeat = 0;
			foreach (var producer in producers)
				totalHeat += producer.CurrentOutput * spamPenalty;

			return totalHeat;
		}

		private static void GetRadiatorDissipation(IMyGridTerminalSystem gts, out float totalDissipation, out float avgMaxDissipation)
		{
			totalDissipation = 0;
			avgMaxDissipation = 0;

			var radiators = new List<IMyUpgradeModule>();
			gts.GetBlocksOfType(radiators);

			var radiatorCount = 0;
			foreach (var rad in radiators)
			{
				var logic = rad.GameLogic.GetAs<HeatRadiatorLogic>();
				if (logic?.radiatorData == null)
					continue;

				totalDissipation += logic.radiatorData.MaxDissipation;
				radiatorCount++;
			}

			if (radiatorCount > 0)
				avgMaxDissipation = totalDissipation / radiatorCount;
		}
	}
}
