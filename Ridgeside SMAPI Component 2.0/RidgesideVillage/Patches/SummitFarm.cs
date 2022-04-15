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
    internal static class SummitFarm
    {
        private static IModHelper Helper { get; set; }

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;
            Log.Trace($"Applying Harmony Patch from \"{nameof(SummitFarm)}\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CanPlantTreesHere)),
                postfix: new HarmonyMethod(typeof(SummitFarm), nameof(GameLocation_CanPlanTreesHere_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.spawnWeedsAndStones)),
                prefix: new HarmonyMethod(typeof(SummitFarm), nameof(GameLocation_SpawnWeedsAndStones_Prefix))
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

        private static bool GameLocation_SpawnWeedsAndStones_Prefix(ref GameLocation __instance)
        {
            try
            {
                    //set result to true
                    //set result to true
                if (__instance != null && __instance.Name.Equals("Custom_Ridgeside_SummitFarm") && Game1.getFarm().isBuildingConstructed("Gold Clock"))
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {

                Log.Error($"Harmony patch \"{nameof(GameLocation_SpawnWeedsAndStones_Prefix)}\" has encountered an error. \n{e.ToString()}");
                return true;
            }
        }
    }
}
