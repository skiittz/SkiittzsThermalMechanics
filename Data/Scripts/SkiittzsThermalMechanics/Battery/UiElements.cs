using System.Text;
using Sandbox.ModAPI;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Battery
{
	public partial class BatteryLogic
	{
		void BatteryLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
		{
			var logic = arg1.GameLogic.GetAs<BatteryLogic>();
			logic.heatData.AppendCustomThermalInfo(logic.block, customInfo);
		}
	}
}
