using System.Xml.Serialization;

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
		public float WeatherMult = 1;
		public long OriginalGridId { get; set; }
	}
}
