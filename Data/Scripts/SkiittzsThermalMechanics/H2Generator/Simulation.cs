using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.H2Generator
{
	public partial class H2EngineLogic
	{
		public override void UpdateAfterSimulation100()
		{
			if (!ThermalAuthority.IsServer || block == null || heatData == null || block.CubeGrid?.Physics == null) return;

			heatData.ApplyHeating(block);
			ThermalAuthority.Sync(this);
			block.RefreshCustomInfo();
		}
	}
}
