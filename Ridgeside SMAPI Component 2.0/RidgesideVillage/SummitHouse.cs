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

            TileActionHandler.RegisterTileAction("RSVKitchen", HandleKitchen);
            TileActionHandler.RegisterTileAction("RSVFridge", HandleFridge);
        }

        public static void HandleKitchen(string tileActionString, Vector2 tile_pos)
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

        private static void HandleFridge(string tileActionString, Vector2 position)
        {
            if (position == fridge_pos)
            {
                fridge.mutex.ReleaseLock();
            }
        }

    }
}
