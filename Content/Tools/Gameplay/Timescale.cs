using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class Timescale : Tool
	{
		public override string IconKey => "Timescale";

		public override void OnActivate()
		{
			TimescaleWindow state = UILoader.GetUIState<TimescaleWindow>();
			state.visible = !state.visible;
		}

		public override void SaveData(TagCompound tag)
		{
			tag["timeScale"] = ModContent.GetInstance<TimeScaleSystem>().TimeScale;
		}

		public override void LoadData(TagCompound tag)
		{
			float timeScale = tag.ContainsKey("timeScale") ? tag.GetFloat("timeScale") : 1f;
			ModContent.GetInstance<TimeScaleSystem>().SetTimeScale(timeScale, true);
		}

		public override void SendPacket(BinaryWriter writer)
		{
			writer.Write(ModContent.GetInstance<TimeScaleSystem>().TimeScale);
		}

		public override void RecievePacket(BinaryReader reader, int sender)
		{
			float timeScale = reader.ReadSingle();

			if (Main.netMode == NetmodeID.Server && !SenderCanUseTimescale(sender))
			{
				NetSend(sender);
				return;
			}

			ModContent.GetInstance<TimeScaleSystem>().SetTimeScale(timeScale);

			if (Main.netMode == NetmodeID.Server)
				NetSend(-1, sender);
		}

		public static string GetText(string key, params object[] args)
		{
			return LocalizationHelper.GetText($"Tools.Timescale.{key}", args);
		}

		public static void CommitTimeScaleChange()
		{
			ToolHandler.NetSend<Timescale>();
			ToolHandler.SaveToolDataNow();
		}

		private static bool SenderCanUseTimescale(int sender)
		{
			if (sender < 0 || sender >= Main.maxPlayers)
				return false;

			Player player = Main.player[sender];
			return player?.active == true && PermissionHandler.CanUseTools(player);
		}
	}

	/// <summary>
	/// Fast-forward is implemented in DoUpdate by repeating updates. Slow-down is implemented by
	/// accumulating partial world/time updates and only running the original methods on whole ticks.
	/// </summary>
	[Autoload(Side = ModSide.Both)]
	internal sealed class TimeScaleSystem : ModSystem
	{
		public static readonly float[] SnapValues = [0f, 0.125f, 0.25f, 0.5f, 1f, 2f, 4f, 8f, 16f];

		/// <summary>
		/// Gets the current time scale factor applied to time-dependent operations.
		/// </summary>
		/// <remarks>A time scale of 1.0 represents normal speed. Values greater than 1.0 accelerate time, while
		/// values below 1.0 slow it down.</remarks>
		public float TimeScale { get; private set; } = 1f;
		public bool Paused => TimeScale == 0f;

		private float lastNonZeroTimeScale = 1f;
		private double worldUpdateAccumulator;
		private double timeUpdateAccumulator;
		private double extraUpdateAccumulator;
		private bool runningExtraUpdates;

		private bool stepOneFrameRequested;
		private bool consumedWorldStep;
		private bool consumedTimeStep;
		private bool ownsFramePause;

		public override void Load()
		{
			On_Main.DoUpdate += HookDoUpdate;
			On_Main.DoUpdateInWorld += HookDoUpdateInWorld;
			On_Main.UpdateTime += HookUpdateTime;
		}

		public override void Unload()
		{
			ReleaseFramePause();

			On_Main.DoUpdate -= HookDoUpdate;
			On_Main.DoUpdateInWorld -= HookDoUpdateInWorld;
			On_Main.UpdateTime -= HookUpdateTime;
		}

		public void SetTimeScale(float value, bool resetTransientState = false)
		{
			float snapped = SnapTimeScale(value);
			bool changed = Math.Abs(TimeScale - snapped) >= 0.0001f;

			TimeScale = snapped;

			if (TimeScale > 0f)
				lastNonZeroTimeScale = TimeScale;

			ApplyFramePauseState();

			if (changed || resetTransientState)
				ResetAccumulators();

			ClearStepRequest();
		}

		public void TogglePause()
		{
			if (Paused)
				SetTimeScale(lastNonZeroTimeScale);
			else
				SetTimeScale(0f);
		}

		public void StepOneFrame()
		{
			if (Main.gameMenu || !Paused)
				return;

			stepOneFrameRequested = true;
			consumedWorldStep = false;
			consumedTimeStep = false;

			if (Main.netMode == NetmodeID.SinglePlayer)
				Content.Tools.Developer.FrameAdvanceSystem.stepReady = true;
		}

		private void ApplyFramePauseState()
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
				return;

			if (Paused)
			{
				if (!Content.Tools.Developer.FrameAdvanceSystem.paused)
				{
					Content.Tools.Developer.FrameAdvanceSystem.paused = true;
					ownsFramePause = true;
				}

				return;
			}

			ReleaseFramePause();
		}

		private void ReleaseFramePause()
		{
			if (!ownsFramePause)
				return;

			Content.Tools.Developer.FrameAdvanceSystem.paused = false;
			Content.Tools.Developer.FrameAdvanceSystem.stepReady = false;
			ownsFramePause = false;
		}

		public static float SnapTimeScale(float rawValue)
		{
			float best = SnapValues[0];
			float bestDistance = Math.Abs(rawValue - best);

			for (int i = 1; i < SnapValues.Length; i++)
			{
				float value = SnapValues[i];
				float distance = Math.Abs(rawValue - value);

				if (distance < bestDistance)
				{
					best = value;
					bestDistance = distance;
				}
			}

			return best;
		}

		public static int GetIndexForTimeScale(float rawValue)
		{
			float snapped = SnapTimeScale(rawValue);

			for (int i = 0; i < SnapValues.Length; i++)
			{
				if (Math.Abs(SnapValues[i] - snapped) < 0.0001f)
					return i;
			}

			return 0;
		}

		public static float ProgressToTimeScale(float progress)
		{
			int index = (int)Math.Round(MathHelper.Clamp(progress, 0f, 1f) * (SnapValues.Length - 1), MidpointRounding.AwayFromZero);
			return SnapValues[index];
		}

		public static float TimeScaleToProgress(float timeScale)
		{
			return GetIndexForTimeScale(timeScale) / (float)(SnapValues.Length - 1);
		}

		public static string FormatTimeScale(float timeScale)
		{
			timeScale = SnapTimeScale(timeScale);
			return timeScale.ToString("0.###", CultureInfo.InvariantCulture) + "x";
		}

		public static Color GetScaleColor(float timeScale)
		{
			timeScale = SnapTimeScale(timeScale);

			if (timeScale <= 1f)
				return Color.Lerp(Color.Cyan, Color.LimeGreen, timeScale);

			if (timeScale <= 4f)
				return Color.Lerp(Color.LimeGreen, Color.Yellow, (timeScale - 1f) / 3f);

			return Color.Lerp(Color.Yellow, Color.Red, (timeScale - 4f) / 12f);
		}

		private void ResetAccumulators()
		{
			worldUpdateAccumulator = 0d;
			timeUpdateAccumulator = 0d;
			extraUpdateAccumulator = 0d;
		}

		private void ClearStepRequest()
		{
			stepOneFrameRequested = false;
			consumedWorldStep = false;
			consumedTimeStep = false;
		}

		private void TryCompleteStepRequest()
		{
			if (stepOneFrameRequested && consumedWorldStep && consumedTimeStep)
				ClearStepRequest();
		}

		private void HookDoUpdate(On_Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
		{
			// End of stream sometimes when loading a replay?
			orig(self, ref gameTime);

			if (runningExtraUpdates || Main.gameMenu || TimeScale <= 1f)
				return;

			double extraUpdatesToRun = TimeScale - 1d;
			extraUpdateAccumulator += extraUpdatesToRun;

			int extraWholeUpdates = (int)extraUpdateAccumulator;
			if (extraWholeUpdates <= 0)
				return;

			extraUpdateAccumulator -= extraWholeUpdates;
			runningExtraUpdates = true;

			try
			{
				for (int i = 0; i < extraWholeUpdates; i++)
				{
					orig(self, ref gameTime);

					if (Main.gameMenu || TimeScale <= 1f)
						break;
				}
			}
			finally
			{
				runningExtraUpdates = false;
			}
		}

		private void HookDoUpdateInWorld(On_Main.orig_DoUpdateInWorld orig, Main self, Stopwatch sw)
		{
			if (Main.gameMenu || TimeScale >= 1f)
			{
				orig(self, sw);
				NotifyWorldTickAdvanced();
				return;
			}

			if (stepOneFrameRequested)
			{
				orig(self, sw);
				NotifyWorldTickAdvanced();
				consumedWorldStep = true;
				TryCompleteStepRequest();
				return;
			}

			worldUpdateAccumulator += TimeScale;

			if (worldUpdateAccumulator < 1d)
				return;

			worldUpdateAccumulator -= 1d;
			orig(self, sw);
			NotifyWorldTickAdvanced();
		}

		private static void NotifyWorldTickAdvanced()
		{
			//if (Main.netMode != NetmodeID.Server && ReplaySession.IsReplayPlayback)
			//	ModContent.GetInstance<Replayer.Replayer>()?.AdvancePlaybackTick();
		}

		private void HookUpdateTime(On_Main.orig_UpdateTime orig)
		{
			if (Main.gameMenu || TimeScale >= 1f)
			{
				orig();
				return;
			}

			if (stepOneFrameRequested)
			{
				orig();
				consumedTimeStep = true;
				TryCompleteStepRequest();
				return;
			}

			timeUpdateAccumulator += TimeScale;

			if (timeUpdateAccumulator < 1d)
				return;

			timeUpdateAccumulator -= 1d;
			orig();
		}
	}

	internal class TimescaleWindow : DraggableUIState
	{
		public override Tool OwnerTool => ModContent.GetInstance<Timescale>();
		public TimescaleSlider slider;
		public TimescalePauseButton pause;
		public TimescaleSpeedButton[] speedButtons;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 460, 54);

		public override Vector2 DefaultPosition => new(0.5f, 0.5f);

		public override string HelpLink => "https://github.com/ScalarVector1/DragonLens/wiki/Timescale-tool";

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			width = 460;
			height = 190;

			slider = new TimescaleSlider();
			Append(slider);

			pause = new TimescalePauseButton();
			Append(pause);

			speedButtons = new TimescaleSpeedButton[TimeScaleSystem.SnapValues.Length];

			for (int k = 0; k < speedButtons.Length; k++)
			{
				speedButtons[k] = new TimescaleSpeedButton(k);
				Append(speedButtons[k]);
			}
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			slider.Left.Set(basePos.X + 25, 0);
			slider.Top.Set(basePos.Y + 70, 0);

			pause.Left.Set(basePos.X + 390, 0);
			pause.Top.Set(basePos.Y + 57, 0);

			float sliderWidth = slider.Width.Pixels;

			for (int k = 0; k < speedButtons.Length; k++)
			{
				float progress = k / (float)(speedButtons.Length - 1);
				speedButtons[k].Left.Set(basePos.X + 25 + progress * sliderWidth - 21, 0);
				speedButtons[k].Top.Set(basePos.Y + 136, 0);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, new Rectangle((int)basePos.X, (int)basePos.Y, 460, 190), ThemeHandler.BackgroundColor);

			Texture2D back = Assets.GUI.Gradient.Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 444, 40);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ThemeHandler.GetIcon("Timescale");
			spriteBatch.Draw(icon, basePos + Vector2.One * 12, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, Timescale.GetText("UITitle"), basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.45f);

			base.Draw(spriteBatch);
		}
	}

	internal class TimescaleSlider : SmartUIElement
	{
		public bool dragging;
		public float progress;

		public TimescaleSlider()
		{
			Width.Set(350, 0);
			Height.Set(16, 0);
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			TimeScaleSystem system = ModContent.GetInstance<TimeScaleSystem>();

			if (dragging)
			{
				progress = MathHelper.Clamp((Main.MouseScreen.X - GetDimensions().Position().X) / GetDimensions().Width, 0, 1);
				system.SetTimeScale(TimeScaleSystem.ProgressToTimeScale(progress));

				if (!Main.mouseLeft)
				{
					dragging = false;
					Timescale.CommitTimeScaleChange();
				}
			}
			else
			{
				progress = TimeScaleSystem.TimeScaleToProgress(system.TimeScale);
			}
		}

		public override void SafeMouseDown(UIMouseEvent evt)
		{
			dragging = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);

			Rectangle trackDims = dims;
			trackDims.Inflate(-4, -4);

			Texture2D tex = Assets.GUI.CloudScale.Value;
			spriteBatch.Draw(tex, trackDims, Color.White);

			for (int i = 0; i < TimeScaleSystem.SnapValues.Length; i++)
			{
				int tickX = trackDims.X + (int)(i / (float)(TimeScaleSystem.SnapValues.Length - 1) * trackDims.Width);
				var tickTarget = new Rectangle(tickX - 5, trackDims.Y - 6, 10, 20);
				GUIHelper.DrawBox(spriteBatch, tickTarget, TimeScaleSystem.GetScaleColor(TimeScaleSystem.SnapValues[i]));

				string label = i switch
				{
					0 => "0x",
					1 => "1/8x",
					2 => "1/4x",
					3 => "1/2x",
					4 => "1x",
					5 => "2x",
					6 => "4x",
					7 => "8x",
					_ => "16x"
				};

				Utils.DrawBorderString(spriteBatch, label, new Vector2(tickX, dims.Y + 20), Color.White, 0.65f, 0.5f);
			}

			var draggerTarget = new Rectangle(trackDims.X + (int)(progress * trackDims.Width) - 6, trackDims.Y - 8, 12, 24);
			GUIHelper.DrawBox(spriteBatch, draggerTarget, ThemeHandler.ButtonColor);

			TimeScaleSystem system = ModContent.GetInstance<TimeScaleSystem>();
			string speedString = Timescale.GetText("Speed", TimeScaleSystem.FormatTimeScale(system.TimeScale));

			Utils.DrawBorderString(spriteBatch, speedString, dims.TopLeft() + new Vector2(0, 40), Color.White, 0.8f);
		}
	}

	internal class TimescalePauseButton : SmartUIElement
	{
		public TimescalePauseButton()
		{
			Width.Set(42, 0);
			Height.Set(42, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);

			TimeScaleSystem system = ModContent.GetInstance<TimeScaleSystem>();

			if (system.Paused)
			{
				Texture2D glowTex = Assets.Misc.GlowAlpha.Value;
				Color glowColor = Color.White;
				glowColor.A = 0;
				spriteBatch.Draw(glowTex, dims, glowColor);
				GUIHelper.DrawOutline(spriteBatch, dims, ThemeHandler.ButtonColor.InvertColor());
			}

			Texture2D icon = Assets.GUI.Pause.Value;
			spriteBatch.Draw(icon, dims.TopLeft() + Vector2.One * 5, Color.White);

			if (IsMouseHovering && CanShowTooltip)
			{
				Tooltip.SetName(Timescale.GetText(system.Paused ? "Resume" : "Pause"));
				Tooltip.SetTooltip("");
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			ModContent.GetInstance<TimeScaleSystem>().TogglePause();
			Timescale.CommitTimeScaleChange();
		}
	}

	internal class TimescaleSpeedButton : SmartUIElement
	{
		private readonly int speedIndex;

		public TimescaleSpeedButton(int speedIndex)
		{
			Width.Set(42, 0);
			Height.Set(42, 0);

			this.speedIndex = speedIndex;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);

			float speed = TimeScaleSystem.SnapValues[speedIndex];
			TimeScaleSystem system = ModContent.GetInstance<TimeScaleSystem>();

			if (TimeScaleSystem.GetIndexForTimeScale(system.TimeScale) == speedIndex)
				GUIHelper.DrawOutline(spriteBatch, dims, ThemeHandler.ButtonColor.InvertColor());

			string label = TimeScaleSystem.FormatTimeScale(speed);
			Vector2 textSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(label);
			float scale = Math.Min(0.8f, Math.Min((dims.Width - 6) / textSize.X, (dims.Height - 8) / textSize.Y));
			Utils.DrawBorderString(spriteBatch, label, dims.Center.ToVector2() + new Vector2(0,2), Color.White, scale, 0.5f, 0.5f);

			if (IsMouseHovering && CanShowTooltip)
			{
				Tooltip.SetName(Timescale.GetText("SetSpeed", label));
				Tooltip.SetTooltip("");
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			ModContent.GetInstance<TimeScaleSystem>().SetTimeScale(TimeScaleSystem.SnapValues[speedIndex]);
			Timescale.CommitTimeScaleChange();
		}
	}
}
