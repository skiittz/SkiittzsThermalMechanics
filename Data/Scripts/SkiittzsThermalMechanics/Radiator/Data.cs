using System.Xml.Serialization;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Radiator
{
	public partial class RadiatorData
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
	}
}
