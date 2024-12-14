using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VividVoidsCommander {
	public class VividVoidsCommanderModSystem : ModSystem {

		private static readonly ICommanderConfig DefaultConfig = new() {
			CanRoleSwap = "canroleswap",
			IsSelfSettable = "isselfsettable"
		};

		static ICommanderConfig Config;

		static ICoreServerAPI sapi;

		static List<IPlayerRole> SelfSettableRoles;

		static string RoleList;

		static IMessages Messages;

		public override bool ShouldLoad(EnumAppSide forSide) {
			return forSide == EnumAppSide.Server;
		}

		public override void AssetsLoaded(ICoreAPI api) {
			Messages = new IMessages {
				MissingArg = Lang.Get("vividvoidscommander:missingarg"),
				IAmArgumentName = Lang.Get("vividvoidscommander:iam_arg_name"),
				IAmSuccess = Lang.Get("vividvoidscommander:iam_success"),
				IAmDescription = Lang.Get("vividvoidscommander:iam_description"),
				IAmCmdName = Lang.Get("vividvoidscommander:iam_cmd_name")
			};
		}

		private static TextCommandResult IAmCommandHandler(TextCommandCallingArgs args) {
			string arg = args.RawArgs.PopWord();
			IPlayerRole request = SelfSettableRoles.Find(r => r.Code.Equals(arg));

			if ( request == null ) {
				return TextCommandResult.Error($"{Messages.MissingArg}{Messages.IAmArgumentName}");
			}

			sapi.Permissions.SetRole((IServerPlayer)args.Caller.Player, request);

			return TextCommandResult.Success($"{Messages.IAmSuccess}{request.Code}");
		}

		public static TextCommandResult KitHandler(TextCommandCallingArgs args) {
			string arg = args.RawArgs.PopWord();

			return TextCommandResult.Success();
		}

		public override void StartServerSide(ICoreServerAPI api) {

			sapi = api;

			Config = api.LoadModConfig<ICommanderConfig>($"{Mod.Info.ModID}.json");

			if ( Config == null ) {
				Config = DefaultConfig;
				api.StoreModConfig(Config, $"{Mod.Info.ModID}.json");
			}

			SelfSettableRoles = api.Server.Config.Roles.FindAll(role => role.Privileges.Contains(Config.IsSelfSettable));

			RoleList = string.Join<string>(", ", SelfSettableRoles.ConvertAll(role => role.Code).ToArray());

			// iam command
			api.ChatCommands.Create(Messages.IAmCmdName)
				.WithDescription($"{Messages.IAmDescription}{RoleList}")
				.RequiresPlayer()
				.RequiresPrivilege(Config.CanRoleSwap)
				.WithArgs(api.ChatCommands.Parsers.Unparsed(Messages.IAmArgumentName))
				.HandleWith(IAmCommandHandler);

		}

	}
}
