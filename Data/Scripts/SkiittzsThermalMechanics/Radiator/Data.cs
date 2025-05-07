using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core.DebuggingTools;
using VRage.Utils;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Radiator
{
	public partial class RadiatorData : IContainDebugMessages
	{
		[XmlIgnore]
		public float MaxDissipation { get; set; }
		[XmlIgnore]
		public float StepSize { get; set; }
		public float CurrentDissipation { get; set; }
		public float HeatRatio => CurrentDissipation / MaxDissipation;
		public Color MinColor { get; set; }
		public Color MaxColor { get; set; }
		[XmlIgnore]
		public bool CanSeeSky { get; set; }
		[XmlIgnore]
		public Vector3D ForwardDirection { get; set; }

		[XmlIgnore] public List<string> DebugMessages { get; set; } = new List<string>();

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

		public static RadiatorData LoadData(IMyUpgradeModule block, out bool configFound)
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

			LoadConfigFileValues(ref data, block.BlockDefinition.SubtypeId, out configFound);
			return data;
		}
	}
}
