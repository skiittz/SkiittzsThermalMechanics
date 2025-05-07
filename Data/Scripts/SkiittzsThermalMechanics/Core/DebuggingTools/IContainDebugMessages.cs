using System.Collections.Generic;

namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics.Core.DebuggingTools
{
	public interface IContainDebugMessages
	{
		List<string> DebugMessages { get; set; }
	}
}
