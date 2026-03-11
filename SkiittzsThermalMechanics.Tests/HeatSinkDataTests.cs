using NUnit.Framework;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.HeatSink;

namespace SkiittzsThermalMechanics.Tests
{
    [TestFixture]
    public class HeatSinkDataTests
    {
        private HeatSinkData CreateHeatSinkData(float currentHeat = 0f, float heatCapacity = 10000f, float passiveCooling = 0.01f, float signalDecay = 0.9992f)
        {
            return new HeatSinkData
            {
                CurrentHeat = currentHeat,
                HeatCapacity = heatCapacity,
                PassiveCooling = passiveCooling,
                SignalDecay = signalDecay,
                VentingHeat = 0f,
                OriginalGridId = 1L,
                IsSmallGrid = false,
                ShuntToParent = false,
                SignalRadius = 0f
            };
        }

        #region AvailableCapacity Tests

        [Test]
        public void AvailableCapacity_NoHeat_ReturnsFullCapacity()
        {
            var data = CreateHeatSinkData(currentHeat: 0f, heatCapacity: 10000f);
            Assert.AreEqual(10000f, data.AvailableCapacity, 0.0001f);
        }

        [Test]
        public void AvailableCapacity_FullHeat_ReturnsZero()
        {
            var data = CreateHeatSinkData(currentHeat: 10000f, heatCapacity: 10000f);
            Assert.AreEqual(0f, data.AvailableCapacity, 0.0001f);
        }

        [Test]
        public void AvailableCapacity_PartialHeat_ReturnsRemainder()
        {
            var data = CreateHeatSinkData(currentHeat: 3000f, heatCapacity: 10000f);
            Assert.AreEqual(7000f, data.AvailableCapacity, 0.0001f);
        }

        [Test]
        public void AvailableCapacity_OverCapacity_ReturnsNegative()
        {
            var data = CreateHeatSinkData(currentHeat: 12000f, heatCapacity: 10000f);
            Assert.IsTrue(data.AvailableCapacity < 0);
        }

        #endregion

        #region HeatRatio Tests

        [Test]
        public void HeatRatio_NoHeat_ReturnsZero()
        {
            var data = CreateHeatSinkData(currentHeat: 0f, heatCapacity: 10000f);
            Assert.AreEqual(0f, data.HeatRatio, 0.0001f);
        }

        [Test]
        public void HeatRatio_FullHeat_ReturnsOne()
        {
            var data = CreateHeatSinkData(currentHeat: 10000f, heatCapacity: 10000f);
            Assert.AreEqual(1f, data.HeatRatio, 0.0001f);
        }

        [Test]
        public void HeatRatio_HalfHeat_ReturnsHalf()
        {
            var data = CreateHeatSinkData(currentHeat: 5000f, heatCapacity: 10000f);
            Assert.AreEqual(0.5f, data.HeatRatio, 0.0001f);
        }

        [Test]
        public void HeatRatio_OverCapacity_ReturnsGreaterThanOne()
        {
            var data = CreateHeatSinkData(currentHeat: 15000f, heatCapacity: 10000f);
            Assert.IsTrue(data.HeatRatio > 1f);
        }

        #endregion

        #region Property Defaults Tests

        [Test]
        public void VentingHeat_DefaultsToZero()
        {
            var data = CreateHeatSinkData();
            Assert.AreEqual(0f, data.VentingHeat);
        }

        [Test]
        public void SignalDecay_CanBeSetAndRetrieved()
        {
            var data = CreateHeatSinkData(signalDecay: 0.999f);
            Assert.AreEqual(0.999f, data.SignalDecay, 0.0001f);
        }

        [Test]
        public void IsSmallGrid_DefaultsFalse()
        {
            var data = CreateHeatSinkData();
            Assert.IsFalse(data.IsSmallGrid);
        }

        [Test]
        public void ShuntToParent_DefaultsFalse()
        {
            var data = CreateHeatSinkData();
            Assert.IsFalse(data.ShuntToParent);
        }

        [Test]
        public void OriginalGridId_IsSet()
        {
            var data = CreateHeatSinkData();
            Assert.AreEqual(1L, data.OriginalGridId);
        }

        #endregion

        #region Signal Decay Behavior Tests

        [Test]
        public void VentingHeat_MultipliedBySignalDecay_Decreases()
        {
            var data = CreateHeatSinkData(signalDecay: 0.9992f);
            data.VentingHeat = 1000f;

            data.VentingHeat *= data.SignalDecay;

            Assert.AreEqual(999.2f, data.VentingHeat, 0.1f);
        }

        [Test]
        public void VentingHeat_MultipleDecayCycles_ConvergesToZero()
        {
            var data = CreateHeatSinkData(signalDecay: 0.5f);
            data.VentingHeat = 1000f;

            for (int i = 0; i < 20; i++)
                data.VentingHeat *= data.SignalDecay;

            Assert.IsTrue(data.VentingHeat < 0.01f);
        }

        #endregion
    }
}
