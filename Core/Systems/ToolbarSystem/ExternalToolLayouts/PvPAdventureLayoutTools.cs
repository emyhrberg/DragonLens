using Terraria.ModLoader;

namespace DragonLens.Core.Systems.ToolbarSystem.ExternalToolLayouts
{
	internal static class PvPAdventureLayoutTools
	{
		private const string ModName = "PvPAdventure";

		public static void AddTools(Toolbar toolbar)
		{
			if (!ModLoader.HasMod(ModName))
				return;

			ExternalToolLayoutHelper.TryAddTool(toolbar, ModName, "DLStartGameTool");
			ExternalToolLayoutHelper.TryAddTool(toolbar, ModName, "DLPauseTool");
			ExternalToolLayoutHelper.TryAddTool(toolbar, ModName, "DLEndGameTool");
			ExternalToolLayoutHelper.TryAddTool(toolbar, ModName, "DLPointsSetterTool");
			ExternalToolLayoutHelper.TryAddTool(toolbar, ModName, "DLOpenConfigTool");
		}
	}
}
