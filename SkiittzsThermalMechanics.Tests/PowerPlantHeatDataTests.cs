using NUnit.Framework;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;

namespace SkiittzsThermalMechanics.Tests
{
    [TestFixture]
    public class PowerPlantHeatDataTests
    {
        private PowerPlantHeatData CreateHeatData(float currentHeat = 0f, float heatCapacity = 1000f, int overHeatCycles = 0)
        {
            return new PowerPlantHeatData
            {
                CurrentHeat = currentHeat,
                HeatCapacity = heatCapacity,
                OverHeatCycles = overHeatCycles,
                PassiveCooling = 0.5f,
                HeatGenerationMultiplier = 1.0f
            };
        }

        #region HeatRatio Tests

        [Test]
        public void HeatRatio_NoHeat_ReturnsZero()
        {
            var data = CreateHeatData(currentHeat: 0f, heatCapacity: 1000f);
            Assert.AreEqual(0f, data.HeatRatio, 0.0001f);
        }

        [Test]
        public void HeatRatio_FullHeat_ReturnsOne()
        {
            var data = CreateHeatData(currentHeat: 1000f, heatCapacity: 1000f);
            Assert.AreEqual(1f, data.HeatRatio, 0.0001f);
        }

        [Test]
        public void HeatRatio_HalfHeat_ReturnsHalf()
        {
            var data = CreateHeatData(currentHeat: 500f, heatCapacity: 1000f);
            Assert.AreEqual(0.5f, data.HeatRatio, 0.0001f);
        }

        [Test]
        public void HeatRatio_OverCapacity_ReturnsGreaterThanOne()
        {
            var data = CreateHeatData(currentHeat: 1500f, heatCapacity: 1000f);
            Assert.IsTrue(data.HeatRatio > 1f);
        }

        #endregion

        #region ThermalFatigue Tests

        [Test]
        public void ThermalFatigue_NoOverheatCycles_ReturnsOne()
        {
            var data = CreateHeatData(overHeatCycles: 0);
            Assert.AreEqual(1f, data.ThermalFatigue, 0.0001f);
        }

        [Test]
        public void ThermalFatigue_100OverheatCycles_ReturnsTwo()
        {
            var data = CreateHeatData(overHeatCycles: 100);
            Assert.AreEqual(2f, data.ThermalFatigue, 0.0001f);
        }

        [Test]
        public void ThermalFatigue_50OverheatCycles_ReturnsOne_DueToIntegerDivision()
        {
            // OverHeatCycles / 100 uses integer division: 50 / 100 = 0
            var data = CreateHeatData(overHeatCycles: 50);
            Assert.AreEqual(1f, data.ThermalFatigue, 0.0001f);
        }

        [Test]
        public void ThermalFatigue_10OverheatCycles_ReturnsOne_DueToIntegerDivision()
        {
            // OverHeatCycles / 100 uses integer division: 10 / 100 = 0
            var data = CreateHeatData(overHeatCycles: 10);
            Assert.AreEqual(1f, data.ThermalFatigue, 0.0001f);
        }

        #endregion

        #region AvailableHeatCapacity Tests

        [Test]
        public void AvailableHeatCapacity_NoHeat_ReturnsFullCapacity()
        {
            var data = CreateHeatData(currentHeat: 0f, heatCapacity: 1000f);
            Assert.AreEqual(1000f, data.AvailableHeatCapacity, 0.0001f);
        }

        [Test]
        public void AvailableHeatCapacity_FullHeat_ReturnsZero()
        {
            var data = CreateHeatData(currentHeat: 1000f, heatCapacity: 1000f);
            Assert.AreEqual(0f, data.AvailableHeatCapacity, 0.0001f);
        }

        [Test]
        public void AvailableHeatCapacity_PartialHeat_ReturnsRemainder()
        {
            var data = CreateHeatData(currentHeat: 300f, heatCapacity: 1000f);
            Assert.AreEqual(700f, data.AvailableHeatCapacity, 0.0001f);
        }

