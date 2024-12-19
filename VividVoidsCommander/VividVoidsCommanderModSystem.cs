using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

// ReSharper disable InconsistentNaming

namespace VividVoidsCommander {
	public class VividVoidsCommanderModSystem : ModSystem {

		private static ICoreServerAPI sapi;
		

		private static readonly CommanderConfig DefaultConfig = new() {
			CanRelocate = "canrelocate",
			Relocatable = "relocatable",
			CanUseKits = "canusekits",
			Kits = new List<Kit>()
		};

		private static CommanderConfig Config;
		private static Messages Messages;
		private static List<IPlayerRole> RelocateRoles;
		private static string[] RelocateRoleList;

		public override bool ShouldLoad(EnumAppSide forSide) {
			return forSide == EnumAppSide.Server;
		}
		
		private static TextCommandResult RelocateCommandHandler(TextCommandCallingArgs args) {
			string arg = args[0] as string;
			string missingArgument = $"{Messages.MissingArgument}{Messages.RelocateParamName}";
			
			// It really shouldn't ever be missing but just in case of unforeseen edge case.
			if ( string.IsNullOrEmpty(arg) ) {
				return TextCommandResult.Error(missingArgument);
			}

			// Ensure a valid role parameter was passed and that it has a declared DefaultSpawn.
			IPlayerRole role = RelocateRoles.Find(role => role.Code.Equals(arg));
			if ( role?.DefaultSpawn == null ) {
				return TextCommandResult.Error(Messages.RelocateBadSpawn);
			}

			// Update the player's role.
			IServerPlayer player = (IServerPlayer)args.Caller.Player;
			sapi.Permissions.SetRole(player, role);


			// Ensure the given spawn has a valid Y coordinate. Falls back to the given locations surface.
			PlayerSpawnPos loc = role.DefaultSpawn;
			loc.y ??= sapi.World.BlockAccessor.GetRainMapHeightAt(loc.x, loc.z) + 1;

			// Teleport the player to their new spawn point.
			player.Entity.TeleportTo(loc.x, (int)loc.y, loc.z);
			
			return TextCommandResult.Success($"{Messages.RelocateSuccess}{role.Code}");
		}


		private static JsonItemStack ItemStackToJsonItemStack(ItemStack stack) {
			return new JsonItemStack {
				Code = stack.Collectible.Code,
				StackSize = stack.StackSize,
				Type = stack.Class,
				Attributes = stack.ItemAttributes
			};
		}

		private TextCommandResult KitCreationHandler(TextCommandCallingArgs args) {

			IInventory hotbar = args.Caller.Player.InventoryManager.GetHotbarInventory();

			List<JsonItemStack> validItemStacks = hotbar.ToList()
				.ConvertAll(slot => slot.Itemstack)
				.FindAll(itemStack => itemStack?.Collectible != null)
				.ConvertAll(ItemStackToJsonItemStack);
			
			Config.Kits.Add(new Kit {
				Name = (string)args[1],
				Items = validItemStacks,
				Uses = 1
			});

			sapi.StoreModConfig(Config, $"{Mod.Info.ModID}.json");

			return TextCommandResult.Success($"{validItemStacks}");

		}

		private TextCommandResult KitDeleteHandler(TextCommandCallingArgs args) {
			string kitName = args[0] as string;
			Kit target = Config.Kits.Find(kit => kit.Name == kitName);
			if ( target == null ) {
				return TextCommandResult.Error($"{Messages.KitNotFound}{kitName}");
			}
			Config.Kits.Remove(target);
			sapi.StoreModConfig(Config, $"{Mod.Info.ModID}.json");
			return TextCommandResult.Success(Messages.KitDeletionSuccess);
		}


