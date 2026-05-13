using DragonLens.Core.Systems.ToolSystem;
using System;
using System.Linq;
using Terraria.ModLoader;

namespace DragonLens.Core.Systems.ToolbarSystem.ExternalToolLayouts
{
	internal static class ExternalToolLayoutHelper
	{
		public static bool TryAddTool(Toolbar toolbar, string modName, string toolTypeName)
		{
			if (toolbar is null || !ModLoader.TryGetMod(modName, out Mod mod))
				return false;

			Type toolType = mod.Code?.GetTypes().FirstOrDefault(type =>
				typeof(Tool).IsAssignableFrom(type) &&
				(type.Name == toolTypeName || type.FullName == toolTypeName));

			if (toolType is null)
				return false;

			Tool tool = ModContent.GetContent<Tool>().FirstOrDefault(loadedTool => loadedTool.GetType() == toolType);

			if (tool is null)
				return false;

			toolbar.AddTool(tool);
			return true;
		}
	}
}
