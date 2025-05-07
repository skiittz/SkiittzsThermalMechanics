using System.Linq;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Configuration
{
	public static class ConfigUpgrader
	{
		public static ModSettings UpgradeTo(this ModSettings original, ModSettings newConfig)
		{
			foreach (var blockType in newConfig.BlockTypeSettings)
			{
				var oldValues = original.BlockTypeSettings.SingleOrDefault(x => x.SubTypeId == blockType.SubTypeId);
				if (oldValues != null)
				{
					foreach (var setting in blockType.Settings)
					{
						var oldSetting = oldValues.Settings.SingleOrDefault(x => x.Name == setting.Name);
						if (oldSetting != null)
							setting.Value = oldSetting.Value;
					}
				}
			}

			foreach (var setting in newConfig.ChatBotSettings.Settings)
			{
				var oldValues = original.ChatBotSettings.Settings.SingleOrDefault(x => x.Name == setting.Name);
				if (oldValues != null)
				{
					setting.Value = oldValues.Value;
				}
			}

			foreach (var blockType in newConfig.WeatherSettings)
			{
				var oldValues = original.WeatherSettings.SingleOrDefault(x => x.WeatherType == blockType.WeatherType);
				if (oldValues != null)
				{
					foreach (var setting in blockType.Settings)
					{
						var oldSetting = oldValues.Settings.SingleOrDefault(x => x.Name == setting.Name);
						if (oldSetting != null)
							setting.Value = oldSetting.Value;
					}
				}
			}

			foreach (var setting in newConfig.GeneralSettings)
			{
				var oldValues = original.GeneralSettings.SingleOrDefault(x => x.Name == setting.Name);
				if (oldValues != null)
				{
					setting.Value = oldValues.Value;
				}
			}

			return newConfig;
		}
	}
}
