using NUnit.Framework;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;

namespace SkiittzsThermalMechanics.Tests
{
    [TestFixture]
    public class UtilitiesTests
    {
        #region LowerBoundedBy Tests

        [Test]
        public void LowerBoundedBy_ValueAboveBound_ReturnsValue()
        {
            var result = 10f.LowerBoundedBy(5f);
            Assert.AreEqual(10f, result);
        }

        [Test]
        public void LowerBoundedBy_ValueBelowBound_ReturnsBound()
        {
            var result = 3f.LowerBoundedBy(5f);
            Assert.AreEqual(5f, result);
        }

        [Test]
        public void LowerBoundedBy_ValueEqualsBound_ReturnsBound()
        {
            var result = 5f.LowerBoundedBy(5f);
            Assert.AreEqual(5f, result);
        }

        [Test]
        public void LowerBoundedBy_NegativeValue_BelowBound_ReturnsBound()
        {
            var result = (-10f).LowerBoundedBy(0f);
            Assert.AreEqual(0f, result);
        }

        [Test]
        public void LowerBoundedBy_NegativeValue_AboveBound_ReturnsValue()
        {
            var result = (-2f).LowerBoundedBy(-5f);
            Assert.AreEqual(-2f, result);
        }

        [Test]
        public void LowerBoundedBy_ZeroValue_ZeroBound_ReturnsZero()
        {
            var result = 0f.LowerBoundedBy(0f);
            Assert.AreEqual(0f, result);
        }

        [Test]
        public void LowerBoundedBy_LargeValue_ReturnsValue()
        {
            var result = 1000000f.LowerBoundedBy(1f);
            Assert.AreEqual(1000000f, result);
        }

        #endregion

        #region UpperBoundedBy Tests

        [Test]
        public void UpperBoundedBy_ValueBelowBound_ReturnsValue()
        {
            var result = 3f.UpperBoundedBy(10f);
            Assert.AreEqual(3f, result);
        }

        [Test]
        public void UpperBoundedBy_ValueAboveBound_ReturnsBound()
        {
            var result = 15f.UpperBoundedBy(10f);
            Assert.AreEqual(10f, result);
        }

        [Test]
        public void UpperBoundedBy_ValueEqualsBound_ReturnsBound()
        {
            var result = 10f.UpperBoundedBy(10f);
            Assert.AreEqual(10f, result);
        }

        [Test]
        public void UpperBoundedBy_NegativeValue_BelowBound_ReturnsValue()
        {
            var result = (-10f).UpperBoundedBy(-5f);
            Assert.AreEqual(-10f, result);
        }

        [Test]
        public void UpperBoundedBy_NegativeValue_AboveBound_ReturnsBound()
        {
            var result = (-2f).UpperBoundedBy(-5f);
            Assert.AreEqual(-5f, result);
        }

        [Test]
        public void UpperBoundedBy_ZeroValue_ZeroBound_ReturnsZero()
        {
            var result = 0f.UpperBoundedBy(0f);
            Assert.AreEqual(0f, result);
        }

        [Test]
        public void UpperBoundedBy_LargeValue_SmallBound_ReturnsBound()
        {
            var result = 1000000f.UpperBoundedBy(1f);
            Assert.AreEqual(1f, result);
        }

        #endregion

        #region Combined Boundary Tests

        [Test]
        public void LowerThenUpper_ClampsToRange()
        {
            var result = (-5f).LowerBoundedBy(0f).UpperBoundedBy(100f);
            Assert.AreEqual(0f, result);
        }

        [Test]
        public void UpperThenLower_ClampsToRange()
        {
            var result = 150f.UpperBoundedBy(100f).LowerBoundedBy(0f);
            Assert.AreEqual(100f, result);
        }

        [Test]
        public void ValueInRange_ClampsUnchanged()
        {
            var result = 50f.LowerBoundedBy(0f).UpperBoundedBy(100f);
            Assert.AreEqual(50f, result);
        }

        #endregion
    }
}
