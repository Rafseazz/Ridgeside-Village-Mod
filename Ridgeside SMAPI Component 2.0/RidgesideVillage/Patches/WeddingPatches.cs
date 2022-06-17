using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using Microsoft.Xna.Framework;

namespace RidgesideVillage
{

    //This Patch is heavily inspired from Stardew Valley Expandeds Code, which can be found there:
    //https://github.com/FlashShifter/StardewValleyExpanded/blob/master/Code/CustomWeddingGuests.cs
    //Patch is only applied if SVE is not loaded.

    internal static class WeddingPatches
    {
        private static IModHelper Helper { get; set; }

        private const string AssetName = "Data/CustomWeddingGuestPositions";

        public static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;
            if (!Helper.ModRegistry.IsLoaded("FlashShifter.SVECode"))
            {
                Log.Trace($"Applying Harmony Patch \"{nameof(WeddingPatches)}.");
                harmony.Patch(
                    original: AccessTools.Method(typeof(Utility), nameof(Utility.getCelebrationPositionsForDatables), new Type[] { typeof(List<string>) }),
                    postfix: new HarmonyMethod(typeof(WeddingPatches), nameof(getCelebrationPositionsForDatables_Postfix))
                );
            }

            harmony.Patch(
                original: AccessTools.Method(typeof(Farm), nameof(Farm.startEvent)),
                postfix: new HarmonyMethod(typeof(WeddingPatches), nameof(WeddingPatches.Farm_StartEvent_Postfix))
            );

        }

        //fixes the weddinreception offset issue on custom farms
        private static void Farm_StartEvent_Postfix(Event evt)
        {
            if(evt.id == 75160245)
            {
                evt.eventPositionTileOffset = new Vector2(0, 0);
            }
        }

        //Adds Weddingguests (if SVE isnt installed, otherwise SVE does)
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
