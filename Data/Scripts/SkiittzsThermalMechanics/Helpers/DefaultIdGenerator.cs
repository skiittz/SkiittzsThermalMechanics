using Sandbox.ModAPI;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Helpers
{
    public static class DefaultIdGenerator
    {
        public static string DefaultId(this IMyPowerProducer block, string type)
        {
            return $"{(block.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Small ? "Small" : "Large")}GridUnknown{type}";
        }
    }
}
