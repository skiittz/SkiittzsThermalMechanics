using System.Text;
using Sandbox.ModAPI;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.H2Generator
{
	public partial class H2EngineLogic
	{
		void H2EngineLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
		{
			var logic = arg1.GameLogic.GetAs<H2EngineLogic>();
			logic.heatData.AppendCustomThermalInfo(logic.block, customInfo);
		}
	}
}
