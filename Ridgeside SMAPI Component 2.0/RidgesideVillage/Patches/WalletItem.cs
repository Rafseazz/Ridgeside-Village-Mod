using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Powers;
using System;
using System.Runtime.CompilerServices;

namespace RidgesideVillage
{
    //This section is heavily inspired by spacechase0's Moon Misadventures code, which can be found here:
    //https://github.com/spacechase0/StardewValleyMods/tree/develop/MoonMisadventures
    internal static class WalletItem
    {
        const string UNLOCKEVENT = "75160380";

        private static IModHelper Helper { get; set; }

        internal class Holder { public readonly NetBool Value = new(); }
        internal static ConditionalWeakTable<FarmerTeam, Holder> values = new();

        public static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            try
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Object), "getPriceAfterMultipliers"),
                    postfix: new HarmonyMethod(typeof(WalletItem), nameof(WalletItem.Object_getPriceAfterMultipliers_Postfix))
                    );
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(WalletItem)}\" has encountered an error. \n{e}");
                return;
            }

        }

        public static void Object_getPriceAfterMultipliers_Postfix(StardewValley.Object __instance, float startPrice, ref float __result, long specificPlayerID = -1L)
        {
            if (__instance.Category != -4 && __instance.Category != -26)
            {
                return;
            }

            float saleMultiplier = 1f;

            if (playerHasRiveraSecret())
            {
                if (__instance.GetContextTags().Contains("rsv_fish"))
                {
                    saleMultiplier = 2f;
                }
                else if(__instance.ItemId.Equals("SmokedFish"))
                {
                    foreach (var contextTag in __instance.GetContextTags())
                    {
                        if (contextTag.StartsWith("preserve_sheet_index_rafseazz.rsvcp"))
                        {
                            saleMultiplier = 1.5f;
                            break;
                        }
                        
                    }
                }
            }

            __result = __result * saleMultiplier;
        }

        private static bool playerHasRiveraSecret()
        {
        
            var powersData = DataLoader.Powers(Game1.content);
            if (powersData.TryGetValue(RSVConstants.P_RIVERASECRET, out var data))
            {
                return GameStateQuery.CheckConditions(data.UnlockedCondition);
            }

            return false;
        }
    }
  
}
