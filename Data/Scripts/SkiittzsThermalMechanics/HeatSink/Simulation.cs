using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Battery;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.H2Generator;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Reactor;
using System.Collections.Generic;
using System;
using System.Linq;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.ChatBot;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.HeatSink
{
	public partial class HeatSinkLogic
	{
		public float ActiveCooling(float heatValue)
		{
			var incomingHeat = heatValue.LowerBoundedBy(0);
			if (HeatSinkData.AvailableCapacity > incomingHeat)
			{
				HeatSinkData.CurrentHeat += incomingHeat;
				return incomingHeat;
			}
			else
			{
				var remainingHeat = incomingHeat - HeatSinkData.AvailableCapacity;
				HeatSinkData.CurrentHeat = HeatSinkData.HeatCapacity;
				return remainingHeat;
			}
		}

		public override void UpdateAfterSimulation100()
		{
			if (block == null || HeatSinkData == null || !block.IsOwnedByAPlayer()) return;
			CheckForSeparation();

			if (HeatSinkData.IsSmallGrid && HeatSinkData.ShuntToParent)
			{
				List<IMyShipConnector> connectors = new List<IMyShipConnector>();
				MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid)
					.GetBlocksOfType(connectors, x => 
						x.IsWorking && 
						x.CubeGrid == block.CubeGrid && 
						x.IsConnected && 
						x.OtherConnector.CubeGrid.GridSizeEnum == MyCubeSize.Large);

				foreach (var connector in connectors)
				{
					var parentHeatSink = Utilities.GetHeatSinkLogic(connector.OtherConnector.CubeGrid);
					if (parentHeatSink != null)
					{
						var sunkHeat = parentHeatSink.ActiveCooling(HeatSinkData.CurrentHeat);
						HeatSinkData.CurrentHeat -= sunkHeat;
					}
				}
			}

			HeatSinkData.VentingHeat *= 0.992f;

			HeatSinkData.CurrentHeat = (HeatSinkData.CurrentHeat - Math.Min(HeatSinkData.PassiveCooling, HeatSinkData.CurrentHeat)).LowerBoundedBy(0);
			HeatSinkData.SignalRadius = Math.Min(500000, HeatSinkData.VentingHeat * HeatSinkData.WeatherMult);
			block.Radius = HeatSinkData.SignalRadius;
			(block as IMyTerminalBlock).RefreshCustomInfo();

			if (HeatSinkData.HeatRatio >= 1)
				ChatBot.ChatBot.WarnPlayer(block, "Heat sink is at capacity!  Generators are overheating!", MessageSeverity.Warning);
			else if (HeatSinkData.HeatRatio >= 0.8)
				ChatBot.ChatBot.WarnPlayer(block, "Heat sink is at almost at capacity!  Need more radiators!", MessageSeverity.Tutorial);
			else if (HeatSinkData.HeatRatio >= 0.5)
				ChatBot.ChatBot.WarnPlayer(block, "Heat sink is at 50% capacity - do you have enough radiators?", MessageSeverity.Tutorial);
		}

		private void CheckForSeparation()
		{
			if (block == null || HeatSinkData == null || !block.IsOwnedByAPlayer() ||
				block?.CubeGrid?.EntityId == HeatSinkData.OriginalGridId) return;

			IMyEntity entity;
			if (MyAPIGateway.Entities.TryGetEntityById(HeatSinkData.OriginalGridId, out entity))
			{
				// Ensure the entity is a grid
				var originalGrid = entity as IMyCubeGrid;
				if (originalGrid != null)
				{
					ChatBot.ChatBot.WarnPlayer(originalGrid,
						"Heat sink has been disconnected from grid, generators are receiving feedback heat!",
						MessageSeverity.Warning);

					var powerProducersOnOriginalGrid = new List<IMyPowerProducer>();
					var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(originalGrid);
					gts.GetBlocksOfType(powerProducersOnOriginalGrid, x => x.IsWorking);

					foreach (var powerProducer in powerProducersOnOriginalGrid)
					{
						var heatData = powerProducer.GameLogic.GetAs<ReactorLogic>()?.heatData
									   ?? powerProducer.GameLogic.GetAs<BatteryLogic>()?.heatData
									   ?? powerProducer.GameLogic.GetAs<H2EngineLogic>()?.heatData;
						if (heatData != null)
						{
							HeatSinkData.CurrentHeat -= heatData.FeedHeatBack(HeatSinkData.CurrentHeat, true);
						}
					}
				}
			}

			HeatSinkData.OriginalGridId = block.CubeGrid.EntityId;
		}

		private void OnBlockDestroyed(object target, MyDamageInformation info)
		{
			var tgt = target as IMyEntity;
			if (tgt == null || tgt.EntityId != block.EntityId)
				return;

			if (HeatSinkData == null)
				return;

			var currentHeat = HeatSinkData.CurrentHeat;
			var ventingHeat = HeatSinkData.VentingHeat;
			var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid);

			var beacons = new List<IMyBeacon>();
			gts.GetBlocksOfType(beacons, x => x.IsWorking && x.BlockDefinition.SubtypeName.Contains("HeatSink") && x.EntityId != block.EntityId);
			foreach (var heatSink in beacons.OrderByDescending(x => x.Radius))
			{
				var gameLogic = heatSink.GameLogic.GetAs<HeatSinkLogic>();
				var sunkHeat = gameLogic.ActiveCooling(currentHeat);
				gameLogic.HeatSinkData.VentingHeat += ventingHeat;
				currentHeat -= sunkHeat;
			}

			var powerProducers = new List<IMyPowerProducer>();
			gts.GetBlocksOfType(powerProducers, x => x.IsWorking && x.IsSameConstructAs(block));
			foreach (var powerProducer in powerProducers)
			{
				PowerPlantHeatData heatData;
				if (powerProducer.BlockDefinition.SubtypeName.Contains("Battery"))
					heatData = powerProducer.GameLogic.GetAs<BatteryLogic>().heatData;
				else if (powerProducer.BlockDefinition.SubtypeName.Contains("Reactor"))
					heatData = powerProducer.GameLogic.GetAs<ReactorLogic>().heatData;
				else if (powerProducer.BlockDefinition.SubtypeName.Contains("Engine"))
					heatData = powerProducer.GameLogic.GetAs<H2EngineLogic>().heatData;
				else
					continue;

				var ventingHeatPortion = ventingHeat / powerProducers.Count;
				var sunkHeat = heatData.FeedHeatBack(currentHeat + ventingHeatPortion);
				currentHeat -= sunkHeat;
			}

		}

		public float RemoveHeat(float heat, float weatherMult)
		{
			var dissipatedHeat = (Math.Min(heat, HeatSinkData.CurrentHeat) * weatherMult).LowerBoundedBy(0);
			HeatSinkData.CurrentHeat -= dissipatedHeat;
			HeatSinkData.VentingHeat += dissipatedHeat;

			return dissipatedHeat;
		}
	}
}
