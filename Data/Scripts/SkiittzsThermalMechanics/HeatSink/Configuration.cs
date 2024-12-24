using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Configuration;
namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.HeatSink
{
	public partial class HeatSinkData
	{
		public static void LoadConfigFileValues(ref HeatSinkData data, string subTypeId, out bool configFound)
		{
			if (!Configuration.Configuration.BlockSettings.ContainsKey(subTypeId))
			{
				configFound = false;
				return;
			}

			configFound = true;
			float heatCapacity;
			float passiveCooling;
			if (Configuration.Configuration.TryGetValue(subTypeId, "HeatCapacity", out heatCapacity))
				data.HeatCapacity = heatCapacity;
			if (Configuration.Configuration.TryGetValue(subTypeId, "PassiveCooling", out passiveCooling))
				data.PassiveCooling = passiveCooling;
		}
	}
}
