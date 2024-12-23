namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core
{
	public partial class ThrusterHeatData
	{
		public static void LoadConfigFileValues(ref ThrusterHeatData data, string subTypeId)
		{
			if (!Configuration.Configuration.BlockSettings.ContainsKey(subTypeId)) return;
			float mwHeatPerNewtonThrust;
			float passiveCooling;
			if (Configuration.Configuration.TryGetValue(subTypeId, "MwHeatPerNewtonThrust", out mwHeatPerNewtonThrust))
				data.MwHeatPerNewtonThrust = mwHeatPerNewtonThrust;
			if (Configuration.Configuration.TryGetValue(subTypeId, "PassiveCooling", out passiveCooling))
				data.PassiveCooling = passiveCooling;
		}
	}
	public partial class PowerPlantHeatData
	{
		public static void LoadConfigFileValues(ref PowerPlantHeatData data, string subTypeId)
		{
			if (!Configuration.Configuration.BlockSettings.ContainsKey(subTypeId)) return;
			float heatCapacity;
			float passiveCooling;
			if (Configuration.Configuration.TryGetValue(subTypeId, "HeatCapacity", out heatCapacity))
				data.HeatCapacity = heatCapacity;
			if (Configuration.Configuration.TryGetValue(subTypeId, "PassiveCooling", out passiveCooling))
				data.PassiveCooling = passiveCooling;
		}
	}
}
