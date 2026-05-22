using DragonLens.Content.Themes.BoxProviders;
using DragonLens.Content.Themes.IconProviders;
using DragonLens.Content.Tools;
using DragonLens.Content.Tools.Despawners;
using DragonLens.Content.Tools.Developer;
using DragonLens.Content.Tools.Editors;
using DragonLens.Content.Tools.Gameplay;
using DragonLens.Content.Tools.Map;
using DragonLens.Content.Tools.Multiplayer;
using DragonLens.Content.Tools.Spawners;
using DragonLens.Content.Tools.Visualization;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Core.Systems.ToolbarSystem.ExternalToolLayouts;
using Terraria.ID;

namespace DragonLens.Core.Systems
{
	internal class FirstTimeSetupSystem : ModSystem
	{
		public static bool trueFirstTime;

		public static void SetupPresets()
		{
			ToolbarHandler.BuildPreset("Simple", n =>
			{
				n.Add(
					new Toolbar(new Vector2(0.3f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<ItemSpawner>()
					.AddTool<ProjectileSpawner>()
					.AddTool<NPCSpawner>()
					.AddTool<BuffSpawner>()
					);

				n.Add(
					new Toolbar(new Vector2(0.3f, 0.85f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<ItemDespawner>()
					.AddTool<ProjectileDespawner>()
					.AddTool<NPCDespawner>()
					.AddTool<GoreDespawner>()
					);

				n.Add(
					new Toolbar(new Vector2(0.5f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<Godmode>()
					.AddTool<InfiniteReach>()
					.AddTool<NoClip>()
					.AddTool<FastForward>()
					.AddTool<Time>()
					.AddTool<Weather>()
					.AddTool<Magnet>()
					.AddTool<CustomizeTool>()
					);

				n.Add(
					new Toolbar(new Vector2(0.7f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<Floodlight>()
					.AddTool<FreeCamera>()
					.AddTool<LockCamera>()
					.AddTool<PlayerManager>()
					);

				n.Add(
					new Toolbar(new Vector2(1f, 0.5f), Orientation.Vertical, AutomaticHideOption.NoMapScreen)
					.AddTool<RevealMap>()
					.AddTool<HideMap>()
					.AddTool<MapTeleport>()
					.AddTool<CustomizeTool>()
					);
			},
			ThemeHandler.GetBoxProvider<SimpleBoxes>(),
			ThemeHandler.GetIconProvider<DefaultIcons>());

			//advanced layout, default for mod devs
			ToolbarHandler.BuildPreset("Advanced", n =>
			{
				n.Add(
					new Toolbar(new Vector2(0f, 0.5f), Orientation.Vertical, AutomaticHideOption.Never)
					.AddTool<ItemSpawner>()
					.AddTool<ProjectileSpawner>()
					.AddTool<NPCSpawner>()
					.AddTool<BuffSpawner>()
					.AddTool<DustSpawner>()
					.AddTool<TileSpawner>()
					.AddTool<SoundSpawner>()
					.AddTool<AccessoryTray>()
					.AddTool<EntityEditorTool>()
					.AddTool<SystemEditorTool>()
					);

				n.Add(
					new Toolbar(new Vector2(1f, 0.5f), Orientation.Vertical, AutomaticHideOption.InventoryOpen)
					.AddTool<ItemDespawner>()
					.AddTool<ProjectileDespawner>()
					.AddTool<NPCDespawner>()
					.AddTool<BuffDespawner>()
					.AddTool<GoreDespawner>()
					.AddTool<DustDespawner>()
					);

				n.Add(
					new Toolbar(new Vector2(0.48f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<Godmode>()
					.AddTool<InfiniteReach>()
					.AddTool<NoClip>()
					.AddTool<FastForward>()
					.AddTool<Time>()
					.AddTool<Weather>()
					.AddTool<Difficulty>()
					.AddTool<SpawnTool>()
					.AddTool<ItemEditor>()
					.AddTool<PlayerEditorTool>()
					.AddTool<Paint>()
					.AddTool<Magnet>()
					.AddTool<CustomizeTool>()
					);

				n.Add(
					new Toolbar(new Vector2(0.75f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<Floodlight>()
					.AddTool<Hitboxes>()
					.AddTool<FreeCamera>()
					.AddTool<LockCamera>()
					.AddTool<PlayerManager>()
					);

				n.Add(
					new Toolbar(new Vector2(1f, 0.5f), Orientation.Vertical, AutomaticHideOption.NoMapScreen)
					.AddTool<RevealMap>()
					.AddTool<HideMap>()
					.AddTool<MapTeleport>()
					.AddTool<CustomizeTool>()
					);
			},
			ThemeHandler.GetBoxProvider<SimpleBoxes>(),
			ThemeHandler.GetIconProvider<DefaultIcons>());

			SetupErkysLayout();

			//Attempts to mock the HEROs mod UI as best as possible
			ToolbarHandler.BuildPreset("HEROS mod imitation", n =>
			{
				n.Add(
					new Toolbar(new Vector2(0.5f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<ItemSpawner>()
					.AddTool<InfiniteReach>()
					.AddTool<SpawnTool>()
					.AddTool<ItemDespawner>()
					.AddTool<Time>()
					.AddTool<Weather>()
					//TODO: Waypoints?
					.AddTool<NPCSpawner>()
					.AddTool<BuffSpawner>()
					.AddTool<Godmode>()
					.AddTool<ItemEditor>()
					.AddTool<CustomizeTool>()
					);

				n.Add(
					new Toolbar(new Vector2(0f, 0.9f), Orientation.Vertical, AutomaticHideOption.NoMapScreen)
					.AddTool<MapTeleport>()
					);
			},
			 ThemeHandler.GetBoxProvider<VanillaBoxes>(),
			 ThemeHandler.GetIconProvider<HEROsIcons>());

			//Attempts to mock the Cheatsheet mod UI as best as possible
			ToolbarHandler.BuildPreset("Cheatsheet imitation", n => n.Add(
						new Toolbar(new Vector2(0.5f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
						.AddTool<ItemSpawner>()
						.AddTool<NPCSpawner>()
						//TODO: Recipie browser?
						//TODO: Waypoints?
						.AddTool<ItemDespawner>()
						.AddTool<Paint>()
						.AddTool<AccessoryTray>()
						.AddTool<PlayerEditorTool>()
						.AddTool<Magnet>()
						.AddTool<NPCDespawner>()
						.AddTool<SpawnTool>()
						.AddTool<Floodlight>()
						.AddTool<CustomizeTool>()
						),
						ThemeHandler.GetBoxProvider<VanillaBoxes>(),
						ThemeHandler.GetIconProvider<DefaultIcons>());

			//A blank slate
			ToolbarHandler.BuildPreset("Empty", n => n.Add(
					new Toolbar(new Vector2(0.5f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<CustomizeTool>()),
					ThemeHandler.GetBoxProvider<SimpleBoxes>(),
					ThemeHandler.GetIconProvider<DefaultIcons>());

			ToolbarHandler.activeToolbars.Clear();
		}

		public static void SetupErkysLayout()
		{
			ToolbarHandler.BuildPreset("Erkys Layout", n =>
			{
				n.Add(
					new Toolbar(new Vector2(0.15f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<ItemSpawner>()
					.AddTool<NPCSpawner>()
					.AddTool<ItemDespawner>()
					.AddTool<NPCDespawner>()
					.AddTool<BuffDespawner>()
				);

				n.Add(
					new Toolbar(new Vector2(0.35f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<Godmode>()
					.AddTool<InfiniteReach>()
					.AddTool<NoClip>()
					.AddTool<Timescale>()
					.AddTool<Time>()
					.AddTool<Weather>()
					.AddTool<SpawnTool>()
				);

				n.Add(
					new Toolbar(new Vector2(0.63f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<ItemEditor>()
					.AddTool<PlayerEditorTool>()
					.AddTool<Paint>()
					.AddTool<Magnet>()
					.AddTool<Floodlight>()
					.AddTool<Hitboxes>()
					.AddTool<PlayerManager>()
					.AddTool<AssetManager>()
					.AddTool<CustomizeTool>()
				);

				if (ExternalToolLayoutHelper.TryBuildToolbar("ErkySSC", new(1f, 0.2f), Orientation.Vertical, AutomaticHideOption.Never, out Toolbar erkySSCToolbar,
					"DLTeamAssignerTool", "SSCManagerTool"))
				{
					n.Add(erkySSCToolbar);
				}

				string[] modReloaderTools = Main.netMode == NetmodeID.SinglePlayer
					? ["DragonLensReload", "DragonLensUIPanel", "DragonLensLogPanel", "DragonLensModsPanel"]
					: ["DragonLensReloadMP", "DragonLensUIPanel", "DragonLensLogPanel", "DragonLensModsPanel"];

				if (ExternalToolLayoutHelper.TryBuildToolbar("ModReloader", new(1f, 0.4f), Orientation.Vertical, AutomaticHideOption.Never, out Toolbar modReloaderToolbar, modReloaderTools))
					n.Add(modReloaderToolbar);

				if (ExternalToolLayoutHelper.TryBuildToolbar("PvPAdventure", new(0f, 0.48f), Orientation.Vertical, AutomaticHideOption.Never, out Toolbar pvpAdventureToolbar,
					"DLStartGameTool", "DLPauseTool", "DLEndGameTool", "DLPointsSetterTool", "DLOpenConfigTool"))
				{
					n.Add(pvpAdventureToolbar);
				}

				n.Add(
					new Toolbar(new Vector2(0f, 0.78f), Orientation.Vertical, AutomaticHideOption.NoMapScreen)
					.AddTool<RevealMap>()
					.AddTool<HideMap>()
					.AddTool<MapTeleport>()
					.AddTool<CustomizeTool>()
				);
			},
			ThemeHandler.GetBoxProvider<SimpleBoxes>(),
			ThemeHandler.GetIconProvider<DefaultIcons>());
		}

		public static void PanicToSetup()
		{
			trueFirstTime = true;
			ThemeHandler.SetBoxProvider<SimpleBoxes>();
			ThemeHandler.SetIconProvider<DefaultIcons>();
			ToolbarHandler.activeToolbars.Clear();
		}

		public override void PostUpdateEverything()
		{
			if (trueFirstTime)
			{
			}
		}
	}
}
