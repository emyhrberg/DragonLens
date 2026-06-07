using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DragonLens.Common.Compat;

internal static class GhostSpectatingCompat
{
	private static bool resolved;
	private static Type spectateModeType;
	private static MethodInfo getModeMethod;
	private static MethodInfo setModeLocalMethod;
	private static MethodInfo setModeServerMethod;

	public static bool IsAvailable() => Resolve();

	public static bool IsSpectator(int whoAmI)
	{
		if (!Resolve() || whoAmI < 0 || whoAmI >= Main.maxPlayers)
			return false;

		object mode = getModeMethod.Invoke(null, [whoAmI]);
		return mode != null && Convert.ToInt32(mode) == 1;
	}

	public static bool TrySetMode(int whoAmI, bool spectator)
	{
		if (!Resolve() || whoAmI < 0 || whoAmI >= Main.maxPlayers)
			return false;

		object mode = Enum.ToObject(spectateModeType, spectator ? 1 : 0);

		if (Main.netMode == NetmodeID.Server)
			setModeServerMethod.Invoke(null, [whoAmI, mode]);
		else
			setModeLocalMethod.Invoke(null, [whoAmI, mode]);

		return true;
	}

	public static bool TryToggleMode(int whoAmI, out bool spectator)
	{
		spectator = !IsSpectator(whoAmI);
		return TrySetMode(whoAmI, spectator);
	}

	private static bool Resolve()
	{
		if (resolved)
			return IsResolved();

		resolved = true;

		return TryResolve("Reese", "Reese.Common.Spectator") ||
			TryResolve("Reese", "Reese.Common.SpectatorMode") ||
			TryResolve("GhostSpectating", "GhostSpectating.Common.SpectatorMode");
	}

	private static bool IsResolved()
	{
		return spectateModeType != null && getModeMethod != null && setModeLocalMethod != null && setModeServerMethod != null;
	}

	private static bool TryResolve(string modName, string namespaceName)
	{
		if (!ModLoader.TryGetMod(modName, out Mod mod))
			return false;

		Type spectatorSystemType = mod.Code.GetType($"{namespaceName}.SpectatorModeSystem");
		Type modeType = mod.Code.GetType($"{namespaceName}.SpectateMode");
		MethodInfo getMode = spectatorSystemType?.GetMethod("GetMode", BindingFlags.Public | BindingFlags.Static);
		MethodInfo setModeLocal = spectatorSystemType?.GetMethod("SetModeLocal", BindingFlags.Public | BindingFlags.Static);
		MethodInfo setModeServer = spectatorSystemType?.GetMethod("SetModeServer", BindingFlags.Public | BindingFlags.Static);

		if (modeType == null || getMode == null || setModeLocal == null || setModeServer == null)
			return false;

		spectateModeType = modeType;
		getModeMethod = getMode;
		setModeLocalMethod = setModeLocal;
		setModeServerMethod = setModeServer;
		return true;
	}
}
