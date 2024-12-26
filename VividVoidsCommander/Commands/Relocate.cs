using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VividVoidsCommander.Commands {
	
	internal class Relocate : Command {

		private const int DefaultSpawnYOffset = 1;

		private ICoreServerAPI _sapi;
		private CommanderConfig _config;

		private string _paramMissing;
		private string _paramName;
		private static List<IPlayerRole> _relocateRoles;
		private static string[] _relocateRoleList;


		internal override void Init(ICoreServerAPI api, CommanderConfig config) {
			_sapi = api;
			_config = config;
			_paramMissing = Lang.Get("vividvoidscommander:param_missing");
			_paramName = Lang.Get("vividvoidscommander:relocate_name");
			_relocateRoles = api.Server.Config.Roles.FindAll(role => role.Privileges.Contains(_config.Relocatable));
			_relocateRoleList = _relocateRoles.ConvertAll(role => role.Code).ToArray();
			
			api.ChatCommands.Create(Lang.Get("vividvoidscommander:relocate_command"))
				 .WithDescription($"{Lang.Get("vividvoidscommander:relocate_description")}{string.Join(", ", _relocateRoleList)}")
				 .RequiresPlayer()
				 .RequiresPrivilege(config.CanRelocate)
				 .WithArgs(api.ChatCommands.Parsers.Word(Lang.Get("vividvoidscommander:relocate_name")))
				 .HandleWith(RelocateCommandHandler);
		}

		private TextCommandResult RelocateCommandHandler(TextCommandCallingArgs args) {
			string param = (string)args?.Parsers?[0].GetValue();

			if ( string.IsNullOrEmpty(param) ) {
				return TextCommandResult.Error($"{_paramMissing}{_paramName}");
			}

			// Ensure a valid role parameter was passed and that it has a declared DefaultSpawn.
			IPlayerRole role = _relocateRoles.Find(role => role.Code.Equals(param));
			if ( role?.DefaultSpawn == null ) {
				return TextCommandResult.Error(Lang.Get("vividvoidscommander:relocate_bad_spawn"));
			}

			// Update the player's role.
			IServerPlayer player = (IServerPlayer)args.Caller.Player;
			_sapi.Permissions.SetRole(player, role);


			// Ensure the given spawn has a valid Y coordinate. Falls back to the given locations surface.
			PlayerSpawnPos loc = role.DefaultSpawn;
			loc.y ??= _sapi.World.BlockAccessor.GetRainMapHeightAt(loc.x, loc.z) + DefaultSpawnYOffset;

			// Teleport the player to their new spawn point.
			player.Entity.TeleportTo(loc.x, (int)loc.y, loc.z);
			return TextCommandResult.Success($"{Lang.Get("vividvoidscommander:relocate_success")}{role.Code}");
		}

	}
}