using System.Collections.Generic;
using System.Linq;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Configuration
{
	public class ModSettings
	{
		public List<BlockType> BlockTypeSettings { get; set; }
		public ChatBotSettings ChatBotSettings { get; set; }
		public List<WeatherSetting> WeatherSettings { get; set; }
		public List<Setting> GeneralSettings { get; set; }

		public float ConfigVersion { get; set; }
		public static float CurrentVersion = 1.8f;

		public static ModSettings Default()
		{
			return new ModSettings
			{
				ConfigVersion = CurrentVersion,
				BlockTypeSettings = Enumerable.ToList<BlockType>(Configuration.DefaultBlockSettings()),
				ChatBotSettings = Configuration.DefaultChatBotSettings(),
				WeatherSettings = Enumerable.ToList<WeatherSetting>(Configuration.DefaultWeatherSettings()),
				GeneralSettings = Enumerable.ToList<Setting>(Configuration.DefaultGeneralSettings())
			};
		}
	}
}
