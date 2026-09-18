using Sandbox.ModAPI;
using System;
using System.Xml.Serialization;
using VRage.Utils;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core
{
	public partial class ThrusterHeatData
	{
		public float CurrentHeat { get; set; }
		[XmlIgnore]
		public float PassiveCooling { get; set; }
		public float LastHeatDelta { get; set; }
		[XmlIgnore]
		public float MwHeatPerNewtonThrust { get; set; }

		public static void SaveData(long entityId, ThrusterHeatData data)
		{
			if (!ThermalAuthority.IsServer || data == null) return;
			try
			{
				using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage($"{entityId}.xml", typeof(ThrusterHeatData)))
				{
					writer.Write(MyAPIGateway.Utilities.SerializeToXML(data));
					writer.Flush();
				}
			}
			catch (Exception e)
			{
				MyLog.Default.WriteLine($"Failed to save data: {e.Message}");
			}
		}
		public static ThrusterHeatData LoadData(IMyThrust block, out bool configFound)
		{
			if (!ThermalAuthority.IsServer)
			{
				configFound = true;
				return new ThrusterHeatData();
			}

			var file = $"{block.EntityId}.xml";
			var data = new ThrusterHeatData();
			try
			{
				if (MyAPIGateway.Utilities.FileExistsInWorldStorage(file, typeof(ThrusterHeatData)))
				{
					string content;
					using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(file, typeof(ThrusterHeatData)))
						content = reader.ReadToEnd();
					data = MyAPIGateway.Utilities.SerializeFromXML<ThrusterHeatData>(content);
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

	public partial class PowerPlantHeatData
	{
		public float CurrentHeat { get; set; }
		[XmlIgnore] public float HeatCapacity { get; set; }
		[XmlIgnore] public float PassiveCooling { get; set; }
		public float LastHeatDelta;
		public float HeatRatio => (CurrentHeat / HeatCapacity);
		public int OverHeatCycles { get; set; }
		public float ThermalFatigue => 1 + (OverHeatCycles / 100);
		public float AvailableHeatCapacity => HeatCapacity - CurrentHeat;
		[XmlIgnore] public float HeatGenerationMultiplier { get; set; }
		[XmlIgnore] public bool IsUnknownSubType { get; set; }
		public static void SaveData(long entityId, PowerPlantHeatData data)
		{
			if (!ThermalAuthority.IsServer || data == null)
				return;
			try
			{
				using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage($"{entityId}.xml", typeof(PowerPlantHeatData)))
				{
					writer.Write(MyAPIGateway.Utilities.SerializeToXML(data));
					writer.Flush();
				}
			}
			catch (Exception e)
			{
				MyLog.Default.WriteLine($"Failed to save data: {e.Message}");
			}
		}
		public static PowerPlantHeatData LoadData(IMyPowerProducer block, out bool configFound, string defaultId = "")
		{
			if (!ThermalAuthority.IsServer)
			{
				configFound = true;
				return new PowerPlantHeatData { HeatCapacity = 1f, HeatGenerationMultiplier = 1f };
			}

			var file = $"{block.EntityId}.xml";
			var heatData = new PowerPlantHeatData();
			try
			{
				if (MyAPIGateway.Utilities.FileExistsInWorldStorage(file, typeof(PowerPlantHeatData)))
				{
					string content;
					using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(file, typeof(PowerPlantHeatData)))
						content = reader.ReadToEnd();
					heatData = MyAPIGateway.Utilities.SerializeFromXML<PowerPlantHeatData>(content);
				}
			}
			catch (Exception e)
			{
				MyLog.Default.WriteLine($"Failed to load data: {e.Message}");
			}

			LoadConfigFileValues(ref heatData, block.BlockDefinition.SubtypeId, out configFound);
			if(!configFound && !string.IsNullOrEmpty(defaultId))
			{
				heatData.IsUnknownSubType = true;
				LoadConfigFileValues(ref heatData, defaultId, out configFound);
            }
            return heatData;
		}
	}
}
