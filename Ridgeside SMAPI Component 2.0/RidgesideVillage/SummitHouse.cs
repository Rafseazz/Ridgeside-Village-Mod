using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using StardewValley;
using StardewValley.Objects;
using StardewModdingAPI.Events;
using StardewModdingAPI.Enums;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using StardewValley.Locations;
using StardewModdingAPI.Utilities;
using xTile.Tiles;

namespace RidgesideVillage
{
	//Stuff to make the Summit House behave like the player's farmhouse
	//The kitchen section is very heavily based on blueberry's Community Kitchen code, which can be found here:
	//https://github.com/b-b-blueberry/CustomCommunityCentre/blob/master/CommunityKitchen/Core/Kitchen.cs
	internal static class SummitHouse
	{
		static IModHelper Helper;
		static IMonitor Monitor;

		public static readonly Rectangle FridgeOpenedSpriteArea = new(32, 560, 16, 32);
		public static readonly Vector2 FridgeChestPosition = new(6830);
		public static readonly int[] FridgeTileIndexes = { 468, 500, 1122, 1154 };
		public static readonly int[] CookingTileIndexes = { 498, 499, 631, 632, 633 };
		public static Vector2 FridgeTilePosition = Vector2.Zero;

		// Esca's Modding Plugins integrations
		public static string EMPTileActionKitchen;
		public static string EMPMapPropertyMinifridges;

		internal static void Initialize(IMod ModInstance)
		{
			Helper = ModInstance.Helper;
			Monitor = ModInstance.Monitor;
			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
			Helper.Events.Display.MenuChanged += Display_MenuChanged;
			Helper.Events.Specialized.LoadStageChanged += Specialized_LoadStageChanged;
			Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

			// The Harmony patches are in SummitFarm bc it was already there
		}

		private static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs args)
        {
			GameLocation house = Game1.getLocationFromName(RSVConstants.L_SUMMITHOUSE);
			if (Game1.MasterPlayer.mailReceived.Contains(RSVConstants.M_HOUSEUPGRADED))
			{
				house.modData["renovated"] = "true";

			}
			try
            {
				if (house.modData["RSV.SummitHouseFurnished"] == "true")
					return;
			}
			catch
			{
				house.modData["RSV.SummitHouseFurnished"] = "false";
			}

			house.furniture.Add(new Furniture(2742, new Vector2(1f, 5f))); // blossom rug
			house.furniture.Add(new Furniture(1664, new Vector2(15f, 11f))); // mystic rug
			house.furniture.Add(new Furniture(1614, new Vector2(2f, 1f))); // basic window
			house.furniture.Add(new Furniture(1614, new Vector2(14f, 1f))); // basic window
			house.furniture.Add(new Furniture(1614, new Vector2(17f, 1f))); // basic window

			house.furniture.Add(new BedFurniture(BedFurniture.DOUBLE_BED_INDEX, new Vector2(1f, 4f))); // double bed
			house.furniture.Add(new Furniture(1395, new Vector2(4f, 4f))); // birch end table
			house.furniture.Add(new Furniture(1285, new Vector2(5f, 4f))); // luxury bookcase
			house.furniture.Add(new Furniture(1134, new Vector2(2f, 8f))); // pub table
			house.furniture.Add(new Furniture(1120, new Vector2(15f, 6f))); // oak table
			house.furniture.Add(new Furniture(1297, new Vector2(9f, 14f))); // topiary tree
			house.furniture.Add(new Furniture(94, new Vector2(1f, 9f))); // green stool
			house.furniture.Add(new Furniture(94, new Vector2(4f, 9f))); // green stool

			var chair1 = new Furniture(18, new Vector2(14f, 7f));
			chair1.currentRotation.Value = 1;
			chair1.updateRotation();
			var chair2 = new Furniture(18, new Vector2(17f, 7f));
			chair2.currentRotation.Value = 3;
			chair2.updateRotation();
			house.furniture.Add(chair1); // country chair
			house.furniture.Add(chair2); // country chair

			((DecoratableLocation)house).SetWallpaper("11", "Main");

			house.modData["RSV.SummitHouseFurnished"] = "true";
			house.ignoreLights.Value = true;
			Log.Trace($"RSV: Added {house.furniture.Count} pieces of furniture");
		}

