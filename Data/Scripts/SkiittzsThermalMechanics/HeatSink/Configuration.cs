using Sandbox.ModAPI;
using System;
using VRage.Utils;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.HeatSink
{
	public partial class HeatSinkData
	{
		public static void SaveData(long entityId, HeatSinkData data)
		{
			try
			{
				var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage($"{entityId}.xml", typeof(HeatSinkData));
				writer.Write(MyAPIGateway.Utilities.SerializeToXML(data));
				writer.Flush();
				writer.Close();
			}
			catch (Exception e)
			{
				MyLog.Default.WriteLine($"Failed to save data: {e.Message}");
			}
		}

		public static HeatSinkData LoadData(IMyBeacon block)
		{
			var file = $"{block.EntityId}.xml";
			var data = new HeatSinkData { OriginalGridId = block.CubeGrid.EntityId };
			try
			{
				if (MyAPIGateway.Utilities.FileExistsInWorldStorage(file, typeof(HeatSinkData)))
				{
					var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(file, typeof(HeatSinkData));
					string content = reader.ReadToEnd();
					reader.Close();
					data = MyAPIGateway.Utilities.SerializeFromXML<HeatSinkData>(content);
				}
			}
			catch (Exception e)
			{
				MyLog.Default.WriteLine($"Failed to load data: {e.Message}");
			}

			LoadConfigFileValues(ref data, block.BlockDefinition.SubtypeId);
			return data;
		}

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
