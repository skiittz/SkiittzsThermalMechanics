using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.H2Generator
{
	public partial class H2EngineLogic
	{
		public override void UpdateAfterSimulation100()
		{
			if (!MyAPIGateway.Multiplayer.IsServer && !MyAPIGateway.Utilities.IsDedicated) return;
			if (block == null || heatData == null || !block.IsOwnedByAPlayer()) return;

			heatData.ApplyHeating(block);
			block.RefreshCustomInfo();
		}
	}
}
