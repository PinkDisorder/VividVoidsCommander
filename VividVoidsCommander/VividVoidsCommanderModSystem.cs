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

		static ICoreServerAPI sapi;

		static List<IPlayerRole> SelfSettableRoles;

		static string RoleList;

		public override bool ShouldLoad(EnumAppSide forSide) {
			return forSide == EnumAppSide.Server;
		}

		static TextCommandResult IAmCommandHandler(TextCommandCallingArgs args) {
			string arg = args.RawArgs.PopWord();
			IPlayerRole request = SelfSettableRoles.Find(r => r.Code.Equals(arg));

			if ( request == null ) {
				return TextCommandResult.Error(
					Lang.Get("vividvoidscommander:missingarg") +
					Lang.Get("vividvoidscommander:iam_arg_name")
				);
			}

			sapi.Permissions.SetRole((IServerPlayer)args.Caller.Player, request);

			return TextCommandResult.Success($"{Lang.Get("vividvoidscommander:iam_success")}{request.Code}");
		}

		public override void StartServerSide(ICoreServerAPI api) {

			sapi = api;

			Config = api.LoadModConfig<CommanderConfig>($"{Mod.Info.ModID}.json") ?? new() {
				CanRoleSwap = "canroleswap",
				IsSelfSettable = "isselfsettable"
			};

			api.StoreModConfig(Config, $"{Mod.Info.ModID}.json");


			SelfSettableRoles = api.Server.Config.Roles.FindAll(role => {
				return role.Privileges.Contains(Config.IsSelfSettable);
			});

			RoleList = String.Join<string>(", ", SelfSettableRoles.ConvertAll(role => role.Code).ToArray());

			// iam command
			api.ChatCommands.Create(Lang.Get("vividvoidscommander:iam_cmd_name"))
				.WithDescription($"{Lang.Get("vividvoidscommander:iam_description")}{RoleList}")
				.RequiresPlayer()
				.RequiresPrivilege(Config.CanRoleSwap)
				.WithArgs(api.ChatCommands.Parsers.Unparsed(Lang.Get("vividvoidscommander:iam_arg_name")))
				.HandleWith(IAmCommandHandler);

		}

	}
}
