using System.Collections.Generic;
using Vintagestory.API.Common;

namespace VividVoidsCommander {
	public class CommanderConfig {
		
		public List<Kit> Kits { get; init; } = new();
		
		public string Relocatable { get; init; }
		public string CanRelocate { get; init; }
		public string CanUseKits { get; init; }
		public string CanMakeKits { get; init; }
		public string CanDelKits { get; init; }
		public string Path { get; init; }
	}
	public class Kit {
		public required string Name { get; init; }
		public required List<JsonItemStack> Items { get; init; } = new();
		public required int Uses { get; init; }
	}


}