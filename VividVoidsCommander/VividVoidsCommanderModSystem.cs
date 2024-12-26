using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace VividVoidsCommander {
	public class VividVoidsCommanderModSystem : ModSystem {

		private static CommanderConfig _config;

		public override bool ShouldLoad(EnumAppSide forSide) {
			return forSide == EnumAppSide.Server;
		}

		public override void AssetsFinalize(ICoreAPI api) {
			_config = api.LoadModConfig<CommanderConfig>($"{Mod.Info.ModID}.json");
			if ( _config != null ) return;
			// Config missing, store the default one.
			_config = new CommanderConfig {
				CanRelocate = "canrelocate",
				Relocatable = "relocatable",
				CanUseKits = "canusekits",
				CanMakeKits = "canmakekits",
				CanDelKits = "candelkits",
				Kits = new List<Kit>(),
				Path = $"{Mod.Info.ModID}.json"
			};
			api.StoreModConfig(_config, _config.Path);
		}

		public override void StartServerSide(ICoreServerAPI api) {
			new Commands.Kit().Init(api, _config);
			new Commands.Relocate().Init(api, _config);
		}
	}
}