		[EventPriority(EventPriority.Low)] // run after Content Patcher
		private static void Specialized_LoadStageChanged(object sender, LoadStageChangedEventArgs args)
		{
			if (args.NewStage is LoadStage.CreatedInitialLocations or LoadStage.SaveAddedLocations)
			{
				for (int i = 0; i < Game1.locations.Count; i++)
				{
					GameLocation location = Game1.locations[i];
					if ((location.Name == RSVConstants.L_SUMMITHOUSE || location.Name == RSVConstants.L_SUMMITSHED) && location is not DecoratableLocation)
					{
						Log.Trace($"RSV: Found {location.Name}");
						Game1.locations.RemoveAt(i);
						Game1.locations.Insert(i, new DecoratableLocation(location.mapPath.Value, location.Name));
						Log.Trace($"RSV: Made {location.Name} decoratable ({location.mapPath.Value})");
						
						break;
					}
				}
			}
		}

		private static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			LoadEMPValues();
		}

		private static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			// In-game interactions
			if (!Game1.game1.IsActive || Game1.currentLocation == null || !Context.IsWorldReady)
				return;

			// World interactions
			if (!Context.CanPlayerMove)
				return;

			if (e.Button.IsActionButton())
			{
				// Tile actions
				Tile tile = Game1.currentLocation.Map.GetLayer("Buildings")
					.Tiles[(int)e.Cursor.GrabTile.X, (int)e.Cursor.GrabTile.Y];
				if (tile != null)
				{
					if (Game1.currentLocation.Name == RSVConstants.L_SUMMITHOUSE)
					{
						// Use here as a cooking station
						if (CookingTileIndexes.Contains(tile.TileIndex))
						{
							Log.Trace("RSV: Opening Summit House cooking menu.");
							TryOpenCookingMenuFromKitchenTile(Game1.currentLocation, tilePosition: e.Cursor.GrabTile, button: e.Button);
							return;
						}
						// Open fridge door
						if (tile.TileIndex == FridgeTileIndexes[1])
						{
							// Open the fridge as a chest
							Log.Trace("RSV: Opening Summit House fridge menu.");
							TrySetFridgeDoor(Game1.currentLocation, isOpening: true, isUsingChest: true, button: e.Button);
							return;
						}
					}
				}
			}
		}

		private static void Display_MenuChanged(object sender, MenuChangedEventArgs e)
		{
			if (e.OldMenu is TitleMenu || e.NewMenu is TitleMenu || Game1.currentLocation == null || Game1.player == null)
				return;

			bool isCraftingMenu(IClickableMenu menu)
			{
				return menu is ItemGrabMenu
					|| menu is CraftingPage
					|| nameof(menu).EndsWith("CraftingPage", StringComparison.InvariantCultureIgnoreCase)
					|| nameof(menu).EndsWith("CookingMenu", StringComparison.InvariantCultureIgnoreCase);
			}

			// Close Community Centre fridge door after use in the renovated kitchen
			if (Game1.currentLocation.Name == RSVConstants.L_SUMMITHOUSE && isCraftingMenu(e.OldMenu) && !isCraftingMenu(e.NewMenu))
			{
				TrySetFridgeDoor(Game1.currentLocation, isOpening: false, isUsingChest: false);
				return;
			}
		}

		private static void LoadEMPValues()
		{
			// Load values from Esca's Modding Plugins
			if (Helper.ModRegistry.IsLoaded("Esca.EMP"))
			{
				Type type;

				type = HarmonyLib.AccessTools.TypeByName("EscasModdingPlugins.HarmonyPatch_ActionKitchen");
				EMPTileActionKitchen = (string)type
					?.GetProperty("ActionName", bindingAttr: System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
					?.GetValue(obj: null);

				type = HarmonyLib.AccessTools.TypeByName("EscasModdingPlugins.HarmonyPatch_AllowMiniFridges");
				EMPMapPropertyMinifridges = (string)type
					?.GetProperty("MapPropertyName", bindingAttr: System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
					?.GetValue(obj: null);

				if (EMPTileActionKitchen == null || EMPMapPropertyMinifridges == null)
				{
					Log.Error($"RSV: Failed to load values from Esca's Modding Plugins. Submit a bug report!");
				}
			}
		}

		public static bool IsKitchenLoaded(GameLocation here)
		{
			if (here == null)
				return false;
			return true;
		}

		internal static void SetUpKitchen(GameLocation here)
		{

			// Set tiles used for opening/closing fridge
			FridgeTilePosition = GetKitchenFridgeTilePosition(here);

			// Update kitchen fridge chest
			GetKitchenFridge(here);

			// Implement Esca's Modding Plugins features if available
			if (EMPMapPropertyMinifridges != null)
			{
				// Allow mini-fridges to be placed in the Community Centre
				here.Map.Properties[EMPMapPropertyMinifridges] = true;
			}
		}

		public static Chest GetKitchenFridge(GameLocation here)
		{
			// Update fridge chest object if null
			if (!here.Objects.TryGetValue(FridgeChestPosition, out StardewValley.Object o) || o is not Chest chest)
			{
				Log.Trace("RSV: No fridge found. Creating new chest to be Summit House fridge.");
				chest = new Chest(playerChest: true, tileLocation: FridgeChestPosition, parentSheetIndex: 216);
				chest.fridge.Value = true;
				chest.name = Helper.Translation.Get("RSV.Fridge");
				here.Objects[FridgeChestPosition] = chest;
			}
			return chest;
		}

		private static Vector2 GetKitchenFridgeTilePosition(GameLocation here)
		{
			int w = here.Map.GetLayer("Buildings").LayerWidth;
			int h = here.Map.GetLayer("Buildings").LayerHeight;
			for (int x = 0; x < w; ++x)
			{
				for (int y = 0; y < h; ++y)
				{
					if (here.Map.GetLayer("Buildings").Tiles[x, y] != null
						&& here.Map.GetLayer("Buildings").Tiles[x, y].TileIndex == FridgeTileIndexes[1])
					{
						return new Vector2(x, y);
					}
				}
			}
			return Vector2.Zero;
		}

		private static void TryOpenCookingMenuFromKitchenTile(GameLocation here, Vector2 tilePosition, SButton button)
		{
			Helper.Input.Suppress(button);
			TryOpenCookingMenu(here, tilePosition: tilePosition, fridge: GetKitchenFridge(here));
		}

		private static bool TrySetFridgeDoor(GameLocation here, bool isOpening, bool isUsingChest, SButton? button = null)
		{
			if (button != null)
			{
				Helper.Input.Suppress(button.Value);
			}

			Point tilePosition = Utility.Vector2ToPoint(FridgeTilePosition);
			(xTile.Dimensions.Location tileA, xTile.Dimensions.Location tileB) = (
				new(tilePosition.X, tilePosition.Y - 1),
				new(tilePosition.X, tilePosition.Y));
			if (FridgeTilePosition != Vector2.Zero)
			{
				// Set fridge tiles to default if fridge door is closing or if fridge chest is not in use
				// Set fridge tiles to alternate (open) if fridge door is open or fridge chest is in use
				int fridgeTiles = !isOpening
					|| (isUsingChest && !((Chest)here.Objects[FridgeChestPosition]).checkForAction(Game1.player))
					? 0
					: 2;
				if (!isOpening)
				{
					((Chest)here.Objects[FridgeChestPosition]).mutex.ReleaseLock();
				}
				/*
				// I don't know where the open fridge tiles are so nvm
				here.Map.GetLayer("Front").Tiles[tileA].TileIndex
					= FridgeTileIndexes[fridgeTiles];
				here.Map.GetLayer("Buildings").Tiles[tileB].TileIndex
					= FridgeTileIndexes[fridgeTiles + 1];
				*/
				return true;
			}
			return false;
		}

		// Taken from StardewValley.Locations.FarmHouse.cs:ActivateKitchen(NetRef<Chest> fridge)
		// Edited to remove netref, mini-fridge and multiple-mutex references
		public static void TryOpenCookingMenu(GameLocation here, Chest fridge, Vector2 tilePosition)
		{
			// Try opening the cooking menu via Esca's Modding Plugins
			xTile.Dimensions.Location location = new((int)tilePosition.X, (int)tilePosition.Y);
			if (EMPTileActionKitchen != null
				&& here.performAction(action: EMPTileActionKitchen, who: Game1.player, tileLocation: location))
				return;

			// Try opening a cooking menu using our own logic otherwise
			// Minifridges are ignored as they are only usable with Esca's Modding Plugins
			if (fridge != null && fridge.mutex.IsLocked())
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
				return;
			}
			fridge?.mutex.RequestLock(
				acquired: delegate
				{
					// Set fridge door visuals
					TrySetFridgeDoor(here, isOpening: true, isUsingChest: false);

					// Set new crafting page
					Point dimensions = new(
						x: 800 + IClickableMenu.borderWidth * 2,
						y: 600 + IClickableMenu.borderWidth * 2);
					Point position = Utility.Vector2ToPoint(Utility.getTopLeftPositionForCenteringOnScreen(
						width: dimensions.X,
						height: dimensions.Y));
					IClickableMenu menu = null;
					menu = new CraftingPage(
						x: position.X,
						y: position.Y,
						width: dimensions.X,
						height: dimensions.Y,
						cooking: true,
						standalone_menu: true,
						material_containers: new List<Chest> { fridge })
					{
						exitFunction = delegate
						{
							fridge.mutex.ReleaseLock();
						}
					};
					Game1.activeClickableMenu = menu;
				},
				failed: delegate
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
				});
		}
	}
}