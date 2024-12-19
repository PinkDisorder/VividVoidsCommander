using System.Collections.Generic;
using Vintagestory.API.Common;

namespace VividVoidsCommander {
	public class CommanderConfig {
		public string Relocatable { get; init; }
		public string CanRelocate { get; init; }
		public string CanUseKits { get; init; }
		public List<Kit> Kits { get; set; } = new();
	}
	public class Kit {
		public string Name { get; set; }
		public List<JsonItemStack> Items { get; set; } = new List<JsonItemStack>();
		public int Uses { get; set; }
	}


}