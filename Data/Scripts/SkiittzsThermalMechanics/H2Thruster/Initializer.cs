using System;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.H2Thruster
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false)]
    public partial class HydrogenThrusterLogic : MyGameLogicComponent
    {
        private ThrusterHeatData heatData;
        private IMyThrust block;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyThrust)Container.Entity;
            if (block == null)
                return;

            bool configFound = false;
			heatData = ThrusterHeatData.LoadData(block, out configFound);
            if (!configFound) return;

			NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            (Container.Entity as IMyTerminalBlock).AppendingCustomInfo += ThrusterLogic_AppendingCustomInfo;
        }

        void ThrusterLogic_OnClose(IMyEntity obj)
        {
            try
            {
                if (Entity != null)
                {
                    (Container.Entity as IMyTerminalBlock).AppendingCustomInfo -= ThrusterLogic_AppendingCustomInfo;
                    (Container.Entity as IMyCubeBlock).OnClose -= ThrusterLogic_OnClose;
                    var logic = obj.GameLogic.GetAs<HydrogenThrusterLogic>();
                    if (logic == null || !logic.block.IsOwnedByAPlayer()) return;
                    ThrusterHeatData.SaveData(obj.EntityId, logic.heatData);
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
    }
}
