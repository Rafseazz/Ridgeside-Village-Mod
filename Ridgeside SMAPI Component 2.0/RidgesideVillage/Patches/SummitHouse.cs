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
using HarmonyLib;
using SObject = StardewValley.Object;

namespace RidgesideVillage
{
	//Stuff to make the Summit House behave like the player's farmhouse
	//The kitchen section is very heavily inspired by blueberry's Community Kitchen code, which can be found here:
	//https://github.com/b-b-blueberry/CustomCommunityCentre/blob/master/CommunityKitchen/Core/Kitchen.cs
	internal static class SummitHouse
	{
		static IModHelper Helper;

		public static readonly Rectangle FridgeOpenedSpriteArea = new(32, 560, 16, 32);
		//public static readonly Vector2 FridgeChestPosition = new(6830);
        public static readonly Vector2 FridgeChestPosition = new(19, 4);
        public static readonly int[] FridgeTileIndexes = { 468, 500, 1122, 1154 };
		public static readonly int[] CookingTileIndexes = { 498, 499, 631, 632, 633 };
		public static Vector2 FridgeTilePosition = Vector2.Zero;


		internal static void ApplyPatch(Harmony harmony, IModHelper ModHelper)
		{
			Helper = ModHelper;

            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            // The Harmony patches are in SummitFarm bc it was already there
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                prefix: new HarmonyMethod(typeof(SummitHouse), nameof(placementAction_Prefix))
            );
        }

        private static bool placementAction_Prefix(SObject __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            if (__instance.QualifiedItemId == "(BC)216" && location.Name == RSVConstants.L_SUMMITHOUSE) {
				
				Vector2 placementTile = new Vector2(x/64, y/64);
                if (location.Objects.ContainsKey(placementTile))
				{
					return true;
				}
                Chest fridge = new Chest("216", placementTile, 217, 2)
                {
                    shakeTimer = 50
                };
                fridge.fridge.Value = true;
                location.objects.Add(placementTile, fridge);
                location.playSound("hammer");
                __result = true;
				return false;
			}
			return true;
            
        }

        private static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs args)
        {
			if(!Game1.player.IsMainPlayer)
			{
				return;
			}

			GameLocation house = Game1.getLocationFromName(RSVConstants.L_SUMMITHOUSE);

            GetKitchenFridge(house); //used to spawn fridge
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

			house.furniture.Add(new Furniture("2742", new Vector2(1f, 5f))); // blossom rug
			house.furniture.Add(new Furniture("1664", new Vector2(15f, 11f))); // mystic rug
			house.furniture.Add(new Furniture("1614", new Vector2(2f, 1f))); // basic window
			house.furniture.Add(new Furniture("1614", new Vector2(14f, 1f))); // basic window
			house.furniture.Add(new Furniture("1614", new Vector2(17f, 1f))); // basic window

			house.furniture.Add(new BedFurniture(BedFurniture.DOUBLE_BED_INDEX, new Vector2(1f, 4f))); // double bed
			house.furniture.Add(new Furniture("1395", new Vector2(4f, 4f))); // birch end table
			house.furniture.Add(new Furniture("1285", new Vector2(5f, 4f))); // luxury bookcase
			house.furniture.Add(new Furniture("1134", new Vector2(2f, 8f))); // pub table
            house.furniture.Add(new Furniture("1120", new Vector2(15f, 6f))); // oak table
            house.furniture.Add(new Furniture("1297", new Vector2(9f, 14f))); // topiary tree
            house.furniture.Add(new Furniture("94", new Vector2(1f, 9f))); // green stool
            house.furniture.Add(new Furniture("94", new Vector2(4f, 9f))); // green stool


            var chair1 = new Furniture("18", new Vector2(14f, 7f));
			chair1.currentRotation.Value = 1;
			chair1.updateRotation();
			var chair2 = new Furniture("18", new Vector2(17f, 7f));
			chair2.currentRotation.Value = 3;
			chair2.updateRotation();
			house.furniture.Add(chair1); // country chair
			house.furniture.Add(chair2); // country chair

			((DecoratableLocation)house).SetWallpaper("11", "Main");

			house.modData["RSV.SummitHouseFurnished"] = "true";
			house.ignoreLights.Value = true;
			Log.Trace($"RSV: Added {house.furniture.Count} pieces of furniture");
		}


		public static Chest GetKitchenFridge(GameLocation here)
		{
			// Update fridge chest object if null
			if (!here.Objects.TryGetValue(FridgeChestPosition, out StardewValley.Object o) || o is not Chest chest)
			{
				Log.Trace("RSV: No fridge found. Creating new chest to be Summit House fridge.");
				chest = new Chest(playerChest: true, tileLocation: FridgeChestPosition);
				chest.fridge.Value = true;
				chest.name = Helper.Translation.Get("RSV.Fridge");
				here.Objects[FridgeChestPosition] = chest;
			}
			return chest;
		}

	}
}