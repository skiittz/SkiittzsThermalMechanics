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
			float signalDecay;
			bool shuntToParent;
			if (Configuration.Configuration.TryGetBlockSettingValue(subTypeId, "HeatCapacity", out heatCapacity))
				data.HeatCapacity = heatCapacity;
			if (Configuration.Configuration.TryGetBlockSettingValue(subTypeId, "PassiveCooling", out passiveCooling))
				data.PassiveCooling = passiveCooling;
			if (Configuration.Configuration.TryGetBlockSettingValue(subTypeId, "SignalDecay", out signalDecay))
				data.SignalDecay = signalDecay;
			if (Configuration.Configuration.TryGetBlockSettingValue(subTypeId, "SmallGridShuntsToLarge", out shuntToParent))
				data.ShuntToParent = shuntToParent;
		}
	}
}
