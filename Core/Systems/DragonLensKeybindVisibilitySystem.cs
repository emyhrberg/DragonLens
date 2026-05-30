using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.GameContent.UI.States;

namespace DragonLens.Core.Systems;

[Autoload(Side = ModSide.Client)]
internal sealed class DragonLensKeybindVisibilitySystem : ModSystem
{
	private const string ModName = "DragonLens";

	private bool? lastVisible;

	public override void Load()
	{
		MethodInfo assembleMethod = GetAssembleBindPanelsMethod();
		MemberInfo keybindsMember = GetKeybindsMember();
		MethodInfo visibleGetter = typeof(DragonLensKeybindVisibilitySystem).GetMethod(nameof(GetVisibleKeybinds), BindingFlags.Static | BindingFlags.NonPublic);

		if (assembleMethod == null || keybindsMember == null || visibleGetter == null)
		{
			Mod.Logger.Warn($"Could not hook keybind visibility. assemble={assembleMethod != null}, keybinds={keybindsMember != null}, visible={visibleGetter != null}");
			return;
		}

		MonoModHooks.Modify(assembleMethod, il => FilterDragonLensKeybinds(il, keybindsMember, visibleGetter));
	}

	public override void Unload()
	{
		lastVisible = null;
		MonoModHooks.RemoveAll(Mod);
	}

	public override void PostUpdateEverything()
	{
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

	private static MethodInfo GetAssembleBindPanelsMethod()
	{
		BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		return typeof(UIManageControls).GetMethod("OnAssembleBindPanels", flags)
			?? typeof(UIManageControls).GetMethod("AssembleBindPanels", flags);
	}

	private static MemberInfo GetKeybindsMember()
	{
		BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		return (MemberInfo)typeof(KeybindLoader).GetProperty("Keybinds", flags)?.GetMethod ?? typeof(KeybindLoader).GetField("Keybinds", flags);
	}

	private static void FilterDragonLensKeybinds(ILContext il, MemberInfo keybindsMember, MethodInfo visibleGetter)
	{
		ILCursor c = new(il);

		bool found = keybindsMember switch
		{
			MethodInfo method => c.TryGotoNext(MoveType.Before, i => i.MatchCall(method)),
			FieldInfo field => c.TryGotoNext(MoveType.Before, i => i.MatchLdsfld(field)),
			_ => false
		};

		if (!found)
			throw new InvalidOperationException("Could not find KeybindLoader.Keybinds in UIManageControls.OnAssembleBindPanels.");

		c.Remove();
		c.Emit(OpCodes.Call, visibleGetter);
	}

	private static IEnumerable<ModKeybind> GetVisibleKeybinds()
	{
		IEnumerable<ModKeybind> keybinds = KeybindLoader.Keybinds;
		return ShouldShowDragonLensKeybinds() ? keybinds : keybinds.Where(keybind => keybind.Mod?.Name != ModName);
	}

	private static bool ShouldShowDragonLensKeybinds()
	{
		return Main.LocalPlayer is not null && PermissionHandler.LooksLikeAdmin(Main.LocalPlayer);
	}
}