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
using Netcode;

namespace RidgesideVillage
{
    //This section is heavily inspired by blueberry's Community Kitchen code, which can be found here:
    //https://github.com/b-b-blueberry/CustomCommunityCentre/blob/master/CommunityKitchen/Core/Kitchen.cs
    internal static class SummitHouse
    {

        static IModHelper Helper;
        static IMonitor Monitor;
        static Vector2 fridge_pos = new(19, 4);

        [XmlElement("fridge")]
        static readonly NetRef<Chest> fridge = new NetRef<Chest>(new Chest(playerChest: true));

        internal static void Initialize(IMod ModInstance)
        {

            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            TileActionHandler.RegisterTileAction("RSVKitchen", HandleKitchen);
            TileActionHandler.RegisterTileAction("RSVFridge", HandleFridge);

            Helper.Events.GameLoop.SaveLoaded += SetUpKitchen;
        }

        public static void HandleKitchen(string tileActionString, Vector2 tile_pos)
        {
            // Try opening a cooking menu
            // Minifridges are ignored as they are only usable with Esca's Modding Plugins
            if (fridge.Value != null && fridge.Value.mutex.IsLocked())
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
                return;
            }
            fridge?.Value.mutex.RequestLock(
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
                            fridge.Value.mutex.ReleaseLock();
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
                fridge.Value.mutex.ReleaseLock();
            }
        }

        private static void SetUpKitchen(object sender, SaveLoadedEventArgs e)
        {
            if (fridge.Value == null)
            {
                fridge.Value = new Chest(playerChest: true);
            }
        }

    }
}
