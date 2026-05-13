using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Terraria.ID;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class FastForward : Tool
	{
		private const int NormalSpeedIndex = 4;
		private const int MaxSpeedIndex = 8;

		public override string IconKey => "FastForward";

		public override bool HasRightClick => true;

		public override void OnActivate()
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				Main.NewText(LocalizationHelper.GetToolText("FastForward.MultiplayerDisabled"), Color.Red);
				return;
			}

			int currentIndex = GetCurrentScaleIndex();
			int nextIndex = currentIndex < NormalSpeedIndex || currentIndex >= MaxSpeedIndex ? NormalSpeedIndex : currentIndex + 1;

			SetScaleIndex(nextIndex);
		}

		public override void OnRightClick()
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				Main.NewText(LocalizationHelper.GetToolText("FastForward.MultiplayerDisabled"), Color.Red);
				return;
			}

			int currentIndex = GetCurrentScaleIndex();
			int nextIndex = currentIndex <= NormalSpeedIndex ? MaxSpeedIndex : currentIndex - 1;

			SetScaleIndex(nextIndex);
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Rectangle position)
		{
			base.DrawIcon(spriteBatch, position);

			int currentIndex = GetCurrentScaleIndex();

			if (currentIndex > NormalSpeedIndex)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle(position.X - 4, position.Y - 4, 46, 46), ThemeHandler.ButtonColor.InvertColor());

				Texture2D tex = Assets.Misc.GlowAlpha.Value;
				float intensity = (currentIndex - NormalSpeedIndex) / (float)(MaxSpeedIndex - NormalSpeedIndex);
				Color color = new Color(150, 255, 170) * intensity;
				color.A = 0;
				var target = new Rectangle(position.X, position.Y, 38, 38);

				spriteBatch.Draw(tex, target, color);
			}
		}

		private static int GetCurrentScaleIndex()
		{
			TimeScaleSystem system = ModContent.GetInstance<TimeScaleSystem>();
			return TimeScaleSystem.GetIndexForTimeScale(system.TimeScale);
		}

		private static void SetScaleIndex(int index)
		{
			if (index < NormalSpeedIndex)
				index = NormalSpeedIndex;

			if (index > MaxSpeedIndex)
				index = MaxSpeedIndex;

			ModContent.GetInstance<TimeScaleSystem>().SetTimeScale(TimeScaleSystem.SnapValues[index]);
			ToolHandler.SaveToolDataNow();
		}
	}
}
