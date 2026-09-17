using NUnit.Framework;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Networking;

namespace SkiittzsThermalMechanics.Tests
{
    [TestFixture]
    public class ThermalBlockStateTests
    {
        [Test]
        public void HasSameValues_IdenticalStates_ReturnsTrue()
        {
            var first = CreateState();
            var second = CreateState();

            Assert.IsTrue(first.HasSameValues(second));
        }

        [Test]
        public void HasSameValues_ChangedHeat_ReturnsFalse()
        {
            var first = CreateState();
            var second = CreateState();
            second.CurrentHeat += 1f;

            Assert.IsFalse(first.HasSameValues(second));
        }

        [Test]
        public void HasSameValues_DifferentEntity_ReturnsFalse()
        {
            var first = CreateState();
            var second = CreateState();
            second.EntityId++;

            Assert.IsFalse(first.HasSameValues(second));
        }

        [Test]
        public void HasSameValues_DifferentSequence_ReturnsTrue()
        {
            var first = CreateState();
            var second = CreateState();
            first.Sequence = 10;
            second.Sequence = 11;

            Assert.IsTrue(first.HasSameValues(second));
        }

        [Test]
        public void HasSameValues_Null_ReturnsFalse()
        {
            Assert.IsFalse(CreateState().HasSameValues(null));
        }

        private static ThermalBlockState CreateState()
        {
            return new ThermalBlockState
            {
                EntityId = 42,
                Kind = ThermalBlockKind.HeatSink,
                CurrentHeat = 250f,
                HeatCapacity = 1000f,
                VentingHeat = 15f,
                SignalRadius = 3000f,
                SignalDecay = 0.99f,
                EnvironmentalMultiplier = 1.5f
            };
        }
    }
}
