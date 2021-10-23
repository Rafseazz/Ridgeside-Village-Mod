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
                prefix: new HarmonyMethod(typeof(HarmonyPatch_SummitFarm), nameof(GameLocation_CanPlanTreesHere_Prefix))
            );
        }

        private static bool GameLocation_CanPlanTreesHere_Prefix(ref GameLocation __instance, int sapling_index, int tile_x, int tile_y, ref bool __result)
        {
            try
            {
                if(__instance != null && __instance.Name.Equals("Custom_Ridgeside_SummitFarm"))
                {
                    //set result to true and dont execute vanilla method
                    __result = true;  
                    return false;
                }
                return true;
            }
            catch(Exception e)
            {

                Log.Error($"Harmony patch \"{nameof(GameLocation_CanPlanTreesHere_Prefix)}\" has encountered an error. \n{e.ToString()}");
                return true;
            }
        }
    }
}
