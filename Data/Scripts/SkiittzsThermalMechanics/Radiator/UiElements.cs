using Sandbox.ModAPI.Interfaces.Terminal;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Game.Entity;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Radiator
{
	public partial class HeatRadiatorLogic
	{
		void RadiatorLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
		{
			var logic = arg1.GameLogic.GetAs<HeatRadiatorLogic>();
			var currentDissipation = logic.radiatorData.CurrentDissipation.ToString("F1");
			customInfo.Append($"Dissipating Heat: {currentDissipation}MW ({(logic.radiatorData.HeatRatio * 100).ToString("N0")}%)\n");
			customInfo.DebugLog($"Current Dissipation: {radiatorData.CurrentDissipation}");
			if (!logic.radiatorData.CanSeeSky)
				customInfo.Append("Radiator must be external to function!\n");
			
			logic.radiatorData.DisplayDebugMessages(customInfo);
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
			//if (radiatorData.HeatRatio > 0.7)
			//{
			//	EmitSteam();
			//}
			//else
			//{
			//	StopSteam();
			//}
		}

		private MyParticleEffect particleEffect;

		public void EmitSteam()
		{
			if (block == null || block.Closed)
				return;

			if (particleEffect != null)
				StopSteam();

			var worldMatrix = block.WorldMatrix;
			var frontPosition = block.GetPosition() + radiatorData.ForwardDirection * (block.CubeGrid.GridSize * 8);
			if (MyParticlesManager.TryCreateParticleEffect("ExhaustSmokeWhiteSmall", ref worldMatrix,
				    ref frontPosition, (uint)block.EntityId, out particleEffect))
			{
				particleEffect.UserScale = 0.4f;
			}
		}

		public void StopSteam()
		{
			particleEffect?.Stop();
		}

		private void SetBladeRotation()
		{
			var entity = (MyEntity)block;
			if (!entity.Subparts.Any())
				return;
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
