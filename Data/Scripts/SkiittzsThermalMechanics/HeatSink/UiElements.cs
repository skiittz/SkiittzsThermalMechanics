using System.Text;
using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.HeatSink
{
	public partial class HeatSinkLogic
	{
		void HeatSinkLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
		{
			var logic = arg1.GameLogic.GetAs<HeatSinkLogic>();
			var heatLevel = ((logic.HeatSinkData.CurrentHeat / logic.HeatSinkData.HeatCapacity) * 100).LowerBoundedBy(0);
			customInfo.Append($"Heat Level: {heatLevel:N0}%{Utilities.DebugString($" [{logic.HeatSinkData.CurrentHeat}]")}\n");
			customInfo.Append($"Current IR Detectable Distance: {logic.block.Radius:N0} meters \n");
		}
	}
}
