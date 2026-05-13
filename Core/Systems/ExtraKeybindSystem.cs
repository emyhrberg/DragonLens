using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolbarSystem;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using Terraria.GameInput;

namespace DragonLens.Core.Systems
{
	internal class ExtraKeybindSystem : ModPlayer
	{
		public static ModKeybind collapseAll;

		public override void Load()
		{
			collapseAll = KeybindLoader.RegisterKeybind(Mod, "CollapseAll", Keys.None);
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
#if DEBUG
			if (Main.keyState.IsKeyDown(Keys.F5) && Main.oldKeyState.IsKeyUp(Keys.F5))
			{
				string currentPath = System.IO.Path.Join(Main.SavePath, "DragonLensLayouts", "Current");
				ToolbarHandler.ExportToFile(currentPath);
				FirstTimeSetupSystem.SetupPresets();
				ToolbarHandler.LoadFromFile(currentPath);
				Main.NewText("Preset refreshed with F5");
			}
#endif
			if (collapseAll.JustPressed)
			{
				if (ToolbarHandler.activeToolbars.Any(n => !n.collapsed))
				{
					foreach (Toolbar bar in ToolbarHandler.activeToolbars)
					{
						bar.collapsed = true;
						UILoader.GetUIState<Content.GUI.ToolbarState>().UpdateCollapse();
					}
				}
				else
				{
					foreach (Toolbar bar in ToolbarHandler.activeToolbars)
					{
						bar.collapsed = false;
						UILoader.GetUIState<Content.GUI.ToolbarState>().UpdateCollapse();
					}
				}
			}
		}
	}
}
