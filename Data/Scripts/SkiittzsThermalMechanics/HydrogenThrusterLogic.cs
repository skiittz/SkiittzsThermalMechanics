using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System.Text;
using System;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false
    ,new []{ "LargeBlockLargeHydrogenThrust", "LargeBlockSmallHydrogenThrust", "SmallBlockLargeHydrogenThrust", "SmallBlockSmallHydrogenThrust" }
    )]
    public class HydrogenThrusterLogic : MyGameLogicComponent
    {
        private ThrusterHeatData heatData;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            var subtype = ((IMyThrust)Entity).BlockDefinition.SubtypeName;
            if (!subtype.Contains("Hydrogen"))
            {
                base.Init(objectBuilder);
                return;
            }
            Logger.Instance.LogDebug("Initializing H2 Thruster Logic");
            var block = (IMyThrust)Entity;
            var passiveCooling = .25f;
            heatData = new ThrusterHeatData(block, passiveCooling);
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += ThrusterLogic_AppendingCustomInfo;
        }

        void ThrusterLogic_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder customInfo)
        {
            Logger.Instance.LogDebug("Appending Custom Info");
            var logic = arg1.GameLogic.GetAs<HydrogenThrusterLogic>();
            logic.heatData.AppendCustomThermalInfo(customInfo);
        }



        void ThrusterLogic_OnClose(IMyEntity obj)
        {
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= ThrusterLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= ThrusterLogic_OnClose;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            if (heatData.Block.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;

            if (!heatData.IsInitialized)
            {
                CreateControls();
                try
                {
                    (Container.Entity as IMyCubeBlock).OnClose += ThrusterLogic_OnClose;
                }
                catch (Exception ex)
                {

                }
                heatData.IsInitialized = true;
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            if (heatData?.Block == null)
                return;

            heatData.ApplyHeating();
            (heatData.Block as IMyTerminalBlock).RefreshCustomInfo();
        }

        private void CreateControls()
        {
            var heatPercent = MyAPIGateway.TerminalControls.CreateProperty<float, IMyThrust>("CurrentHeat");
            heatPercent.Getter = x => heatData.CurrentHeat;
            MyAPIGateway.TerminalControls.AddControl<IMyThrust>(heatPercent);
        }
    }
}
