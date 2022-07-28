using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;

namespace RidgesideVillage
{
    //Corrects the location name in the "X has begun in Y" message
    internal static class EventPatches
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            Log.Trace($"Applying Harmony Patch \"{nameof(ShowGlobalMessage_Prefix)}.");
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.showGlobalMessage)),
                prefix: new HarmonyMethod(typeof(EventPatches), nameof(ShowGlobalMessage_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkEventPrecondition)),
                prefix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.checkEventPrecondition_Prefix)));
        }

        internal static void ShowGlobalMessage_Prefix(ref string message)
        {
            try
            {
                if (message.Contains("Custom_Ridgeside_RidgesideVillage"))
                {
                    message = message.Replace("Custom_Ridgeside_RidgesideVillage", Helper.Translation.Get("Gathering.location"));

                }
                else if (message.Contains("Custom_Ridgeside_Ridge"))
                {
                    message = message.Replace("Custom_Ridgeside_Ridge", Helper.Translation.Get("EoR.location"));
                }
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(ShowGlobalMessage_Prefix)}\" has encountered an error. \n{e.ToString()}");
            }
        }

        internal static bool checkEventPrecondition_Prefix(ref string precondition, ref int __result)
        {
            if (precondition.Contains("/rsvRidingHorse", StringComparison.OrdinalIgnoreCase))
            {
                if(Game1.player.mount is null)
                {
                    __result = -1;
                    return false;
                }
                precondition = precondition.Replace("/rsvRidingHorse", "", StringComparison.OrdinalIgnoreCase);
                return true;
            }
            else
            {
                return true;
            }
        }

    }
}
