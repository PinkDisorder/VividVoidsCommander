using System;
using System.Collections.Generic;
using System.Linq;
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

			Config = api.LoadModConfig<CommanderConfig>($"{Mod.Info.ModID}.json") ?? new() {
				CanRoleSwap = "canroleswap",
				IsSelfSettable = "isselfsettable"
			};

			api.StoreModConfig(Config, $"{Mod.Info.ModID}.json");


			List<IPlayerRole> SelfSettableRoles = api.Server.Config.Roles.FindAll(role => {
				return role.Privileges.Contains(Config.IsSelfSettable);
			});

			string RoleList = String.Join<string>(", ", SelfSettableRoles.ConvertAll(role => role.Code).ToArray());

			api.ChatCommands.Create(Lang.Get("vividvoidscommander:iam_cmd_name"))
				.WithDescription($"{Lang.Get("vividvoidscommander:iam_description")}{RoleList}")
				.RequiresPlayer()
				.RequiresPrivilege(Config.CanRoleSwap)
				.WithArgs(api.ChatCommands.Parsers.Unparsed(Lang.Get("vividvoidscommander:iam_arg_name")))
				.HandleWith((args) => {
					var first = args.RawArgs.PopWord();
					IPlayerRole req = SelfSettableRoles.Find(r => r.Code.Equals(first));

					if ( req == null ) {
						return TextCommandResult.Error(
							Lang.Get("vividvoidscommander:missingarg") +
							Lang.Get("vividvoidscommander:iam_arg_name") +
							Lang.Get("vividvoidscommander:empty_error") +
							Lang.Get("vividvoidscommander:iam_cmd_name")
						);
					}

					IServerPlayer player = api.Server.Players.ToList().Find(e => e.ClientId.Equals(args.Caller.Player.ClientId));

					api.Logger.Notification(player.ToString());

					api.Permissions.SetRole(player, req);
					return TextCommandResult.Success();
				});

		}

	}
}
