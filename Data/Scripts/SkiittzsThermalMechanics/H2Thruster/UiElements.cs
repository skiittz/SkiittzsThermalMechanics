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
			if (logic == null || !logic.hasAuthoritativeState)
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
			heatPercent.Getter = x =>
			{
				var logic = x.GameLogic.GetAs<HydrogenThrusterLogic>();
				return logic != null && logic.hasAuthoritativeState ? logic.heatData?.CurrentHeat ?? 0f : 0f;
			};
			MyAPIGateway.TerminalControls.AddControl<IMyThrust>(heatPercent);
		}
	}
}
