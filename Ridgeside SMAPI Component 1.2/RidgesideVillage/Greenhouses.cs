using StardewModdingAPI;
using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI.Events;
using System.Reflection;


namespace RidgesideVillage
{
    internal static class Greenhouses
    {
        static IModHelper Helper;
        static IMonitor Monitor;
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            TileActionHandler.RegisterTileAction("ShipmentRSV", ShipmentBin);

            //Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private static void ShipmentBin(string tileActionString = "")
        {
            MethodInfo method = typeof(Farm).GetMethod("shipItem");
            ItemGrabMenu itemGrabMenu = new ItemGrabMenu((List<Item>)null, true, false, new InventoryMenu.highlightThisItem(Utility.highlightShippableObjects), (ItemGrabMenu.behaviorOnItemSelect)Delegate.CreateDelegate(typeof(ItemGrabMenu.behaviorOnItemSelect), (object)Game1.getFarm(), method), "", (ItemGrabMenu.behaviorOnItemSelect)null, true, true, false, true, false, 0, (Item)null, -1, (object)null);
            itemGrabMenu.initializeUpperRightCloseButton();
            int num1 = 0;
            itemGrabMenu.setBackgroundTransparency((uint)num1 > 0U);
            int num2 = 1;
            itemGrabMenu.setDestroyItemOnClick((uint)num2 > 0U);
            itemGrabMenu.initializeShippingBin();
            Game1.activeClickableMenu = (IClickableMenu)itemGrabMenu;
        }

        
    }
  
}
