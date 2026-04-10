using DragonLens.Core.Loaders.UILoading;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria.ID;

namespace DragonLens.Content.Tools.Multiplayer
{
	/// <summary>
	/// Handles updating player states in-game.
	/// Communicates with its UI counterpart: <see cref="PlayerManager"/>
	/// </summary>
	internal class PlayerManagerSystem : ModSystem
	{
		public Player stalkedPlayer;
		public HashSet<int> frozenPlayers = [];
		public override void PreUpdatePlayers()
		{
			foreach (Player player in Main.ActivePlayers)
			{
				if (!frozenPlayers.Contains(player.whoAmI))
					continue;

				player.AddBuff(BuffID.Frozen, 2);
				player.velocity = Vector2.Zero;
				player.position = player.oldPosition;
				player.AddImmuneTime(ImmunityCooldownID.General, 2);
			}
		}

		public override void ModifyScreenPosition()
		{
			if (stalkedPlayer != null)
				Main.screenPosition = stalkedPlayer.Center - new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
		}
	}
}