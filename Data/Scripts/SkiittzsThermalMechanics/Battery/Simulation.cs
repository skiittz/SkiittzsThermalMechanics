namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Battery
{
	public partial class BatteryLogic
	{
		public override void UpdateAfterSimulation100()
		{
			if (block == null || heatData == null || !block.IsOwnedByAPlayer())
				return;

			heatData.ApplyHeating(block);
			block.RefreshCustomInfo();
		}
	}
}
