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
using Netcode;

namespace RidgesideVillage
{
    internal static class SummitHouse
    {

        static IModHelper Helper;
        static IMonitor Monitor;

        [XmlElement("fridge")]
        public static readonly NetRef<Chest> fridge = new NetRef<Chest>(new Chest(playerChest: true));
        internal static void Initialize(IMod ModInstance)
        {

            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            TileActionHandler.RegisterTileAction("RSVFridge", HandleFridge);
            TileActionHandler.RegisterTileAction("RSVKitchen", HandleKitchen);
        }

        private static void HandleKitchen(string tileActionString, Vector2 position)
        {
            /*
            List<NetMutex> muticies = new();
            List<Chest> mini_fridges = new List<Chest>();
            foreach (StardewValley.Object item in GameLocation.objects.Values.Get())
            {
                if (item != null && (bool)item.bigCraftable && item is Chest && item.ParentSheetIndex == 216)
                {
                    mini_fridges.Add(item as Chest);
                    muticies.Add((item as Chest).mutex);
                }
            }
            if (fridge != null && fridge.Value.mutex.IsLocked())
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
                return;
            }
            MultipleMutexRequest multiple_mutex_request = null;
            multiple_mutex_request = new MultipleMutexRequest(muticies, delegate
            {
                fridge.Value.mutex.RequestLock(delegate
                {
                    List<Chest> list = new List<Chest>();
                    if (fridge != null)
                    {
                        list.Add(fridge);
                    }
                    list.AddRange(mini_fridges);
                    Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);
                    Game1.activeClickableMenu = new CraftingPage((int)topLeftPositionForCenteringOnScreen.X, (int)topLeftPositionForCenteringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, cooking: true, standalone_menu: true, list);
                    Game1.activeClickableMenu.exitFunction = delegate
                    {
                        fridge.Value.mutex.ReleaseLock();
                        multiple_mutex_request.ReleaseLocks();
                    };
                }, delegate
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
                    multiple_mutex_request.ReleaseLocks();
                });
            }, delegate
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
            });
            */
        }

        private static void HandleFridge(string tileActionString, Vector2 position)
        {
            /*
            Chest chest = fridge.ElementAt(0);
            Game1.activeClickableMenu = new ItemGrabMenu(chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, chest.grabItemFromInventory, null, chest.grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, chest, -1, chest);
            */
            }

    }

  
}
