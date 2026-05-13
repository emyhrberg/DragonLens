using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class LayoutPresetBrowser : Browser
	{
		public override string Name => LocalizationHelper.GetGUIText("LayoutPresetBrowser.Name");

		public override Vector2 DefaultPosition => new(0.6f, 0.5f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text")) + 1;
		}

		public override void PopulateGrid(UIGrid grid)
		{
			grid.Add(new LayoutPresetButton(this, "Simple", Path.Join(Main.SavePath, "DragonLensLayouts", "Simple")));
			grid.Add(new LayoutPresetButton(this, "Advanced", Path.Join(Main.SavePath, "DragonLensLayouts", "Advanced")));
			grid.Add(new LayoutPresetButton(this, "HEROsMod", Path.Join(Main.SavePath, "DragonLensLayouts", "HEROs mod imitation")));
			grid.Add(new LayoutPresetButton(this, "Cheatsheet", Path.Join(Main.SavePath, "DragonLensLayouts", "Cheatsheet imitation")));
			grid.Add(new LayoutPresetButton(this, "Empty", Path.Join(Main.SavePath, "DragonLensLayouts", "Empty")));
			grid.Add(new LayoutPresetButton(this, "ErkysLayout", Path.Join(Main.SavePath, "DragonLensLayouts", "Erkys Layout"), 3757));
		}

		public override void SetupSorts()
		{
			SortModes.Add(new("Alphabetical", (a, b) => a.Identifier.CompareTo(b.Identifier)));

			SortFunction = SortModes.First().Function;
		}

		public override void PostInitialize()
		{
			listMode = true;
		}
	}

	internal class LayoutPresetButton : BrowserButton
	{
		private readonly string name;
		private readonly string tooltip;
		private readonly string presetPath;
		private readonly int iconItemId;

		public override string Identifier => name;
		public override string Key => name;

		public LayoutPresetButton(Browser parent, string name, string presetPath, string tooltip, int iconItemId = 0) : base(parent)
		{
			this.name = name;
			this.presetPath = presetPath;
			this.tooltip = tooltip;
			this.iconItemId = iconItemId;
		}

		public LayoutPresetButton(Browser parent, string localizationKey, string presetPath, int iconItemId = 0) : this(parent, LocalizationHelper.GetGUIText($"Layout.{localizationKey}.Name"), presetPath, LocalizationHelper.GetGUIText($"Layout.{localizationKey}.Tooltip"), iconItemId)
		{
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			ToolbarHandler.LoadFromFile(presetPath);
			UILoader.GetUIState<ToolbarState>().Refresh();

			Main.NewText(LocalizationHelper.GetGUIText("LayoutPresetBrowser.LoadedLayout", name));
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconArea)
		{
			if (iconItemId > 0)
			{
				Texture2D texture = TextureAssets.Item[iconItemId].Value;
				float scale = MathHelper.Min(1f, MathHelper.Min((iconArea.Width - 8) / (float)texture.Width, (iconArea.Height - 8) / (float)texture.Height));

				spriteBatch.Draw(texture, iconArea.Center.ToVector2(), null, Color.White, 0f, texture.Size() / 2f, scale, SpriteEffects.None, 0f);
			}

			if (IsMouseHovering && CanShowTooltip)
			{
				Tooltip.SetName(Identifier);
				Tooltip.SetTooltip(tooltip);
			}
		}
	}
}
