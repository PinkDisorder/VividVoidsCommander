using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VividVoidsCommander {

	public class CommanderConfig {
		public string IsSelfSettable { get; set; }
		public string CanRoleSwap { get; set; }
	}

	public class VividVoidsCommanderModSystem : ModSystem {

		static CommanderConfig Config;
		public override bool ShouldLoad(EnumAppSide forSide) {
			return forSide == EnumAppSide.Server;
		}

		public override void StartServerSide(ICoreServerAPI api) {

			try {
				Config = api.LoadModConfig<CommanderConfig>($"{Mod.Info.ModID}.json");
			} catch ( Exception ex ) {

				CommanderConfig DefaultCfg = new() {
					CanRoleSwap = "canroleswap",
					IsSelfSettable = "isselfsettable"
				};

				api.Logger.Error(ex);
				api.StoreModConfig($"{Mod.Info.ModID}.json", JsonConvert.SerializeObject(DefaultCfg));

				Config = DefaultCfg;
			}


			List<IPlayerRole> SelfSettableRoles = api.Server.Config.Roles.FindAll(role => {
				return role.Privileges.Contains(Config.IsSelfSettable);
			});

			string Description = $"{Lang.Get("vividvoidscommander:iam_description")}{SelfSettableRoles}";

			api.ChatCommands.Create("iam")
				.WithDescription(Description)
				.RequiresPlayer()
				.RequiresPrivilege(Config.CanRoleSwap)
				.HandleWith((args) => {

					IPlayerRole req = SelfSettableRoles.Find(r => r.Name == (string)args[1]);

					IServerPlayer player = api.Server.Players[args.Caller.Player.ClientId];

					api.Logger.Notification(player.ToString());

					api.Permissions.SetRole(player, req);
					return null;
				});

		}

	}
}
