using Sandbox.ModAPI;
using System;
using VRage.Utils;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Radiator
{
	public partial class RadiatorData
	{
		public static void SaveData(long entityId, RadiatorData data)
		{
			try
			{
				var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage($"{entityId}.xml", typeof(RadiatorData));
				writer.Write(MyAPIGateway.Utilities.SerializeToXML(data));
				writer.Flush();
				writer.Close();
			}
			catch (Exception e)
			{
				MyLog.Default.WriteLine($"Failed to save data: {e.Message}");
			}
		}

		public static RadiatorData LoadData(IMyUpgradeModule block)
		{
			var file = $"{block.EntityId}.xml";
			RadiatorData data = null;
			try
			{
				if (MyAPIGateway.Utilities.FileExistsInWorldStorage(file, typeof(RadiatorData)))
				{
					var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(file, typeof(RadiatorData));
					string content = reader.ReadToEnd();
					reader.Close();
					data = MyAPIGateway.Utilities.SerializeFromXML<RadiatorData>(content);
				}
			}
			catch (Exception e)
			{
				MyLog.Default.WriteLine($"Failed to load data: {e.Message}");
			}

			if (data == null)
				data = new RadiatorData
				{
					MinColor = Color.Black,
					MaxColor = Color.Red
				};

			LoadConfigFileValues(ref data, block.BlockDefinition.SubtypeId);
			return data;
		}

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
