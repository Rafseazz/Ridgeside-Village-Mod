using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace RidgesideVillage.Patches
{
    internal static class SwimPatch
    {
        private static PerScreen<int> WaterEntryTime = new(() => -1);

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            harmony.Patch(
                original: AccessTools.DeclaredMethod(typeof(Farmer), nameof(Farmer.changeIntoSwimsuit)) ?? throw new InvalidOperationException("Farmer.changeIntoSwimsuit not found"),
                postfix: new HarmonyMethod(typeof(SwimPatch), nameof(PostfixChangeInto)));
            harmony.Patch(
                original: AccessTools.DeclaredMethod(typeof(Farmer), nameof(Farmer.changeOutOfSwimSuit)) ?? throw new InvalidOperationException("Farmer.changeOutOfSwimsuit not found"),
                postfix: new HarmonyMethod(typeof(SwimPatch), nameof(PostfixChangeOut)));
        }

        private static void PostfixChangeInto()
            => WaterEntryTime.Value = Game1.timeOfDay;

        private static void PostfixChangeOut(Farmer __instance)
        {
            if (WaterEntryTime.Value != -1 && WaterEntryTime.Value + 100 < Game1.timeOfDay)
            {
                __instance.activeDialogueEvents.Remove(RSVConstants.CT_KEAHITOPIC);
            }
        }
    }
}
