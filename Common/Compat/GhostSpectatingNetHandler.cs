using DragonLens.Core.Systems;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DragonLens.Common.Compat;

public static class GhostSpectatingNetHandler
{
	public const string ToggleSpectatorPacket = "ToggleSpectator";

	public static void SendToggleSpectator(int targetWhoAmI)
	{
		if (targetWhoAmI is < 0 or >= Main.maxPlayers)
			return;

		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			if (targetWhoAmI == Main.myPlayer)
				GhostSpectatingCompat.TryToggleMode(targetWhoAmI, out _);

			return;
		}

		ModPacket packet = ModLoader.GetMod("DragonLens").GetPacket();
		packet.Write(ToggleSpectatorPacket);
		packet.Write(targetWhoAmI);
		packet.Send();
	}

	public static void ReceiveToggleSpectator(BinaryReader reader, int sender)
	{
		if (Main.netMode != NetmodeID.Server || sender is < 0 or >= Main.maxPlayers)
			return;

		Player senderPlayer = Main.player[sender];

		if (senderPlayer?.active != true || !PermissionHandler.CanUseTools(senderPlayer))
			return;

		int targetWhoAmI = reader.ReadInt32();

		if (targetWhoAmI is < 0 or >= Main.maxPlayers || Main.player[targetWhoAmI]?.active != true)
			return;

		GhostSpectatingCompat.TryToggleMode(targetWhoAmI, out _);
	}
}