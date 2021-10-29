using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.Tools;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using System.Reflection;

namespace RidgesideVillage
{
    //Handles the treasures for the spirit realm.
    //Mostly hardcoded for performance
    internal static class HarmonyPatch_SummitFarm
    {
        private static IModHelper Helper { get; set; }

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;
            Log.Trace($"Applying Harmony Patch from \"{nameof(HarmonyPatch_SummitFarm)}\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CanPlantTreesHere)),
                postfix: new HarmonyMethod(typeof(HarmonyPatch_SummitFarm), nameof(GameLocation_CanPlanTreesHere_Postfix))
            );
        }

        private static void GameLocation_CanPlanTreesHere_Postfix(ref GameLocation __instance, int sapling_index, int tile_x, int tile_y, ref bool __result)
        {
            try
            {
                if(__instance != null && __instance.Name.Equals("Custom_Ridgeside_SummitFarm"))
                {
                    //set result to true
                    __result = true;
                }
                return;
            }
            catch(Exception e)
            {

                Log.Error($"Harmony patch \"{nameof(GameLocation_CanPlanTreesHere_Postfix)}\" has encountered an error. \n{e.ToString()}");
                return;
            }
        }
    }
}
