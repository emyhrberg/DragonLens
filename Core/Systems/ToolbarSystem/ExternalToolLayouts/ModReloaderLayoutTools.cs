using Terraria.ID;
using Terraria.ModLoader;

namespace DragonLens.Core.Systems.ToolbarSystem.ExternalToolLayouts
{
	internal static class ModReloaderLayoutTools
	{
		private const string ModName = "ModReloader";

		public static void AddTools(Toolbar toolbar)
		{
			if (!ModLoader.HasMod(ModName))
				return;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ExternalToolLayoutHelper.TryAddTool(toolbar, ModName, "DragonLensReloadMP");
			}
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				ExternalToolLayoutHelper.TryAddTool(toolbar, ModName, "DragonLensReload");
			}
			ExternalToolLayoutHelper.TryAddTool(toolbar, ModName, "DragonLensUIPanel");
			ExternalToolLayoutHelper.TryAddTool(toolbar, ModName, "DragonLensLogPanel");
			ExternalToolLayoutHelper.TryAddTool(toolbar, ModName, "DragonLensModsPanel");
		}
	}
}
