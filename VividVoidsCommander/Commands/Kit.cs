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

		internal override void Init(ICoreServerAPI api, CommanderConfig config) {
			_sapi = api;
			_config = config;

	 		_paramMissing = Lang.Get("vividvoidscommander:param_missing");
	 		_paramName = Lang.Get("vividvoidscommander:kit_name");
	 		_notFound = Lang.Get("vividvoidscommander:kit_not_found");

	 		_createSuccess = Lang.Get("vividvoidscommander:kit_creation_success");
	 		_deleteSuccess = Lang.Get("vividvoidscommander:kit_deletion_success");
			
			api.ChatCommands.Create(Lang.Get("vividvoidscommander:kit_command"))
			.WithDescription(Lang.Get("vividvoidscommander:kit_description"))
			.RequiresPlayer()
			.RequiresPrivilege(Privilege.chat)
			.BeginSubCommand(Lang.Get("vividvoidscommander:kit_subcommand_create"))
				 .RequiresPlayer()
				 .RequiresPrivilege(Privilege.root)
				 .WithArgs(api.ChatCommands.Parsers.Word(Lang.Get("vividvoidscommander:kit_name")))
				 .HandleWith(this.Create)
			.EndSubCommand()

			.BeginSubCommand(Lang.Get("vividvoidscommander:kit_subcommand_delete"))
				 .RequiresPlayer()
				 .RequiresPrivilege(Privilege.root)
				 .WithArgs(api.ChatCommands.Parsers.Word(Lang.Get("vividvoidscommander:kit_name")))
				 .HandleWith(this.Delete)
			.EndSubCommand()

			.BeginSubCommand(Lang.Get("vividvoidscommander:kit_subcommand_use"))
				 .RequiresPlayer()
				 .RequiresPrivilege(Privilege.chat)
				 .WithArgs(api.ChatCommands.Parsers.Word(Lang.Get("vividvoidscommander:kit_name")))
				 .HandleWith(this.Use)
			.EndSubCommand();

		}

		private TextCommandResult Create(TextCommandCallingArgs args) {
			string param = (string)args?.Parsers?[0].GetValue();

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

			string param = (string)args?.Parsers?[0].GetValue();
			
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
			string param = (string)args?.Parsers?[0].GetValue();

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