namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Reactor
{
	public partial class ReactorLogic
	{
		public override void UpdateAfterSimulation100()
		{
			if (block == null || heatData == null || !block.IsOwnedByAPlayer()) return;

			heatData.ApplyHeating(block);
			block.RefreshCustomInfo();
		}
	}
}
