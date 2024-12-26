using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

// ReSharper disable InconsistentNaming

namespace VividVoidsCommander {
	public class VividVoidsCommanderModSystem : ModSystem {

		private static ICoreServerAPI sapi;
		
		private static readonly CommanderConfig DefaultConfig = new() {
			CanRelocate = "canrelocate",
			Relocatable = "relocatable",
			CanUseKits = "canusekits",
			CanMakeKits = "canmakekits",
			CanDelKits = "candelkits",
			Kits = new List<Kit>(),
		};

		private static CommanderConfig Config;

		public override bool ShouldLoad(EnumAppSide forSide) {
			return forSide == EnumAppSide.Server;
		}
		
		public static TextCommandResult Tp2pCommandHandler(TextCommandCallingArgs args) {
			return TextCommandResult.Deferred;
		}


		public override void AssetsFinalize(ICoreAPI api) {
			sapi = (ICoreServerAPI)api;
			Config = sapi.LoadModConfig<CommanderConfig>($"{Mod.Info.ModID}.json");
			if ( Config != null ) return;
			// Config missing, store the default one.
			Config = DefaultConfig;
			Config.Path = $"{Mod.Info.ModID}.json";
			sapi.StoreModConfig(Config, Config.Path);
		}
		
		public override void StartServerSide(ICoreServerAPI api) {
			// iam command
			new Commands.Kit().Init(api, Config);
			new Commands.Relocate().Init(api, Config);
			
		}

	}
}
