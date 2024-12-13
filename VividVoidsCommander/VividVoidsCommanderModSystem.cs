using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VividVoidsCommander {

	public class ICommanderConfig {
		public string IsSelfSettable { get; set; }
		public string CanRoleSwap { get; set; }
	}

	public class IMessages {
		public string MissingArg { get; set; }
		public string IAmArgumentName { get; set; }
		public string IAmSuccess { get; set; }
		public string IAmDescription { get; set; }
		public string IAmCmdName { get; set; }

	}

	public class VividVoidsCommanderModSystem : ModSystem {

		static ICommanderConfig Config;

		static ICoreServerAPI sapi;

		static List<IPlayerRole> SelfSettableRoles;

		static string RoleList;

		static IMessages Messages;

		public override bool ShouldLoad(EnumAppSide forSide) {
			return forSide == EnumAppSide.Server;
		}

		public override void AssetsLoaded(ICoreAPI api) {
			Messages = new() {
				MissingArg = Lang.Get("vividvoidscommander:missingarg"),
				IAmArgumentName = Lang.Get("vividvoidscommander:iam_arg_name"),
				IAmSuccess = Lang.Get("vividvoidscommander:iam_success"),
				IAmDescription = Lang.Get("vividvoidscommander:iam_description"),
				IAmCmdName = Lang.Get("vividvoidscommander:iam_cmd_name")
			};
		}

		static TextCommandResult IAmCommandHandler(TextCommandCallingArgs args) {
			string arg = args.RawArgs.PopWord();
			IPlayerRole request = SelfSettableRoles.Find(r => r.Code.Equals(arg));

			if ( request == null ) {
				return TextCommandResult.Error($"{Messages.MissingArg}{Messages.IAmArgumentName}");
			}

			sapi.Permissions.SetRole((IServerPlayer)args.Caller.Player, request);

			return TextCommandResult.Success($"{Messages.IAmSuccess}{request.Code}");
		}
		public override void StartServerSide(ICoreServerAPI api) {

			sapi = api;

			Config = api.LoadModConfig<ICommanderConfig>($"{Mod.Info.ModID}.json") ?? new() {
				CanRoleSwap = "canroleswap",
				IsSelfSettable = "isselfsettable"
			};

			api.StoreModConfig(Config, $"{Mod.Info.ModID}.json");


			SelfSettableRoles = api.Server.Config.Roles.FindAll(role => {
				return role.Privileges.Contains(Config.IsSelfSettable);
			});

			RoleList = String.Join<string>(", ", SelfSettableRoles.ConvertAll(role => role.Code).ToArray());

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
