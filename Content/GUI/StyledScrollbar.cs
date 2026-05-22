using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Renderers;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class StyledScrollbar : Terraria.ModLoader.UI.Elements.FixedUIScrollbar
	{
		public float oldValue;
		public int scrolledRecently;
		public static MethodInfo handleMethod = typeof(UIScrollbar).GetMethod("GetHandleRectangle", BindingFlags.NonPublic | BindingFlags.Instance);
		public static FieldInfo isDraggingField = typeof(UIScrollbar).GetField("_isDragging", BindingFlags.NonPublic | BindingFlags.Instance);
		public static FieldInfo dragYOffsetField = typeof(UIScrollbar).GetField("_dragYOffset", BindingFlags.NonPublic | BindingFlags.Instance);

		public StyledScrollbar(UserInterface userInterface) : base(userInterface) { }

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			float value = GetValue();

			if (value != oldValue)
			{
				oldValue = value;
				scrolledRecently = 2;
			}

			if (scrolledRecently > 0)
			{
				Parent?.Recalculate();
				scrolledRecently--;
			}
		}

		public override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (userInterface == null || !CanScroll)
				return;

			UpdateDragging();

			Rectangle back = GetDimensions().ToRectangle();
			back.Inflate(2, 2);
			GUIHelper.DrawBox(spriteBatch, back, ThemeHandler.BackgroundColor);

			Rectangle handle = (Rectangle)handleMethod.Invoke(this, null);
			handle.Width = (int)GetDimensions().Width - 4;
			handle.Offset(2, 0);

			GUIHelper.DrawBox(spriteBatch, handle, ThemeHandler.ButtonColor);
		}

		public override void LeftMouseDown(UIMouseEvent evt)
		{
			base.LeftMouseDown(evt);

			if (evt.Target != this || !CanScroll)
				return;

			Rectangle handle = (Rectangle)handleMethod.Invoke(this, null);
			isDraggingField.SetValue(this, true);
			dragYOffsetField.SetValue(this, evt.MousePosition.Y - handle.Y);
		}

		private void UpdateDragging()
		{
			if (!(bool)isDraggingField.GetValue(this))
				return;

			CalculatedStyle innerDimensions = GetInnerDimensions();
			float dragYOffset = (float)dragYOffsetField.GetValue(this);
			float handlePosition = userInterface.MousePosition.Y - innerDimensions.Y - dragYOffset;

			ViewPosition = MathHelper.Clamp(handlePosition / innerDimensions.Height * MaxViewSize, 0, MaxViewSize - ViewSize);
		}
	}
}
