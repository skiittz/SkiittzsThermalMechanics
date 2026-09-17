using System.Text;
using Sandbox.ModAPI;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Reactor
{
	public partial class ReactorLogic
	{
		void ReactorLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
		{
			var logic = arg1.GameLogic.GetAs<ReactorLogic>();
			if (logic == null || !logic.hasAuthoritativeState)
				return;
			logic.heatData.AppendCustomThermalInfo(logic.block, customInfo);
		}
	}
}
