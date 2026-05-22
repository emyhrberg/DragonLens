using DragonLens.Core.Systems.ToolSystem;
using System;
using System.Linq;

namespace DragonLens.Core.Systems.ToolbarSystem.ExternalToolLayouts
{
	internal static class ExternalToolLayoutHelper
	{
		public static bool TryBuildToolbar(string modName, Vector2 position, Orientation orientation, AutomaticHideOption hideOption, out Toolbar toolbar, params string[] toolTypeNames)
		{
			toolbar = null;

			if (!ModLoader.TryGetMod(modName, out Mod mod) || mod.Code is null)
				return false;

			Toolbar newToolbar = new(position, orientation, hideOption);
			bool addedAny = false;

			foreach (string toolTypeName in toolTypeNames)
			{
				try
				{
					addedAny |= TryAddTool(newToolbar, mod, toolTypeName);
				}
				catch (Exception e)
				{
					DragonLens.instance.Logger.Debug($"Skipping external toolbar tool '{modName}/{toolTypeName}': {e.Message}");
				}
			}

			if (!addedAny)
				return false;

			toolbar = newToolbar;
			return true;
		}

		private static bool TryAddTool(Toolbar toolbar, Mod mod, string toolTypeName)
		{
			if (!TryFindTool(mod, toolTypeName, out Tool tool))
				return false;

			toolbar.AddTool(tool);
			return true;
		}

		private static bool TryFindTool(Mod mod, string toolTypeName, out Tool tool)
		{
			if (mod.TryFind(toolTypeName, out tool))
				return true;

			string contentName = GetContentName(toolTypeName);

			if (contentName != toolTypeName && mod.TryFind(contentName, out tool))
				return true;

			tool = ModContent.GetContent<Tool>().FirstOrDefault(loadedTool =>
				loadedTool.Mod == mod &&
				(loadedTool.Name == toolTypeName ||
				loadedTool.FullName == toolTypeName ||
				loadedTool.GetType().Name == toolTypeName ||
				loadedTool.GetType().FullName == toolTypeName));

			return tool is not null;
		}

		private static string GetContentName(string typeName)
		{
			int start = typeName.LastIndexOfAny(new[] { '.', '+', '/' }) + 1;
			return typeName[start..];
		}
	}
}
