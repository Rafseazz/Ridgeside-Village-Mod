using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewModdingAPI.Events;
using Netcode;
    
namespace RidgesideVillage
{
    internal static class ClothingDonation
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }


        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;
            Log.Trace($"Applying Harmony Patch \"{nameof(Animations)}\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(QuestContainerMenu), "TryToPlace"),
                postfix: new HarmonyMethod(typeof(ClothingDonation), nameof(TryToPlace_Postfix))
            );
            /*
            // This one isn't working so bleh
            harmony.Patch(
                original: AccessTools.Method(typeof(QuestContainerMenu), "TryToGrab"),
                postfix: new HarmonyMethod(typeof(ClothingDonation), nameof(TryToGrab_Postfix))
            );
            */
        }

        internal static void TryToPlace_Postfix(ref QuestContainerMenu __instance, ref Item __result)
        {
            try
            {
                if (!(__result is Clothing))
                    return;
                //Clothes don't stack so don't have to worry about stack number
                DonateObjective obj = FindItemObjective(__result, false);
                if (obj != null)
                    __result = null;
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(TryToPlace_Postfix)}\" has encountered an error. \n{e}");
            }
        }

        internal static void TryToGrab_Postfix(ref QuestContainerMenu __instance, ref Item __result)
        {
            try
            {
                if (!(__result is Clothing))
                    return;
                //Clothes don't stack so don't have to worry about stack number
                DonateObjective obj = FindItemObjective(__result, true);
                var complete = Helper.Reflection.GetField<bool>(obj, "_complete");
                Log.Debug($"Objective is complete: {complete}");
                complete.SetValue(false);
                __result = null;
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(TryToGrab_Postfix)}\" has encountered an error. \n{e}");
            }
        }

        internal static DonateObjective FindItemObjective(Item item, bool shouldBeComplete)
        {
            foreach (SpecialOrder so in Game1.player.team.specialOrders)
            {
                foreach (OrderObjective o in so.objectives)
                {
                    if ((o.IsComplete() == shouldBeComplete) && (o is DonateObjective))
                    {
                        DonateObjective obj = o as DonateObjective;
                        if (obj.dropBoxGameLocation.Value == Game1.currentLocation.Name)
                        {
                            List<string> obj_tags = obj.acceptableContextTagSets.ToList();
                            List<string> item_tags = item.GetContextTagList();
                            if (obj_tags.Any(x => item_tags.Any(y => y == x)))
                                return obj;
                        }
                    }
                }
            }
            return null;
        }

    }
}
