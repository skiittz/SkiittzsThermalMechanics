using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Radiator
{
	public partial class RadiatorData
	{
		public static void LoadConfigFileValues(ref RadiatorData data, string subTypeId, out bool configFound)
		{
			if (!Configuration.Configuration.BlockSettings.ContainsKey(subTypeId))
			{
				configFound = false;
				return;
			};

			configFound = true;
			float maxDissipationConfig;
			float stepSizeConfig;
			string forwardFace;
			if (Configuration.Configuration.TryGetBlockSettingValue(subTypeId, "MaxDissipation", out maxDissipationConfig))
				data.MaxDissipation = maxDissipationConfig;
			if (Configuration.Configuration.TryGetBlockSettingValue(subTypeId, "StepSize", out stepSizeConfig))
				data.StepSize = stepSizeConfig;
			if (Configuration.Configuration.TryGetBlockSettingValue(subTypeId, "ForwardFace", out forwardFace))
			{
				switch (forwardFace)
				{
					case "Up":
						data.ForwardDirection = Vector3D.Up;
						break;
					case "Forward":
					default:
						data.ForwardDirection = Vector3D.Forward;
						break;
				}
			}
		}
	}
}
