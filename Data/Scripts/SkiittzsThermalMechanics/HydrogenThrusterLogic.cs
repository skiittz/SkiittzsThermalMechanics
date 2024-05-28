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

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false
    ,new []{ "LargeBlockLargeHydrogenThrust", "LargeBlockSmallHydrogenThrust", 
        "SmallBlockLargeHydrogenThrust", "SmallBlockSmallHydrogenThrust",
        "LargeBlockLargeHydrogenThrustIndustrial", "LargeBlockSmallHydrogenThrustIndustrial",
        "SmallBlockLargeHydrogenThrustIndustrial", "SmallBlockSmallHydrogenThrustIndustrial"
    }
    )]
    public class HydrogenThrusterLogic : MyGameLogicComponent
    {
        private ThrusterHeatData heatData;
        private IMyThrust block;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Logger.Instance.LogDebug("Initializing H2 Thruster Logic");

            block = (IMyThrust)Container.Entity;
            if (block == null)
                return;

            heatData = ThrusterHeatData.LoadData(block);

            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += ThrusterLogic_AppendingCustomInfo;
        }

        void ThrusterLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            Logger.Instance.LogDebug("Appending Custom Info");
            var logic = arg1.GameLogic.GetAs<HydrogenThrusterLogic>();
            if (logic == null)
                return;
            logic.heatData.AppendCustomThermalInfo(logic.block, customInfo);
        }



        void ThrusterLogic_OnClose(IMyEntity obj)
        {
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= ThrusterLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= ThrusterLogic_OnClose;
                    ThrusterHeatData.SaveData(obj.EntityId, obj.GameLogic.GetAs<HydrogenThrusterLogic>().heatData);
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

                AddCurrentHeatControl();
                try
                {
                    (Container.Entity as IMyCubeBlock).OnClose += ThrusterLogic_OnClose;
                }
                catch (Exception ex)
                {

                }
        }

        public override void UpdateBeforeSimulation100()
        {
            if (block == null || heatData == null )
                return;

            heatData.ApplyHeating(block);
            block.RefreshCustomInfo();
        }

        public void AddCurrentHeatControl()
        {
            var existingControls = new List<IMyTerminalControl>();
            MyAPIGateway.TerminalControls.GetControls<IMyThrust>(out existingControls);
            if (existingControls.Any(x => x.Id == Utilities.CurrentHeatControlId))
                return;

            var heatPercent =
                MyAPIGateway.TerminalControls.CreateProperty<float, IMyThrust>(Utilities.CurrentHeatControlId);
            heatPercent.Getter = x => heatData.CurrentHeat;
            MyAPIGateway.TerminalControls.AddControl<IMyThrust>(heatPercent);
        }
    }
}
