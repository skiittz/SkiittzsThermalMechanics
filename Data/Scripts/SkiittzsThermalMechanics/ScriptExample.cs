/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
{
    
    public class ScriptExample : MyGridProgram
    {
        public void Main(string argument, UpdateType updateSource)
        {
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(blocks);
            foreach (var block in blocks)
            {
                var property = block.GetProperty("HeatRatio") as ITerminalProperty<float>;
                float value = 0f;
                if (property != null)
                {
                    value = property.GetValue(block);
                    Echo($"{block.CustomName}-{property.Id}:{value}");
                }
                else
                    Echo($"{block.CustomName}-???");
            }
        }
    }
    
}
*/