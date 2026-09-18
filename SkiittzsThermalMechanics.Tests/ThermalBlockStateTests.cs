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

        [TestCase(nameof(ThermalBlockState.Kind))]
        [TestCase(nameof(ThermalBlockState.HeatCapacity))]
        [TestCase(nameof(ThermalBlockState.LastHeatDelta))]
        [TestCase(nameof(ThermalBlockState.OverheatCycles))]
        [TestCase(nameof(ThermalBlockState.IsUnknownSubtype))]
        [TestCase(nameof(ThermalBlockState.VentingHeat))]
        [TestCase(nameof(ThermalBlockState.IsSmallGrid))]
        [TestCase(nameof(ThermalBlockState.ShuntToParent))]
        [TestCase(nameof(ThermalBlockState.SignalRadius))]
        [TestCase(nameof(ThermalBlockState.SignalDecay))]
        [TestCase(nameof(ThermalBlockState.CurrentDissipation))]
        [TestCase(nameof(ThermalBlockState.MaxDissipation))]
        [TestCase(nameof(ThermalBlockState.CanSeeSky))]
        [TestCase(nameof(ThermalBlockState.MinimumColor))]
        [TestCase(nameof(ThermalBlockState.MaximumColor))]
        [TestCase(nameof(ThermalBlockState.EnvironmentalMultiplier))]
        public void HasSameValues_ChangedField_ReturnsFalse(string fieldName)
        {
            var first = CreateState();
            var second = CreateState();
            var property = typeof(ThermalBlockState).GetProperty(fieldName);
            Assert.IsNotNull(property);

            var value = property.GetValue(second);
            if (property.PropertyType == typeof(bool))
                property.SetValue(second, !(bool)value);
            else if (property.PropertyType == typeof(float))
                property.SetValue(second, (float)value + 1f);
            else if (property.PropertyType == typeof(int))
                property.SetValue(second, (int)value + 1);
            else if (property.PropertyType == typeof(uint))
                property.SetValue(second, (uint)value + 1u);
            else if (property.PropertyType == typeof(ThermalBlockKind))
                property.SetValue(second, ThermalBlockKind.Battery);
            else
                Assert.Fail($"Unexpected field type: {property.PropertyType}");

            Assert.IsFalse(first.HasSameValues(second), $"Changed {fieldName} must be compared.");
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
