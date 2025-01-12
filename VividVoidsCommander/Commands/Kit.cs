using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VividVoidsCommander.Commands {
	internal class Kit : Command {
		private ICoreServerAPI _sapi;
		private CommanderConfig _config;

		private string _paramMissing;
		private string _paramName;
		private string _notFound;
		
		private string _createSuccess;
		private string _deleteSuccess;
		private string _baseCommand;
		private string _baseDescription;
		private string _subcommandCreate;
		private string _subcommandDelete;
		private string _subcommandUse;
		private string _paramNameKit;

		internal override void Init(ICoreServerAPI api, CommanderConfig config) {
			_sapi = api;
			_config = config;

	 		_paramMissing = Lang.Get("vividvoidscommander:param_missing");
	 		_paramName = Lang.Get("vividvoidscommander:kit_name");
	 		_notFound = Lang.Get("vividvoidscommander:kit_not_found");

	 		_createSuccess = Lang.Get("vividvoidscommander:kit_creation_success");
	 		_deleteSuccess = Lang.Get("vividvoidscommander:kit_deletion_success");

		  _baseCommand = Lang.Get("vividvoidscommander:kit_command");
		  _baseDescription = Lang.Get("vividvoidscommander:kit_description");

		  _subcommandCreate = Lang.Get("vividvoidscommander:kit_subcommand_create");
		  _subcommandDelete = Lang.Get("vividvoidscommander:kit_subcommand_delete");
		  _subcommandUse = Lang.Get("vividvoidscommander:kit_subcommand_use");
		  _paramNameKit = Lang.Get("vividvoidscommander:kit_name");

		  WordArgParser kitNameParser = api.ChatCommands.Parsers.Word(_paramNameKit);
		  
			api.ChatCommands.Create(_baseCommand)
			.WithDescription(_baseDescription)
			.RequiresPlayer()
			.RequiresPrivilege(Privilege.chat)
			.BeginSubCommand(_subcommandCreate)
				 .RequiresPlayer()
				 .RequiresPrivilege(Privilege.root)
				 .WithArgs(kitNameParser)
				 .HandleWith(this.Create)
			.EndSubCommand()

			.BeginSubCommand(_subcommandDelete)
				 .RequiresPlayer()
				 .RequiresPrivilege(Privilege.root)
				 .WithArgs(kitNameParser)
				 .HandleWith(this.Delete)
			.EndSubCommand()

			.BeginSubCommand(_subcommandUse)
				 .RequiresPlayer()
				 .RequiresPrivilege(Privilege.chat)
				 .WithArgs(kitNameParser)
				 .HandleWith(this.Use)
			.EndSubCommand();

		}

		private TextCommandResult Create(TextCommandCallingArgs args) {
			string param = (string)args?.Parsers?[0]?.GetValue();

			if ( param == null ) {
				return TextCommandResult.Error($"{_paramMissing}{_paramName}");
			}

			IInventory hotbar = args.Caller.Player.InventoryManager.GetHotbarInventory();
			
			_config.Kits.Add(new VividVoidsCommander.Kit {
				Name = param,
				Items = hotbar.ToList()
					 .ConvertAll(slot => slot.Itemstack)
					 .FindAll(itemStack => itemStack?.Collectible != null)
					 .ConvertAll(stack => new JsonItemStack {
							Code = stack.Collectible.Code,
							StackSize = stack.StackSize,
							Type = stack.Class,
							Attributes = stack.ItemAttributes
						}),
				Uses = 1
			});

			_sapi.StoreModConfig(_config, _config.Path);

			return TextCommandResult.Success($"{_createSuccess} {param}");
		}

		private TextCommandResult Delete(TextCommandCallingArgs args) {

			string param = (string)args?.Parsers?[0]?.GetValue();
			
			if ( param == null ) {
				return TextCommandResult.Error($"{_paramMissing}{_paramName}");
			}
			
			VividVoidsCommander.Kit target = _config.Kits.Find(kit => kit.Name == param);
			if ( target == null ) {
				return TextCommandResult.Error($"{_notFound}{param}");
			}
			
			_config.Kits.Remove(target);
			_sapi.StoreModConfig(_config, _config.Path);
			return TextCommandResult.Success($"{_deleteSuccess}{param}");
		}

		private TextCommandResult Use(TextCommandCallingArgs args) {
			string param = (string)args?.Parsers?[0]?.GetValue();

			if ( param == null ) {
				return TextCommandResult.Error($"{_paramMissing}{_paramName}");
			}
			
			VividVoidsCommander.Kit requestedKit = _config.Kits.Find(kit => kit.Name == param);
			
			if ( requestedKit == null ) {
				return TextCommandResult.Error($"{_notFound}{param}");
			}
			
			IServerPlayer player = (IServerPlayer)args.Caller.Player;

			int i = player.GetModData($"kits_used_{requestedKit.Name}", 0);
			if ( i >= requestedKit.Uses ) {
				return TextCommandResult.Error(Lang.Get("vividvoidscommander:kit_uses_max_reached"));
			}

			requestedKit.Items.ForEach(item => {
				item.Resolve(_sapi.World, "what");
				args.Caller.Player.InventoryManager.TryGiveItemstack(item.ResolvedItemstack, true);
			});

			player.SetModData($"kits_used_{requestedKit.Name}", i + 1);
			return TextCommandResult.Success($"{Lang.Get("vividvoidscommander:kit_success")}{param}");
		}

	}
}