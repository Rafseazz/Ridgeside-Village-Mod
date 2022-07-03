using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{

    internal static class SODialogue
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }

        const string PIKAQUEST = "RSV.SpecialOrder.PikaDeliver";
        const string PIKATOPIC = "pika_pickup";

        public static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;
            Type QFSpecialBoardClass = Type.GetType("QuestFramework.Framework.Menus.CustomOrderBoard, QuestFramework");

            harmony.Patch(
                original: AccessTools.Method(QFSpecialBoardClass, "receiveLeftClick"),
                prefix: new HarmonyMethod(typeof(SODialogue), nameof(SpecialOrdersBoard_ReceiveLeftClick_prefix)),
                postfix: new HarmonyMethod(typeof(SODialogue), nameof(SpecialOrdersBoard_ReceiveLeftClick_postfix)));
            harmony.Patch(
                original: AccessTools.Method(typeof(SpecialOrder), nameof(SpecialOrder.OnFail)),
                prefix: new HarmonyMethod(typeof(SODialogue), nameof(SODialogue.SpecialOrder_OnFail_prefix)));
        }
        private static void SpecialOrdersBoard_ReceiveLeftClick_prefix(ref bool __state)
        {
            try
            {
                foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
                {
                    if (specialOrder.questKey.Value == PIKAQUEST)
                    {
                        __state = true;
                        return;
                    }
                }
                __state = false;
            }
            catch (Exception ex)
            {
                Log.Error($"RSV: Error in SODialogue prefix:\n\n{ex}");
            }
        }
            // Checks if quest has been added since prefix, and if so, adds CT
            private static void SpecialOrdersBoard_ReceiveLeftClick_postfix(bool __state)
        {
            try
            {
                foreach(SpecialOrder specialOrder in Game1.player.team.specialOrders)
                {
                    if ((specialOrder.questKey.Value == PIKAQUEST) && (!__state))
                    {
                        foreach(Farmer player in Game1.getAllFarmers())
                        {
                            player.activeDialogueEvents.Add(PIKATOPIC, specialOrder.GetDaysLeft());
                        }
                        Log.Trace($"RSV: Added pika_pickup conversation topic.");
                        return;
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error($"RSV: Error in SODialogue postfix:\n\n{ex}");
            }
        }

        // Don't put the delivered items in the lost and found
        private static bool SpecialOrder_OnFail_prefix(ref SpecialOrder __instance)
        {
            try
            {
                if (__instance.questKey.Value != PIKAQUEST)
                {
                    return true;
                }
                foreach (OrderObjective objective in __instance.objectives)
                {
                    objective.OnFail();
                }
                if (Game1.IsMasterGame)
                {
                    __instance.HostHandleQuestEnd();
                }
                __instance.questState.Value = SpecialOrder.QuestState.Failed;
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"RSV: Error prefixing SpecialOrder.OnFail:\n\n{ex}");
            }
            return true;

        }

    }
}
