using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game;
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
                var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage($"{entityId}.xml", typeof(HeatSinkData));
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
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(file, typeof(HeatSinkData)))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(file, typeof(HeatSinkData));
                    string content = reader.ReadToEnd();
                    reader.Close();
                    return MyAPIGateway.Utilities.SerializeFromXML<HeatSinkData>(content);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Failed to load data: {e.Message}");
            }

            return new HeatSinkData { heatCapacity = block.BlockDefinition.SubtypeName == "LargeHeatSink" ? 500000f : 250000f};
        }
    }
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon),false, new []{ "LargeHeatSink", "SmallHeatSink" })]
    public class HeatSinkLogic : MyGameLogicComponent
    {
        private IMyBeacon block;
        public HeatSinkData HeatSinkData;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyBeacon)Entity;
            if (block == null)
                return;

            Logger.Instance.LogDebug("Initializing", block);

            HeatSinkData = HeatSinkData.LoadData(block);

            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += HeatSinkLogic_AppendingCustomInfo;

            MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, OnBlockDestroyed);
        }

        public float ActiveCooling(float heatValue)
        {
            var incomingHeat = heatValue.LowerBoundedBy(0);
            if (HeatSinkData.availableCapacity > incomingHeat)
            {
                HeatSinkData.currentHeat += incomingHeat;
                return incomingHeat;
            }
            else
            {
                var remainingHeat = incomingHeat - HeatSinkData.availableCapacity;
                HeatSinkData.currentHeat = HeatSinkData.heatCapacity;
                return incomingHeat - remainingHeat;
            }
        }

        void HeatSinkLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            Logger.Instance.LogDebug("Appending Custom Info", arg1);

            var logic = arg1.GameLogic.GetAs<HeatSinkLogic>();
            var heatLevel = ((logic.HeatSinkData.currentHeat / logic.HeatSinkData.heatCapacity) * 100).LowerBoundedBy(0);
            customInfo.Append($"Heat Level: {heatLevel:N0}%\n");
            customInfo.Append($"Current IR Detectable Distance: {logic.block.Radius:N0} meters \n");
        }

        void HeatSinkLogic_OnClose(IMyEntity obj)
        {
            Logger.Instance.LogDebug("On Close", obj as IMyTerminalBlock);
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= HeatSinkLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= HeatSinkLogic_OnClose;
                    HeatSinkData.SaveData(obj.EntityId, obj.GameLogic.GetAs<HeatSinkLogic>().HeatSinkData);
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

            try
            {
                (Container.Entity as IMyCubeBlock).OnClose += HeatSinkLogic_OnClose;
            }
            catch (Exception ex)
            {

            }
            ScriptHookCreator.AddBeaconHeatRatioControl();
        }

        public override void UpdateBeforeSimulation100()
        {
            if (block == null || HeatSinkData == null || !block.IsPlayerOwned()) return;
            
            Logger.Instance.LogDebug("Simulating Heat", block);

            HeatSinkData.ventingHeat *= 0.999f;

            HeatSinkData.currentHeat = (HeatSinkData.currentHeat - Math.Min(HeatSinkData.passiveCooling, HeatSinkData.currentHeat)).LowerBoundedBy(0);
            block.Radius = Math.Min(500000, HeatSinkData.ventingHeat);
            (block as IMyTerminalBlock).RefreshCustomInfo();
        }

        private void OnBlockDestroyed(object target, MyDamageInformation info)
        {
            var tgt = target as IMyEntity;
            if (tgt == null || tgt.EntityId != block.EntityId)
                return;

            if(HeatSinkData == null)
                return;

            var currentHeat = HeatSinkData.currentHeat;
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid);

            var beacons = new List<IMyBeacon>();
            gts.GetBlocksOfType(beacons, x => x.IsWorking && x.BlockDefinition.SubtypeName.Contains("HeatSink") && x.EntityId != block.EntityId);
            foreach (var heatSink in beacons.OrderByDescending(x => x.Radius))
            {
                var gameLogic = heatSink.GameLogic.GetAs<HeatSinkLogic>();
                var sunkHeat = gameLogic.ActiveCooling(currentHeat);
                currentHeat -= sunkHeat;
            }

            var powerProducers = new List<IMyPowerProducer>();
            gts.GetBlocksOfType(powerProducers, x => x.IsWorking && x.IsSameConstructAs(block));
            foreach (var powerProducer in powerProducers)
            {
                PowerPlantHeatData heatData;
                if (powerProducer.BlockDefinition.SubtypeName.Contains("Battery"))
                    heatData = powerProducer.GameLogic.GetAs<BatteryLogic>().heatData;
                else if (powerProducer.BlockDefinition.SubtypeName.Contains("Reactor"))
                    heatData = powerProducer.GameLogic.GetAs<ReactorLogic>().heatData;
                else if (powerProducer.BlockDefinition.SubtypeName.Contains("Engine"))
                    heatData = powerProducer.GameLogic.GetAs<H2EngineLogic>().heatData;
                else
                    continue;
                

                var sunkHeat = heatData.FeedHeatBack(currentHeat);
                currentHeat -= sunkHeat;
            }

        }

        public float RemoveHeat(float heat)
        {
            var dissipatedHeat = Math.Min(heat, HeatSinkData.currentHeat);
            HeatSinkData.currentHeat -= dissipatedHeat;
            HeatSinkData.ventingHeat += dissipatedHeat;

            return dissipatedHeat;
        }
    }
}
