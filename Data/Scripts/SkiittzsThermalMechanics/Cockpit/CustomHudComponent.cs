using Sandbox.ModAPI;
using VRage.Game.Components;
using System.Text;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Game.ModAPI;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Cockpit
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	public class HeatSinkHUDSession : MySessionComponentBase
	{
		private bool isInCockpit = false;
		private IMyCubeGrid currentGrid = null;
		private int updateCounter = 0;
		private const int UPDATE_FREQUENCY = 125; // Update every 60 ticks (1 second)

		public override void UpdateBeforeSimulation()
		{
			if (MyAPIGateway.Utilities?.IsDedicated == true)
				return;

			updateCounter++;
			if (updateCounter % UPDATE_FREQUENCY != 0)
				return;

			CheckCockpitStatus();

			if (isInCockpit && currentGrid != null)
			{
				DisplayHeatHUD();
			}
		}

		private void CheckCockpitStatus()
		{
			var controlledEntity = MyAPIGateway.Session?.ControlledObject;
			var cockpit = controlledEntity?.Entity as IMyCockpit;

			if (cockpit != null)
			{
				if (!isInCockpit || currentGrid != cockpit.CubeGrid)
				{
					isInCockpit = true;
					currentGrid = cockpit.CubeGrid;
					MyAPIGateway.Utilities.ShowMessage("HeatSink HUD", "Heat monitoring enabled");
				}
			}
			else
			{
				if (isInCockpit)
				{
					isInCockpit = false;
					currentGrid = null;
				}
			}
		}

		private void DisplayHeatHUD()
		{
			if (Configuration.Configuration.PlayerHudIsDisabled(Utilities.TryGetCurrentPlayerId()))
				return;

			var hudText = BuildHeatHUDText();
			MyAPIGateway.Utilities.ShowNotification(hudText);
		}

		private string BuildHeatHUDText()
		{
			var hudBuilder = new StringBuilder();
			var heatSink = Utilities.GetHeatSinkLogic(currentGrid);
			if (heatSink?.HeatSinkData == null)
				return string.Empty;

			float percentage = heatSink.HeatSinkData.HeatRatio * 100f;
			hudBuilder.Append($"Heat Sink Capacity: {CreateHeatBar(percentage)} {percentage:F0}%");
			hudBuilder.Append($" Signal Range: {(int)heatSink.HeatSinkData.SignalRadius/1000}km");
			if (Configuration.Configuration.DebugMode)
				hudBuilder.Append($"\n{heatSink.HeatSinkData.CurrentHeat:F0}/{heatSink.HeatSinkData.HeatCapacity:F0} Heat Units");


			return hudBuilder.ToString();
		}

		private string CreateHeatBar(float percentage)
		{
			int barLength = 10;
			int filledBars = (int)((percentage / 100f) * barLength);

			var bar = new StringBuilder("[");

			for (int i = 0; i < barLength; i++)
			{
				if (i <= filledBars)
					bar.Append("|");
				else
					bar.Append(" ");

			}

			bar.Append("]");
			return bar.ToString();
		}
	}
}
