using DragonLens.Core.Systems.ToolSystem;
using System;
using Terraria.ID;
using Terraria.UI;

namespace DragonLens.Content.Tools.Developer
{
	public class Leave : Tool
	{
		public override string IconKey => "NoIconForThisDebugTool";

		public override string DisplayName => "Exit world";

		public override string Description => "Instantly leave to main menu without saving";

		public override void OnActivate()
		{
			WorldGen.JustQuit();
		}

		public override void DrawIcon(SpriteBatch sb, Rectangle position)
		{
			//base.DrawIcon(sb, position);

			// Draw wooden door icon
			var item = ContentSamples.ItemsByType[ItemID.WoodenDoor];
			var pos = position.Center.ToVector2();

			ItemSlot.DrawItemIcon(item, ItemSlot.Context.InventoryItem, sb, pos, 1f, Math.Min(position.Width, position.Height), Color.White);
		}
	}
}
