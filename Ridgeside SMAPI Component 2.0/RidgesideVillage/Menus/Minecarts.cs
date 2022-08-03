using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace RidgesideVillage
{
    internal static class Minecarts
    {
        static IModHelper Helper;
        static IMonitor Monitor;
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            TileActionHandler.RegisterTileAction("RSVMinecart", OpenMinecartDialogue);
        }
        
        internal static void OpenMinecartDialogue(string tileActionString, Vector2 position)
        {
            if (tileActionString.Contains("Repair"))
            {
                //actually dropbox, not a warp
                return;
            }
            var choices = new List<Response>();
            var selectionActions = new List<Action>();
            if (!tileActionString.Contains("1"))
            {
                choices.Add(new Response("loc1", Helper.Translation.Get("MinecartLocation.1")));
                selectionActions.Add(delegate
                {
                    Game1.playSound("stairsdown");
                    Game1.warpFarmer("Custom_Ridgeside_RSVCableCar", 25, 18, false);
                });
            }
            if (!tileActionString.Contains("2"))
            {
                choices.Add(new Response("loc2", Helper.Translation.Get("MinecartLocation.2")));
                selectionActions.Add(delegate
                {
                    Game1.playSound("stairsdown");
                    Game1.warpFarmer("Custom_Ridgeside_RidgesideVillage", 69, 16, false);
                });
            }
            if (!tileActionString.Contains("3"))
            {
                choices.Add(new Response("loc3", Helper.Translation.Get("MinecartLocation.3")));
                selectionActions.Add(delegate
                {
                    Game1.playSound("stairsdown");
                    Game1.warpFarmer("Custom_Ridgeside_RidgesideVillage", 16, 82, false);
                });
            }
            if (!tileActionString.Contains("4"))
            {
                choices.Add(new Response("loc4", Helper.Translation.Get("MinecartLocation.4")));
                selectionActions.Add(delegate
                {
                    Game1.playSound("stairsdown");
                    Game1.warpFarmer("Custom_Ridgeside_RidgesideVillage", 109, 85, false);
                });
            }
            if (!tileActionString.Contains("5") && Game1.player.eventsSeen.Contains(75160263))
            {
                choices.Add(new Response("loc5", Helper.Translation.Get("MinecartLocation.5")));
                selectionActions.Add(delegate
                {
                    Game1.playSound("stairsdown");
                    Game1.warpFarmer("Custom_Ridgeside_SummitFarm", 77, 48, false);
                });
            }

            choices.Add(new Response("cancel", Helper.Translation.Get("Exit.Text")));
         selectionActions.Add(delegate { });


         Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("RSV.Minecart.Question"), choices, selectionActions);
         
        }
    }
}
