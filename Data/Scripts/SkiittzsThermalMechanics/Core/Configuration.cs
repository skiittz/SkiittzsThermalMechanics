namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core
{
	public partial class ThrusterHeatData
	{
		public static void LoadConfigFileValues(ref ThrusterHeatData data, string subTypeId, out bool configFound)
		{
			if (!Configuration.Configuration.BlockSettings.ContainsKey(subTypeId))
			{
				configFound = false;
				return;
			}

			configFound = true;
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
		public static void LoadConfigFileValues(ref PowerPlantHeatData data, string subTypeId, out bool configFound)
		{
			if (!Configuration.Configuration.BlockSettings.ContainsKey(subTypeId))
			{
				configFound = false;
				return;
			};

			configFound = true;
			float heatCapacity;
			float passiveCooling;
			float heatGenerationMultiplier;
			if (Configuration.Configuration.TryGetValue(subTypeId, "HeatCapacity", out heatCapacity))
				data.HeatCapacity = heatCapacity;
			if (Configuration.Configuration.TryGetValue(subTypeId, "PassiveCooling", out passiveCooling))
				data.PassiveCooling = passiveCooling;
			if(Configuration.Configuration.TryGetValue(subTypeId, "HeatGenerationMultiplier", out heatGenerationMultiplier))
				data.HeatGenerationMultiplier = heatGenerationMultiplier;
			else
				data.HeatGenerationMultiplier = 1.0f;
		}
	}
}
