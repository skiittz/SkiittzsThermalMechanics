using Sandbox.ModAPI;
using System;
using VRage.Utils;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Radiator
{
	public partial class RadiatorData
	{
		public static void LoadConfigFileValues(ref RadiatorData data, string subTypeId)
		{
			if (!Configuration.BlockSettings.ContainsKey(subTypeId))
			{
				data.MaxDissipation = 0.0001f;
				data.StepSize = 0.0001f;
			};

			float maxDissipationConfig;
			float stepSizeConfig;
			if (Configuration.TryGetValue(subTypeId, "MaxDissipation", out maxDissipationConfig))
				data.MaxDissipation = maxDissipationConfig;
			if (Configuration.TryGetValue(subTypeId, "StepSize", out stepSizeConfig))
				data.StepSize = stepSizeConfig;
		}
	}
}
