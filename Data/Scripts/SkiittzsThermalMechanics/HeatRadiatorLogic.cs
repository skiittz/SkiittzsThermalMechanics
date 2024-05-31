using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml.Serialization;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public class RadiatorData
    {
        [XmlIgnore]
        public float MaxDissipation { get; set; }
        [XmlIgnore]
        public float StepSize {get;set;}
        public float CurrentDissipation { get; set; }
        public float HeatRatio => CurrentDissipation / MaxDissipation;
        public Color MinColor { get; set; }
        public Color MaxColor { get; set; }

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

        public static RadiatorData LoadData(IMyUpgradeModule block)
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

            if(data == null)
                data =  new RadiatorData
                {
                    MinColor = Color.Black,
                    MaxColor = Color.Red
                };

            LoadConfigFileValues(ref data, block.BlockDefinition.SubtypeId);
            return data;
        }

        public static void LoadConfigFileValues(ref RadiatorData data, string subTypeId)
        {
            if (!Configuration.BlockSettings.ContainsKey(subTypeId)) return;

            float maxDissipationConfig;
            float stepSizeConfig;
            if(Configuration.TryGetValue(subTypeId,"MaxDissipation", out maxDissipationConfig))
                data.MaxDissipation = maxDissipationConfig;
            if (Configuration.TryGetValue(subTypeId,"StepSize", out stepSizeConfig))
                data.StepSize = stepSizeConfig;
        }
    }
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_UpgradeModule), false, new []
    {
        "LargeHeatRadiatorBlock", "SmallHeatRadiatorBlock"
    })]
    public class HeatRadiatorLogic : MyGameLogicComponent
    {
        private RadiatorData radiatorData;
        private IMyUpgradeModule block;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (Container.Entity as IMyUpgradeModule);
            if (block == null)
                return;

            Logger.Instance.LogDebug("Initializing", block);

            radiatorData = RadiatorData.LoadData(block);
            
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += RadiatorLogic_AppendingCustomInfo;
        }

        void RadiatorLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            Logger.Instance.LogDebug("Appending Custom Info",arg1);
            var logic = arg1.GameLogic.GetAs<HeatRadiatorLogic>();
            customInfo.Append($"Dissipating Heat: {logic.radiatorData.CurrentDissipation.ToString("F1")}MW ({(logic.radiatorData.HeatRatio *100).ToString("N0")}%)");
        }

        void RadiatorLogic_OnClose(IMyEntity obj)
        {
            Logger.Instance.LogDebug("On Close", obj as IMyTerminalBlock);
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= RadiatorLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= RadiatorLogic_OnClose;
                    RadiatorData.SaveData(obj.EntityId, (obj).GameLogic.GetAs<HeatRadiatorLogic>().radiatorData);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            Logger.Instance.LogDebug("UpdateOnceBeforeFrame", block);

            if (block.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;
            CreateControls();
            try
            {
                (Container.Entity as IMyCubeBlock).OnClose += RadiatorLogic_OnClose;
            }
            catch (Exception ex)
            {

            }
        }

        public override void UpdateAfterSimulation100()
        {
            if (block == null || radiatorData == null || !block.IsPlayerOwned()) return;

            Logger.Instance.LogDebug("Simulating Heat", block);

            if (!block.Enabled)
            {
                radiatorData.CurrentDissipation = Math.Max(0, (radiatorData.CurrentDissipation - radiatorData.StepSize));
            }
            else
            {
                var beacon = Utilities.GetHeatSinkLogic(block.CubeGrid);
                if (beacon == null)
                    return;

                if (radiatorData.CurrentDissipation < 0)
                    radiatorData.CurrentDissipation = 0;
                if (radiatorData.CurrentDissipation > radiatorData.MaxDissipation)
                    radiatorData.CurrentDissipation = radiatorData.MaxDissipation;

                var dissipatedHeat = beacon.RemoveHeat(radiatorData.CurrentDissipation);
                if (dissipatedHeat < radiatorData.CurrentDissipation)
                    radiatorData.CurrentDissipation -= radiatorData.StepSize;
                if (dissipatedHeat == radiatorData.CurrentDissipation)
                    if (radiatorData.CurrentDissipation + radiatorData.StepSize < radiatorData.MaxDissipation)
                        radiatorData.CurrentDissipation += radiatorData.StepSize;
                    else
                        radiatorData.CurrentDissipation = radiatorData.MaxDissipation;
            }

            Animate();
            block.RefreshCustomInfo();
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

            return logic.radiatorData.MinColor;
        }

        private void SetMinColor(IMyTerminalBlock b, Color color)
        {
            var logic = b.GameLogic.GetAs<HeatRadiatorLogic>();
            if (logic == null)
                return;
            logic.radiatorData.MinColor = color;
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

            return logic.radiatorData.MaxColor;
        }

        private void SetMaxColor(IMyTerminalBlock b, Color color)
        {
            var logic = b.GameLogic.GetAs<HeatRadiatorLogic>();
            if (logic == null)
                return;
            logic.radiatorData.MaxColor = color;
        }

        private void CreateControls()
        {
            CreateMinColorPicker();
            CreateMaxColorPicker();
        }

        private void Animate()
        {
            block.SetEmissiveParts("Emissive", InterpolateColor(radiatorData.MinColor, radiatorData.MaxColor, radiatorData.HeatRatio), radiatorData.HeatRatio);
            SetBladeRotation();
        }

        private void SetBladeRotation()
        {
            var entity = (MyEntity)block;
                // Define the rotation limits
                var minRotation = MathHelper.ToRadians(-45); // Fully closed position
                var maxRotation = MathHelper.ToRadians(45); // Fully open position (90 degrees in radians)

                // Calculate the rotation angle based on the percentage
                var rotationAngle = MathHelper.Lerp(minRotation, maxRotation, radiatorData.HeatRatio);

                foreach (var subpart in entity.Subparts)
                {
                    var subpartEntity = subpart.Value as MyEntitySubpart;
                    if (subpartEntity != null)
                    {
                        // Get current local matrix of the subpart
                        MatrixD currentMatrix = subpartEntity.PositionComp.LocalMatrixRef;

                        // Extract current position
                        Vector3D currentPosition = currentMatrix.Translation;

                        // Create rotation matrix for the calculated angle
                        MatrixD rotationMatrix = MatrixD.CreateRotationZ(rotationAngle);

                        // Combine the new rotation with the original position
                        Matrix newMatrix = MatrixD.CreateWorld(currentPosition, rotationMatrix.Forward, rotationMatrix.Up);

                        // Set the new local matrix to the subpart
                        subpartEntity.PositionComp.SetLocalMatrix(ref newMatrix);
                    }
                }
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
