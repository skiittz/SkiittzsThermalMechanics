using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml.Serialization;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Battery;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Reactor;
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
        public float CurrentHeat;
        [XmlIgnore] public float HeatCapacity { get; set; }
        public float AvailableCapacity => HeatCapacity - CurrentHeat;
        public float HeatRatio => (CurrentHeat / HeatCapacity);
        [XmlIgnore] public float PassiveCooling { get; set; }
        public float VentingHeat;
        public float WeatherMult = 1;
        public long OriginalGridId { get; set; }

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
            var data = new HeatSinkData{OriginalGridId = block.CubeGrid.EntityId};
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(file, typeof(HeatSinkData)))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(file, typeof(HeatSinkData));
                    string content = reader.ReadToEnd();
                    reader.Close();
                    data = MyAPIGateway.Utilities.SerializeFromXML<HeatSinkData>(content);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Failed to load data: {e.Message}");
            }

            LoadConfigFileValues(ref data, block.BlockDefinition.SubtypeId);
            return data;
        }

        public static void LoadConfigFileValues(ref HeatSinkData data, string subTypeId)
        {
            if (!Configuration.BlockSettings.ContainsKey(subTypeId)) return;
            float heatCapacity;
            float passiveCooling;
            if (Configuration.TryGetValue(subTypeId, "HeatCapacity", out heatCapacity))
                data.HeatCapacity = heatCapacity;
            if (Configuration.TryGetValue(subTypeId, "PassiveCooling", out passiveCooling))
                data.PassiveCooling = passiveCooling;
        }
    }

	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon),false, new []{ "LargeHeatSink", "SmallHeatSink", "LargeHeatSinkUgly", "SmallHeatSinkUgly" })]
	public class HeatSinkLogic : MyGameLogicComponent
    {
        private IMyBeacon block;
        public HeatSinkData HeatSinkData;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyBeacon)Entity;
            if (block == null)
                return;

            HeatSinkData = HeatSinkData.LoadData(block);

            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += HeatSinkLogic_AppendingCustomInfo;

            MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, OnBlockDestroyed);
        }

        public float ActiveCooling(float heatValue)
        {
            var incomingHeat = heatValue.LowerBoundedBy(0);
            if (HeatSinkData.AvailableCapacity > incomingHeat)
            {
                HeatSinkData.CurrentHeat += incomingHeat;
                return incomingHeat;
            }
            else
            {
                var remainingHeat = incomingHeat - HeatSinkData.AvailableCapacity;
                HeatSinkData.CurrentHeat = HeatSinkData.HeatCapacity;
                return incomingHeat - remainingHeat;
            }
        }

        void HeatSinkLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            var logic = arg1.GameLogic.GetAs<HeatSinkLogic>();
            var heatLevel = ((logic.HeatSinkData.CurrentHeat / logic.HeatSinkData.HeatCapacity) * 100).LowerBoundedBy(0);
            customInfo.Append($"Heat Level: {heatLevel:N0}%\n");
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
                    HeatSinkData.SaveData(obj.EntityId, obj.GameLogic.GetAs<HeatSinkLogic>().HeatSinkData);
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

            try
            {
                (Container.Entity as IMyCubeBlock).OnClose += HeatSinkLogic_OnClose;
            }
            catch (Exception ex)
            {

            }
            ScriptHookCreator.AddBeaconHeatRatioControl();
        }

        public override void UpdateAfterSimulation100()
        {
            if (block == null || HeatSinkData == null || !block.IsOwnedByAPlayer()) return;
            CheckForSeparation();

            HeatSinkData.VentingHeat *= 0.999f;

            HeatSinkData.CurrentHeat = (HeatSinkData.CurrentHeat - Math.Min(HeatSinkData.PassiveCooling, HeatSinkData.CurrentHeat)).LowerBoundedBy(0);
            block.Radius = Math.Min(500000, HeatSinkData.VentingHeat*HeatSinkData.WeatherMult);
            (block as IMyTerminalBlock).RefreshCustomInfo();

            if(HeatSinkData.HeatRatio >= 1)
	            ChatBot.WarnPlayer(block, "Heat sink is at capacity!  Generators are overheating!", MessageSeverity.Warning);
			else if (HeatSinkData.HeatRatio >= 0.8)
                ChatBot.WarnPlayer(block, "Heat sink is at almost at capacity!  Need more radiators!", MessageSeverity.Tutorial);
            else if (HeatSinkData.HeatRatio >= 0.5)
                ChatBot.WarnPlayer(block, "Heat sink is at 50% capacity - do you have enough radiators?", MessageSeverity.Tutorial);
        }


        private void CheckForSeparation()
        {
	        if (block == null || HeatSinkData == null || !block.IsOwnedByAPlayer() ||
                block?.CubeGrid?.EntityId == HeatSinkData.OriginalGridId) return;
	        
			IMyEntity entity;
	        if (MyAPIGateway.Entities.TryGetEntityById(HeatSinkData.OriginalGridId, out entity))
	        {
				// Ensure the entity is a grid
				var originalGrid = entity as IMyCubeGrid;
		        if (originalGrid != null)
		        {
					ChatBot.WarnPlayer(originalGrid,
				        "Heat sink has been disconnected from grid, generators are receiving feedback heat!",
				        MessageSeverity.Warning);

			        var powerProducersOnOriginalGrid = new List<IMyPowerProducer>();
			        var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(originalGrid);
			        gts.GetBlocksOfType(powerProducersOnOriginalGrid, x => x.IsWorking);

			        foreach (var powerProducer in powerProducersOnOriginalGrid)
			        {
				        var heatData = powerProducer.GameLogic.GetAs<ReactorLogic>()?.heatData
				                       ?? powerProducer.GameLogic.GetAs<BatteryLogic>()?.heatData
				                       ?? powerProducer.GameLogic.GetAs<H2EngineLogic>()?.heatData;
				        if (heatData != null)
				        {
					        HeatSinkData.CurrentHeat -= heatData.FeedHeatBack(HeatSinkData.CurrentHeat, true);
				        }
			        }
		        }
	        }

	        HeatSinkData.OriginalGridId = block.CubeGrid.EntityId;
        }

        private void OnBlockDestroyed(object target, MyDamageInformation info)
        {
            var tgt = target as IMyEntity;
            if (tgt == null || tgt.EntityId != block.EntityId)
                return;

            if(HeatSinkData == null)
                return;

            var currentHeat = HeatSinkData.CurrentHeat;
            var ventingHeat = HeatSinkData.VentingHeat;
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid);

            var beacons = new List<IMyBeacon>();
            gts.GetBlocksOfType(beacons, x => x.IsWorking && x.BlockDefinition.SubtypeName.Contains("HeatSink") && x.EntityId != block.EntityId);
            foreach (var heatSink in beacons.OrderByDescending(x => x.Radius))
            {
                var gameLogic = heatSink.GameLogic.GetAs<HeatSinkLogic>();
                var sunkHeat = gameLogic.ActiveCooling(currentHeat);
                gameLogic.HeatSinkData.VentingHeat += ventingHeat;
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

                var ventingHeatPortion = ventingHeat / powerProducers.Count;
                var sunkHeat = heatData.FeedHeatBack(currentHeat+ventingHeatPortion);
                currentHeat -= sunkHeat;
            }

        }

        public float RemoveHeat(float heat, float weatherMult)
        {
            var dissipatedHeat = Math.Min(heat, HeatSinkData.CurrentHeat) * weatherMult;
            HeatSinkData.CurrentHeat -= dissipatedHeat;
            HeatSinkData.VentingHeat += dissipatedHeat;

            return dissipatedHeat;
        }
    }
}
