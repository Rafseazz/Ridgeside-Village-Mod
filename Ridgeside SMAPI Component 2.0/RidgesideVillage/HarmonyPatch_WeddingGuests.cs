using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;

namespace RidgesideVillage
{

    //This Patch is heavily inspired from Stardew Valley Expandeds Code, which can be found there:
    //https://github.com/FlashShifter/StardewValleyExpanded/blob/master/Code/CustomWeddingGuests.cs
    //Patch is only applied if SVE is not loaded.

    internal static class HarmonyPatch_WeddingGuests
    {
        private static IModHelper Helper { get; set; }

        private const string AssetName = "Data/CustomWeddingGuestPositions";

        public static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;
            if (Helper.ModRegistry.IsLoaded("FlashShifter.SVECode"))
            {
                //SVE is loaded, nothing to do
                return;
            }
            Log.Trace($"Applying Harmony Patch \"{nameof(HarmonyPatch_WeddingGuests)}.");
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getCelebrationPositionsForDatables), new Type[]{ typeof(List<string>) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatch_WeddingGuests), nameof(getCelebrationPositionsForDatables_Postfix))
            );
        }

        public static void getCelebrationPositionsForDatables_Postfix(ref string __result, List<string> people_to_exclude)
        {
            try
            {
                Dictionary<string, string> NPCEntries = Game1.content.Load<Dictionary<string, string>>(AssetName);
                string result = __result;
                foreach(var key in NPCEntries.Keys)
                {
                    if (!people_to_exclude.Contains(key))
                    {
                        result += $"{key} {NPCEntries[key]} ";
                    }
                }
                __result = result;
                return;
            }
            catch (Exception ex)
            {
                Log.Error($"Error setting wedding positions for custom NPCs: {ex}");
                return;
            }
        }

    }
}