        [Test]
        public void AvailableHeatCapacity_OverCapacity_ReturnsNegative()
        {
            var data = CreateHeatData(currentHeat: 1200f, heatCapacity: 1000f);
            Assert.IsTrue(data.AvailableHeatCapacity < 0);
        }

        #endregion

        #region FeedHeatBack Tests

        [Test]
        public void FeedHeatBack_NegativeHeat_ReturnsZero()
        {
            var data = CreateHeatData(currentHeat: 0f, heatCapacity: 1000f);
            var result = data.FeedHeatBack(-100f);
            Assert.AreEqual(0f, result);
            Assert.AreEqual(0f, data.CurrentHeat);
        }

        [Test]
        public void FeedHeatBack_ZeroHeat_ReturnsZero()
        {
            var data = CreateHeatData(currentHeat: 0f, heatCapacity: 1000f);
            var result = data.FeedHeatBack(0f);
            Assert.AreEqual(0f, result);
        }

        [Test]
        public void FeedHeatBack_HeatWithinCapacity_AcceptsAll()
        {
            var data = CreateHeatData(currentHeat: 0f, heatCapacity: 1000f);
            var result = data.FeedHeatBack(500f);
            Assert.AreEqual(500f, result);
            Assert.AreEqual(500f, data.CurrentHeat);
        }

        [Test]
        public void FeedHeatBack_HeatExceedsCapacity_CapsAtCapacity()
        {
            var data = CreateHeatData(currentHeat: 800f, heatCapacity: 1000f);
            var result = data.FeedHeatBack(500f);
            Assert.AreEqual(200f, result, 0.0001f);
            Assert.AreEqual(1000f, data.CurrentHeat, 0.0001f);
        }

        [Test]
        public void FeedHeatBack_AllowOverheat_AcceptsUpTo150Percent()
        {
            var data = CreateHeatData(currentHeat: 0f, heatCapacity: 1000f);
            var result = data.FeedHeatBack(1500f, true);
            Assert.AreEqual(1500f, result, 0.0001f);
            Assert.AreEqual(1500f, data.CurrentHeat, 0.0001f);
        }

        [Test]
        public void FeedHeatBack_AllowOverheat_CapsAt150Percent()
        {
            var data = CreateHeatData(currentHeat: 0f, heatCapacity: 1000f);
            var result = data.FeedHeatBack(2000f, true);
            Assert.AreEqual(1500f, result, 0.0001f);
            Assert.AreEqual(1500f, data.CurrentHeat, 0.0001f);
        }

        [Test]
        public void FeedHeatBack_NoOverheat_CapsAtCapacity()
        {
            var data = CreateHeatData(currentHeat: 0f, heatCapacity: 1000f);
            var result = data.FeedHeatBack(2000f, false);
            Assert.AreEqual(1000f, result, 0.0001f);
            Assert.AreEqual(1000f, data.CurrentHeat, 0.0001f);
        }

        [Test]
        public void FeedHeatBack_PartialCapacity_WithOverheat_AcceptsCorrectAmount()
        {
            var data = CreateHeatData(currentHeat: 600f, heatCapacity: 1000f);
            var result = data.FeedHeatBack(1200f, true);
            // AvailableHeatCapacity = 1000 - 600 = 400, with overheat: 400 * 1.5 = 600
            Assert.AreEqual(600f, result, 0.0001f);
            Assert.AreEqual(1200f, data.CurrentHeat, 0.0001f);
        }

        [Test]
        public void FeedHeatBack_AccumulatesHeat()
        {
            var data = CreateHeatData(currentHeat: 0f, heatCapacity: 1000f);
            data.FeedHeatBack(200f);
            data.FeedHeatBack(300f);
            Assert.AreEqual(500f, data.CurrentHeat, 0.0001f);
        }

        #endregion
    }
}
