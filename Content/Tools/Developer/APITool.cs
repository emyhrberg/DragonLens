using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

internal sealed class APITool : Tool
{
	private readonly Asset<Texture2D> _icon;
	private readonly Action _click;
	private readonly string _key;

	public APITool(Mod parent, string key, Asset<Texture2D> icon, Action onClick)
	{
		_key = key;
		_icon = icon;
		_click = onClick;

		// These few commands register the tool.
		// It essentially does exactly what Tool.Register() does
		ThemeHandler.RegisterAPIIcon(_key, _icon);
		keybind = KeybindLoader.RegisterKeybind(parent, key, Keys.None);
		Language.GetOrRegister($"Mods.{parent.Name}.Tools.{key}.DisplayName", () => key);

		// make DragonLens aware of this tool
		ToolHandler.AddTool(this);
		ModTypeLookup<Tool>.Register(this);   // keeps reflection helpers happy
	}

	// ── required overrides for Tool ───────────────────────────────────────────────
	public override string IconKey => _key;
	public override string LocalizationKey => _key;
	public override void OnActivate() => _click?.Invoke();
	public override bool HasRightClick => false;   // left‑click only

	public override void DrawIcon(SpriteBatch sb, Rectangle target)
	{
		Texture2D tex = _icon.Value;
		float scl = Math.Min((float)target.Width / tex.Width,
							 (float)target.Height / tex.Height);

		sb.Draw(tex, target.Center.ToVector2(), null, Color.White,
				0f, tex.Size() * 0.5f, scl, SpriteEffects.None, 0f);
	}
}
