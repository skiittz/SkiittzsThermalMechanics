using System.Text;
using Sandbox.ModAPI;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Battery
{
	public partial class BatteryLogic
	{
		void BatteryLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
		{
			var logic = arg1.GameLogic.GetAs<BatteryLogic>();
			if (logic == null || !logic.hasAuthoritativeState)
				return;
			logic.heatData.AppendCustomThermalInfo(logic.block, customInfo);
		}
	}
}
