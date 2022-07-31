using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewModdingAPI.Events;
using StardewValley.Quests;
using RidgesideVillage.Questing;

    
namespace RidgesideVillage
{
    internal static class QuestPatches
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }


        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;
            harmony.Patch(
               original: AccessTools.Method(typeof(Quest), nameof(Quest.questComplete)),
               postfix: new HarmonyMethod(typeof(QuestPatches), nameof(Quest_questComplete_postfix))
           );

        }

        private static void Quest_questComplete_postfix(Quest __instance)
        {
            QuestController.FinishedQuests.Value.Add(__instance.id.Value);
            Log.Trace($"Added ID {__instance.id.Value}");
        }
    }
}
