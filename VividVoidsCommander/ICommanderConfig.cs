using System.Collections.Generic;
using Vintagestory.API.Common;

namespace VividVoidsCommander {
	public class CommanderConfig {
		public string Relocatable { get; init; }
		public string CanRelocate { get; init; }
		public string CanUseKits { get; init; }
		public string CanMakeKits { get; init; }
		public string CanDelKits { get; init; }
		public List<Kit> Kits { get; init; } = new();
	}
	public class Kit {
		public string Name { get; init; }
		public List<JsonItemStack> Items { get; init; } = new();
		public int Uses { get; init; }
	}


}