namespace DragonLens.Core.Systems.ToolbarSystem.ExternalToolLayouts
{
	internal static class ErkySSCLayoutTools
	{
		private const string ModName = "ErkySSC";

		public static void AddTools(Toolbar toolbar)
		{
			ExternalToolLayoutHelper.TryAddTool(toolbar, ModName, "DLTeamAssignerTool");
			ExternalToolLayoutHelper.TryAddTool(toolbar, ModName, "SSCManagerTool");
		}
	}
}
