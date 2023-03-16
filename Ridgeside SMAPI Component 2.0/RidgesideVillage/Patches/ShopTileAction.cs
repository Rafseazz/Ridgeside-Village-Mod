using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace RidgesideVillage
{
    internal class ShopTileAction
    {

        static IModHelper Helper;


        //yoinked from atravitas Interaction Tweaks available here:
        //https://github.com/atravita-mods/StardewMods/blob/main/StopRugRemoval/StopRugRemoval/HarmonyPatches/Niceties/StoreDelay.cs

        private const int TICKS_TO_DELAY = 20;
        private static readonly PerScreen<int> OpenedTick = new(() => 0);

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            TileActionHandler.RegisterTileAction("RSVShop", OpenShop);

            harmony.Patch(
                original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.setUpStoreForContext)),
                postfix: new HarmonyMethod(typeof(ShopTileAction), nameof(setUpStoreForContext_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.receiveLeftClick)),
                prefix: new HarmonyMethod(typeof(ShopTileAction), nameof(safetyTimer_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.receiveRightClick)),
                prefix: new HarmonyMethod(typeof(ShopTileAction), nameof(safetyTimer_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.receiveKeyPress)),
                prefix: new HarmonyMethod(typeof(ShopTileAction), nameof(safetyTimer_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.receiveGamePadButton)),
                prefix: new HarmonyMethod(typeof(ShopTileAction), nameof(safetyTimer_prefix))
            );


        }

        private static void setUpStoreForContext_Postfix()
        {
            OpenedTick.Value = Game1.ticks;
        }

        private static bool safetyTimer_prefix()
        {
            return OpenedTick.Value + TICKS_TO_DELAY < Game1.ticks;
        }

        private static void OpenShop(string tileActionString, Vector2 position)
        {
            var split = tileActionString.Split(' ');
            if (split.Length > 1)
            {
                ExternalAPIs.STFAPI.OpenItemShop(split[1]);
            }
        }

        
    }
}

