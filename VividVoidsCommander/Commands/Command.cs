using Vintagestory.API.Server;

namespace VividVoidsCommander.Commands {
	internal abstract  class Command {
		internal abstract void Init(ICoreServerAPI api);
		internal abstract void Init(ICoreServerAPI api, CommanderConfig config);
	}
}