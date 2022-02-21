using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using StardewModdingAPI.Utilities;
using xTile.Tiles;

namespace RidgesideVillage
{
    internal static class SummitHouse
    {

        static IModHelper Helper;
        static IMonitor Monitor;
        static Vector2 fridge_pos = new(19, 4);

        // Got an NRE at this line
        static Chest fridge = new(playerChest: true, tileLocation: fridge_pos, parentSheetIndex: 216);
        internal static void Initialize(IMod ModInstance)
        {

            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
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
                string property = GetSingleProperty(e);
                if (property != null)
                {
                    if ((Game1.currentLocation.Name is "Custom_Ridgeside_RSVSummitHouse") && (property == "RSVKitchen"))
                    {
                        Helper.Input.Suppress(e.Button);
                        UseKitchen(tilePosition: fridge_pos, fridge: fridge);
                        return;
                    }

                    // Open Community Centre fridge door
                    if (property == "RSVFridge")
                    {
                        // Open the fridge as a chest
                        OpenFridge(fridge: fridge, isOpening: true, isUsingChest: true, button: e.Button);

                        return;
                    }
                }
            }
        }

        public static void UseKitchen(Chest fridge, Vector2 tilePosition)
        {
            // Try opening a cooking menu
            // Minifridges are ignored as they are only usable with Esca's Modding Plugins
            if (fridge != null && fridge.mutex.IsLocked())
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
                return;
            }
            fridge?.mutex.RequestLock(
                acquired: delegate
                {
                    // Fridge door visuals
                    //TrySetFridgeDoor(isOpening: true, isUsingChest: false);

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

        private static bool OpenFridge(Chest fridge, bool isOpening, bool isUsingChest, SButton? button = null)
        {
            if (button != null)
            {
                Helper.Input.Suppress(button.Value);
            }

            Point tilePosition = Utility.Vector2ToPoint(fridge_pos);
            (xTile.Dimensions.Location tileA, xTile.Dimensions.Location tileB) = (
                new(tilePosition.X, tilePosition.Y - 1),
                new(tilePosition.X, tilePosition.Y));
            if (fridge_pos != Vector2.Zero)
            {
                if (!isOpening)
                {

                    fridge.mutex.ReleaseLock();
                }
                return true;
            }
            return false;
        }

        public static string GetSingleProperty(ButtonPressedEventArgs e)
        {
            string[] split = Game1.currentLocation.doesTileHaveProperty((int)e.Cursor.GrabTile.X, (int)e.Cursor.GrabTile.Y, "Action", "Buildings").Split(' ');
            if (split[0].Equals("RSVKitchen"))
            {
                return "RSVKitchen";
            }
            if (split[0].Equals("RSVFridge"))
            {
                return "RSVFridge";
            }
            return null;
        }

    }
}
