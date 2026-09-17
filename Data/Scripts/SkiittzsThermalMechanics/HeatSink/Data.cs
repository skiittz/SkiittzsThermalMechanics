using Sandbox.ModAPI;
using System;
using System.Xml.Serialization;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Utils;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.HeatSink
{
	public partial class HeatSinkData
	{
		public float CurrentHeat;
		[XmlIgnore] public float HeatCapacity { get; set; }
		public float AvailableCapacity => HeatCapacity - CurrentHeat;
		public float HeatRatio => (CurrentHeat / HeatCapacity);
		[XmlIgnore] public float PassiveCooling { get; set; }
		public float VentingHeat;
		public long OriginalGridId { get; set; }
		public bool IsSmallGrid { get; set; }
		public bool ShuntToParent { get; set; }
		public float SignalRadius { get; set; }
		public float SignalDecay { get; set; }

		public static void SaveData(long entityId, HeatSinkData data)
		{
			if (!ThermalAuthority.IsServer || data == null)
				return;
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

		public static HeatSinkData LoadData(IMyBeacon block, out bool configFound)
		{
			if (!ThermalAuthority.IsServer)
			{
				configFound = true;
				return new HeatSinkData { HeatCapacity = 1f, OriginalGridId = block.CubeGrid.EntityId };
			}

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

			LoadConfigFileValues(ref data, block.BlockDefinition.SubtypeId, out configFound);
			return data;
		}
	}
}