		private static TextCommandResult KitUseHandler(TextCommandCallingArgs args) {
			string kitName = args[0] as string;
			Kit requestedKit = Config.Kits.Find(kit => kit.Name == kitName);
			
			if ( requestedKit == null ) {
				return TextCommandResult.Error($"{Messages.KitNotFound}{kitName}");
			}
			
			IServerPlayer player = (IServerPlayer)args.Caller.Player;

			int i = player.GetModData($"kits_used_{requestedKit.Name}", 0);
			if ( i >= requestedKit.Uses ) {
				return TextCommandResult.Error(Messages.KitUsesMaxReached);
			}
			
			foreach ( JsonItemStack jsonItemStack in requestedKit.Items ) {
				args.Caller.Player.InventoryManager.TryGiveItemstack(jsonItemStack.ResolvedItemstack, true);
			}

			player.SetModData($"kits_used_{requestedKit.Name}", i + 1);
			return TextCommandResult.Success(Messages.KitSuccess);
		}
		
		private TextCommandResult KitHandler(TextCommandCallingArgs args) {
			string arg = (string)args[0];
			return arg switch {
				null => TextCommandResult.Error($"{Messages.MissingArgument}{Messages.RelocateParamName}"),
				"create" => KitCreationHandler(args),
				"delete" => KitDeleteHandler(args),
				_ => KitUseHandler(args)
			};
		}
		
		public override void AssetsFinalize(ICoreAPI api) {
			sapi = (ICoreServerAPI)api;
			Config = sapi.LoadModConfig<CommanderConfig>($"{Mod.Info.ModID}.json");
			// Config missing, store the default one.
			if ( Config == null ) {
				Config = DefaultConfig;
				sapi.StoreModConfig(Config, $"{Mod.Info.ModID}.json");
			}
			// Command specific variables
			RelocateRoles = sapi.Server.Config.Roles.FindAll(role => role.Privileges.Contains(Config.Relocatable));
			RelocateRoleList = RelocateRoles.ConvertAll(role => role.Code).ToArray();
			
			Messages = new Messages {
				MissingArgument = Lang.Get("vividvoidscommander:missing_argument"),
				RelocateCmdName = Lang.Get("vividvoidscommander:relocate_cmd_name"),
				RelocateParamName = Lang.Get("vividvoidscommander:relocate_param_name"),
				RelocateDescription = Lang.Get("vividvoidscommander:relocate_description"),
				RelocateSuccess = Lang.Get("vividvoidscommander:relocate_success"),
				RelocateBadSpawn = Lang.Get("vividvoidscommander:relocate_bad_spawn"),
				RelocateInvalidOption = Lang.Get("vividvoidscommander:relocate_invalid_option"),
				
				KitCmdName = Lang.Get("vividvoidscommander:kit_cmd_name"),
				KitParamName = Lang.Get("vividvoidscommander:kit_param_name"),
				KitDescription = Lang.Get("vividvoidscommander:kit_description"),
				KitSuccess = Lang.Get("vividvoidscommander:kit_success"),
				KitNotFound = Lang.Get("vividvoidscommander:kit_not_found"),
				KitUsesMaxReached = Lang.Get("vividvoidscommander:kit_uses_max_reached"),
				KitMalformed = Lang.Get("vividvoidscommander:kit_malformed"),
				KitDeletionSuccess = Lang.Get("vividvoidscommander:kit_deletion_success"),
			};
		}
		
		public override void StartServerSide(ICoreServerAPI api) {
			// iam command
			sapi.ChatCommands.Create(Messages.RelocateCmdName)
				.WithDescription($"{Messages.RelocateDescription}{string.Join(", ", RelocateRoleList)}")
				.RequiresPlayer()
				.RequiresPrivilege(Config.CanRelocate)
				.WithArgs(sapi.ChatCommands.Parsers.Word(Messages.RelocateParamName))
				.HandleWith(RelocateCommandHandler);

			sapi.ChatCommands.Create(Messages.KitCmdName)
				.WithDescription(Messages.KitDescription)
				.RequiresPlayer()
				.RequiresPrivilege(Config.CanUseKits)
				.WithArgs(sapi.ChatCommands.Parsers.Word(Messages.KitParamName), sapi.ChatCommands.Parsers.Word(Messages.KitParamName))
				.HandleWith(KitHandler);
			sapi.Logger.Debug(Messages.RelocateCmdName);
		}

	}
}
