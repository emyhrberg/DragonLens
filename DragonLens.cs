global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using ReLogic.Content;
global using Terraria;
global using Terraria.ModLoader;
using DragonLens.Configs;
using DragonLens.Content.Tools;
using DragonLens.Content.Tools.Multiplayer;
using DragonLens.Content.Tools.Spawners;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ToolSystem;
using ReLogic.Content;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Terraria.GameContent;
using Terraria.ID;

namespace DragonLens
{
	public class DragonLens : Mod
	{
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			string type = reader.ReadString();

			if (type == "ToolPacket")
				ToolHandler.HandlePacket(reader, whoAmI);

			if (type == "AdminUpdate")
				PermissionHandler.HandlePacket(reader, whoAmI);

			if (type == "ToolDataRequest")
				PermissionHandler.SendToolData(whoAmI);

			if (type == "PlayerManager")
				PlayerManagerNetHandler.HandlePacket(reader);
		}


		public override object Call(params object[] args)
		{
			const string Tag = "AddAPITool";

			try
			{
				// ── Validate signature ──────────────────────────────────────────────
				if (args is not { Length: 4 } ||
					args[0] is not string tag ||
					!tag.Equals(Tag, StringComparison.Ordinal))
				{
					throw new ArgumentException(
						$"{Tag} Error: Expected (\"{Tag}\", string name, Asset<Texture2D> icon, Action click).");
				}

				// ── Safe‑cast with fall‑backs ───────────────────────────────────────
				string name = args[1] as string ?? "Unnamed";
				Asset<Texture2D> icon = args[2] as Asset<Texture2D> ?? TextureAssets.MagicPixel;
				Action click = args[3] as Action ?? (() => Logger.Error($"{Tag} Error: Click action was null."));

				// Log any fall‑backs that were actually used
				if (args[1] is null) Logger.Error($"{Tag} Error: Name was null – defaulted to \"{name}\".");
				if (args[2] is null) Logger.Error($"{Tag} Error: Icon was null – defaulted to TextureAssets.MagicPixel.");
				if (args[3] is null) Logger.Error($"{Tag} Error: Click action was null – defaulted to no‑op.");

				// Constructor registers itself; nothing more to do
				return new APITool(this, name, icon, click);
			}
			catch (Exception ex)
			{
				Logger.Error($"[DragonLens Call Error] {ex}");
				return null;
			}
		}


	}
}