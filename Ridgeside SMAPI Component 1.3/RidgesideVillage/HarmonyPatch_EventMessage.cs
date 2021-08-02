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
    internal static class HarmonyPatch_EventMessage
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            Log.Trace($"Applying Harmony Patch \"{nameof(ShowGlobalMessage_Prefix)}\" prefixing SDV method \"SpecialOrders.IsTimedQuest()\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.showGlobalMessage)),
                prefix: new HarmonyMethod(typeof(HarmonyPatch_EventMessage), nameof(ShowGlobalMessage_Prefix))
            );
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

    }
}
