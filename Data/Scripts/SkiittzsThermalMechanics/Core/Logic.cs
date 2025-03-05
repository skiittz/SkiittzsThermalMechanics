using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.ChatBot;
using VRage.Utils;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core
{
	public partial class ThrusterHeatData
	{
		public void ApplyHeating(IMyThrust block)
		{
			LastHeatDelta = CalculateHeating(block);
			CurrentHeat += LastHeatDelta;
			CurrentHeat -= CalculateCooling(block, CurrentHeat);
			if (CurrentHeat < 0)
				CurrentHeat = 0;
		}

		private float CalculateCooling(IMyThrust block, float availableHeatToSink)
		{
			return Utilities.GetHeatSinkLogic(block?.CubeGrid)?.ActiveCooling(availableHeatToSink) ?? 0;
		}

		private float CalculateHeating(IMyThrust block)
		{
			if (!block.IsWorking || !block.IsOwnedByAPlayer())
				return 0;
			return (block.CurrentThrust * MwHeatPerNewtonThrust) - PassiveCooling;
		}
	}
	public partial class PowerPlantHeatData
	{
		private float CalculateCooling(IMyPowerProducer block, float availableHeatToSink)
		{
			return Utilities.GetHeatSinkLogic(block?.CubeGrid)?.ActiveCooling(availableHeatToSink) ?? 0;
		}

		public float FeedHeatBack(float incomingHeat, bool allowOverheat = false)
		{
			if (incomingHeat < 0)
				return 0;

			var acceptedHeat = Math.Min(incomingHeat, (AvailableHeatCapacity * (allowOverheat ? 1.5f : 1)));
			CurrentHeat += acceptedHeat;
			return acceptedHeat;
		}

		private float CalculateHeating(IMyPowerProducer block)
		{
			if (!block.IsWorking || !block.IsOwnedByAPlayer())
				return 0;

			var powerProducers = new List<IMyPowerProducer>();
			var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid);
			gts.GetBlocksOfType(powerProducers, x => x.IsWorking && x.IsSameConstructAs(block));

			var additionalGeneratorCount = Math.Min(powerProducers.Count - 1, 0);
			var spamPenalty = 1 + (additionalGeneratorCount / 100);
			if (spamPenalty > 1.1)
				ChatBot.ChatBot.WarnPlayer(block, $"Wow that's a lot of power plants!  Did you know that spamming generators will incur a penalty?  You are currently generating {additionalGeneratorCount}% more  heat than you'd otherwise be.  It's better to use fewer, more powerful power plants.", MessageSeverity.Tutorial);
			return (block.CurrentOutput * spamPenalty) - PassiveCooling;
		}

		public void ApplyHeating(IMyPowerProducer block)
		{
			if (block == null || !block.IsOwnedByAPlayer()) return;
			var heatGenerated = CalculateHeating(block);
			var heatDissipated = CalculateCooling(block, CurrentHeat + heatGenerated);
			LastHeatDelta = heatGenerated - heatDissipated;
			CurrentHeat += LastHeatDelta;
			if (CurrentHeat < 0)
				CurrentHeat = 0;

			if (CurrentHeat >= HeatCapacity)
			{
				ChatBot.ChatBot.WarnPlayer(block, $"Taking damage due to overheating!", MessageSeverity.Warning);
				OverHeatCycles++;
				if (OverHeatCycles > 10)
					ChatBot.ChatBot.WarnPlayer(block, $"Due to repeated overheating, this block is suffering from thermal fatigue.  It will generate more heat than usual.  This effect is permanent.", MessageSeverity.Tutorial);
				var thermalFatigue = CurrentHeat + (CurrentHeat * ThermalFatigue);
				block.SlimBlock.DoDamage((thermalFatigue - HeatCapacity), MyStringHash.GetOrCompute("Overheating"), true);
				if (block.IsFunctional)
					CurrentHeat = HeatCapacity;
				else
				{
					ChatBot.ChatBot.WarnPlayer(block, "Disabled due to heat damage.", MessageSeverity.Warning);
					CurrentHeat = 0;
				}
			}
			else if (CurrentHeat > HeatCapacity * .8)
			{
				block.SetDamageEffect(true);
				ChatBot.ChatBot.WarnPlayer(block, $"Approaching heat threshold.", MessageSeverity.Tutorial);
			}
			else
				block.SetDamageEffect(false);
		}
	}
}
