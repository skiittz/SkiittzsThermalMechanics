using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.H2Thruster
{
	public partial class HydrogenThrusterLogic
	{
		void ThrusterLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
		{
			var logic = arg1.GameLogic.GetAs<HydrogenThrusterLogic>();
			if (logic == null)
				return;
			logic.heatData.AppendCustomThermalInfo(logic.block, customInfo);
		}

		public void AddCurrentHeatControl()
		{
			var existingControls = new List<IMyTerminalControl>();
			MyAPIGateway.TerminalControls.GetControls<IMyThrust>(out existingControls);
			if (existingControls.Any(x => x.Id == Utilities.CurrentHeatControlId))
				return;

			var heatPercent =
				MyAPIGateway.TerminalControls.CreateProperty<float, IMyThrust>(Utilities.CurrentHeatControlId);
			heatPercent.Getter = x => heatData.CurrentHeat;
			MyAPIGateway.TerminalControls.AddControl<IMyThrust>(heatPercent);
		}
	}
}
