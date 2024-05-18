using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_UpgradeModule), false, new []
    {
        "LargeHeatRadiatorBlock", "SmallHeatRadiatorBlock"
    })]
    public class HeatRadiatorLogic : MyGameLogicComponent
    {
        private IMyUpgradeModule block;
        private bool isInitialized;
        private float dissipation;
        private Color minColor = Color.Black;
        private Color maxColor = Color.Red;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Logger.Instance.LogDebug("Initializing Radiator Logic");
            block = (IMyUpgradeModule) Entity;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += RadiatorLogic_AppendingCustomInfo;
            dissipation = block.BlockDefinition.SubtypeName == "SmallHeatRadiatorBlock" ? 3f : 30f;
        }

        void RadiatorLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            var debugInfo = new StringBuilder();
            debugInfo.Append($"DEBUG INFO - {arg1.CustomName}:\n");
            Logger.Instance.LogDebug(debugInfo.ToString());

            var logic = arg1.GameLogic.GetAs<HeatRadiatorLogic>();
            customInfo.Append($"Max Heat Dissipation: {dissipation}MW");
        }

        void RadiatorLogic_OnClose(IMyEntity obj)
        {
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= RadiatorLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= RadiatorLogic_OnClose;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            if (block.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;

            if (!isInitialized)
            {
                CreateControls();
                try
                {
                    (Container.Entity as IMyCubeBlock).OnClose += RadiatorLogic_OnClose;
                }
                catch (Exception ex)
                {

                }
                isInitialized = true;
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            if (!block.Enabled)
                return;

            var beacon = Utilities.GetHeatSinkLogic(block.CubeGrid);
            if (beacon == null)
                return;

            var disspiatedHeat = beacon.RemoveHeat(dissipation);
            Animate(disspiatedHeat / dissipation);
        }

        private void CreateMinColorPicker()
        {
            var existingControls = new List<IMyTerminalControl>();
            MyAPIGateway.TerminalControls.GetControls<IMyUpgradeModule>(out existingControls);

            if (existingControls.Any(x => x.Id == "MinColorPicker"))
                return;

            var colorControl = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlColor, IMyTerminalBlock>("MinColorPicker");
            colorControl.Title = MyStringId.GetOrCompute("Pick a Color");
            colorControl.Tooltip = MyStringId.GetOrCompute("Select a color to represent an idle radiator");
            colorControl.Getter = GetMinColor;
            colorControl.Setter = SetMinColor;
            colorControl.SupportsMultipleBlocks = true;
            colorControl.Visible = b => b.GameLogic.GetAs<HeatRadiatorLogic>() != null;
            MyAPIGateway.TerminalControls.AddControl<IMyUpgradeModule>(colorControl);
        }

        private Color GetMinColor(IMyTerminalBlock b)
        {
            var logic = b.GameLogic.GetAs<HeatRadiatorLogic>();
            if (logic == null)
                return Color.Black;

            return logic.minColor;
        }

        private void SetMinColor(IMyTerminalBlock b, Color color)
        {
            var logic = b.GameLogic.GetAs<HeatRadiatorLogic>();
            if (logic == null)
                return;
            logic.minColor = color;
        }

        private void CreateMaxColorPicker()
        {
            var existingControls = new List<IMyTerminalControl>();
            MyAPIGateway.TerminalControls.GetControls<IMyUpgradeModule>(out existingControls);

            if (existingControls.Any(x => x.Id == "MaxColorPicker"))
                return;

            var colorControl = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlColor, IMyTerminalBlock>("MaxColorPicker");
            colorControl.Title = MyStringId.GetOrCompute("Pick a Color");
            colorControl.Tooltip = MyStringId.GetOrCompute("Select a color to represent a radiator at peak operation");
            colorControl.Getter = GetMaxColor;
            colorControl.Setter = SetMaxColor;
            colorControl.SupportsMultipleBlocks = true;
            colorControl.Visible = b => b.GameLogic.GetAs<HeatRadiatorLogic>() != null;
            MyAPIGateway.TerminalControls.AddControl<IMyUpgradeModule>(colorControl);
        }

        private Color GetMaxColor(IMyTerminalBlock b)
        {
            var logic = b.GameLogic.GetAs<HeatRadiatorLogic>();
            if (logic == null)
                return Color.Black;

            return logic.maxColor;
        }

        private void SetMaxColor(IMyTerminalBlock b, Color color)
        {
            var logic = b.GameLogic.GetAs<HeatRadiatorLogic>();
            if (logic == null)
                return;
            logic.maxColor = color;
        }

        private void CreateControls()
        {
            CreateMinColorPicker();
            CreateMaxColorPicker();
        }

        private void Animate(float heatLevel)
        {
            block.SetEmissiveParts("Emissive", InterpolateColor(minColor, maxColor, heatLevel), dissipation);
            //var ventPlates = new List<MyEntitySubpart>();
            //for (int i = 1; i < 5; i++)
            //{
            //    var subPart = block.GetSubpart($"HeatVentPlate{i}");
            //    if(subPart != null)
                    
            //        //ventPlates.Add(subPart);
            //}
        }

        public static Color InterpolateColor(Color color1, Color color2, double t)
        {
            // Ensure t is within the range [0, 1]
            t = Math.Max(0, Math.Min(1, t));

            // Interpolate each color component
            int r = (int)(color1.R + (color2.R - color1.R) * t);
            int g = (int)(color1.G + (color2.G - color1.G) * t);
            int b = (int)(color1.B + (color2.B - color1.B) * t);
            int a = (int)(color1.A + (color2.A - color1.A) * t);

            // Create and return the new color
            return new Color(r, g, b, a);
        }
    }
}
