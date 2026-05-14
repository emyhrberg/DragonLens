using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.GameContent.UI.States;

namespace DragonLens.Core.Systems
{
	internal class DragonLensKeybindVisibilitySystem : ModSystem
	{
		private const string ModName = "DragonLens";

		private static readonly MethodInfo OnAssembleBindPanelsMethod = typeof(UIManageControls).GetMethod("OnAssembleBindPanels", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo KeybindsGetterMethod = typeof(KeybindLoader).GetProperty(nameof(KeybindLoader.Keybinds), BindingFlags.Static | BindingFlags.Public)?.GetMethod;
		private static readonly MethodInfo VisibleKeybindsGetterMethod = typeof(DragonLensKeybindVisibilitySystem).GetMethod(nameof(GetVisibleKeybinds), BindingFlags.Static | BindingFlags.NonPublic);

		private bool? lastVisible;

		public override void Load()
		{
			if (Main.dedServ)
				return;

			if (OnAssembleBindPanelsMethod is null || KeybindsGetterMethod is null || VisibleKeybindsGetterMethod is null)
			{
				Mod.Logger.Warn("Could not hook UIManageControls keybind visibility; DragonLens keybinds will remain visible in Controls.");
				return;
			}

			MonoModHooks.Modify(OnAssembleBindPanelsMethod, FilterDragonLensKeybinds);
		}

		public override void Unload()
		{
			lastVisible = null;

			if (!Main.dedServ)
				MonoModHooks.RemoveAll(Mod);
		}

		public override void PostUpdateEverything()
		{
			if (Main.dedServ)
				return;

			bool visible = ShouldShowDragonLensKeybinds();

			if (lastVisible is null)
			{
				lastVisible = visible;
				return;
			}

			if (lastVisible == visible)
				return;

			lastVisible = visible;

			if (Main.InGameUI?.CurrentState == Main.ManageControlsMenu)
				Main.ManageControlsMenu.OnActivate();
		}

		private static void FilterDragonLensKeybinds(ILContext il)
		{
			ILCursor cursor = new(il);

			if (!cursor.TryGotoNext(instruction => instruction.MatchCall(KeybindsGetterMethod)))
				throw new InvalidOperationException("Could not find KeybindLoader.Keybinds in UIManageControls.OnAssembleBindPanels.");

			cursor.Next.OpCode = OpCodes.Call;
			cursor.Next.Operand = VisibleKeybindsGetterMethod;
		}

		private static IEnumerable<ModKeybind> GetVisibleKeybinds()
		{
			IEnumerable<ModKeybind> keybinds = KeybindLoader.Keybinds;

			if (ShouldShowDragonLensKeybinds())
				return keybinds;

			return keybinds.Where(keybind => keybind.Mod?.Name != ModName);
		}

		private static bool ShouldShowDragonLensKeybinds()
		{
			return Main.LocalPlayer is not null && PermissionHandler.LooksLikeAdmin(Main.LocalPlayer);
		}
	}
}
