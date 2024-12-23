namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.HeatSink
{
	public partial class HeatSinkData
	{
		public static void LoadConfigFileValues(ref HeatSinkData data, string subTypeId)
		{
			if (!Configuration.BlockSettings.ContainsKey(subTypeId)) return;
			float heatCapacity;
			float passiveCooling;
			if (Configuration.TryGetValue(subTypeId, "HeatCapacity", out heatCapacity))
				data.HeatCapacity = heatCapacity;
			if (Configuration.TryGetValue(subTypeId, "PassiveCooling", out passiveCooling))
				data.PassiveCooling = passiveCooling;
		}
	}
}
