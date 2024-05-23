using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    public class HeatSinkData
    {
        public float currentHeat;
        public float heatCapacity;
        public float availableCapacity => heatCapacity - currentHeat;
        public float HeatRatio => (currentHeat / heatCapacity);
        public float passiveCooling = 0.01f;
        public float ventingHeat;

        public static void SaveData(long entityId, HeatSinkData data)
        {
            try
            {
                var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage($"{entityId}.xml", typeof(HeatSinkData));
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(data));
                writer.Flush();
                writer.Close();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Failed to save data: {e.Message}");
            }
        }

        public static HeatSinkData LoadData(IMyBeacon block)
        {
            var file = $"{block.EntityId}.xml";
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInLocalStorage(file, typeof(HeatSinkData)))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(file, typeof(HeatSinkData));
                    string content = reader.ReadToEnd();
                    reader.Close();
                    return MyAPIGateway.Utilities.SerializeFromXML<HeatSinkData>(content);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Failed to load data: {e.Message}");
            }

            return new HeatSinkData { heatCapacity = block.BlockDefinition.SubtypeName == "LargeHeatSink" ? 1000000f : 500000f};
        }
    }
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon),false, new []{ "LargeHeatSink", "SmallHeatSink" })]
    public class HeatSinkLogic : MyGameLogicComponent
    {
        private IMyBeacon block;
        private HeatSinkData heatSinkData;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Logger.Instance.LogDebug("Initializing Heat Sink Logic");
            block = (IMyBeacon)Entity;
            if (block == null || !block.CubeGrid.IsPlayerOwnedGrid())
                return;

            heatSinkData = HeatSinkData.LoadData(block);

            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += HeatSinkLogic_AppendingCustomInfo;
        }

        public float ActiveCooling(float incomingHeat)
        {
            if (heatSinkData.availableCapacity > incomingHeat)
            {
                heatSinkData.currentHeat += incomingHeat;
                return incomingHeat;
            }
            else
            {
                var remainingHeat = incomingHeat - heatSinkData.availableCapacity;
                heatSinkData.currentHeat = heatSinkData.heatCapacity;
                return incomingHeat - remainingHeat;
            }
        }

        void HeatSinkLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            var debugInfo = new StringBuilder();
            debugInfo.Append($"DEBUG INFO - {arg1.CustomName}:\n");
            debugInfo.Append($"Current Heat: {heatSinkData.currentHeat}\n");
            debugInfo.Append($"Heat Capacity: {heatSinkData.heatCapacity}\n");
            Logger.Instance.LogDebug(debugInfo.ToString());

            var logic = arg1.GameLogic.GetAs<HeatSinkLogic>();
            customInfo.Append($"Heat Level: {(logic.heatSinkData.currentHeat / logic.heatSinkData.heatCapacity) * 100}%\n");
            customInfo.Append($"Current IR Detectable Distance: {logic.block.Radius:N0} meters \n");
        }

        void HeatSinkLogic_OnClose(IMyEntity obj)
        {
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= HeatSinkLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= HeatSinkLogic_OnClose;
                    HeatSinkData.SaveData(obj.EntityId, obj.GameLogic.GetAs<HeatSinkLogic>().heatSinkData);
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

            AddHeatRatioControl();
            try
            {
                (Container.Entity as IMyCubeBlock).OnClose += HeatSinkLogic_OnClose;
            }
            catch (Exception ex)
            {

            }
        }

        public override void UpdateBeforeSimulation100()
        {
            heatSinkData.currentHeat -= Math.Min(heatSinkData.passiveCooling, heatSinkData.currentHeat);
            block.Radius = Math.Min(500000, heatSinkData.ventingHeat);
            (block as IMyTerminalBlock).RefreshCustomInfo();
            heatSinkData.ventingHeat *= 0.95f;
        }

        public void AddHeatRatioControl()
        {
            var existingControls = new List<IMyTerminalControl>();
            MyAPIGateway.TerminalControls.GetControls<IMyBeacon>(out existingControls);
            if (existingControls.Any(x => x.Id == Utilities.HeatRatioControlId))
                return;

            var heatPercent = MyAPIGateway.TerminalControls.CreateProperty<float, IMyBeacon>(Utilities.HeatRatioControlId);
            heatPercent.Getter = x => heatSinkData.HeatRatio;
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(heatPercent);
        }

        public float RemoveHeat(float heat)
        {
            var dissipatedHeat = Math.Min(heat, heatSinkData.currentHeat);
            heatSinkData.currentHeat -= dissipatedHeat;
            heatSinkData.ventingHeat += dissipatedHeat;

            return dissipatedHeat;
        }
    }
}
