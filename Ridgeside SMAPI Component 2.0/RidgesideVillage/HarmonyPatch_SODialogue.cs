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

    internal static class HarmonyPatch_SODialogue
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
                prefix: new HarmonyMethod(typeof(HarmonyPatch_SODialogue), nameof(SpecialOrdersBoard_ReceiveLeftClick_prefix)));
            harmony.Patch(
                original: AccessTools.Method(QFSpecialBoardClass, "receiveLeftClick"),
                postfix: new HarmonyMethod(typeof(HarmonyPatch_SODialogue), nameof(SpecialOrdersBoard_ReceiveLeftClick_postfix)));
            harmony.Patch(
                original: AccessTools.Method(typeof(SpecialOrder), nameof(SpecialOrder.OnFail)),
                postfix: new HarmonyMethod(typeof(HarmonyPatch_SODialogue), nameof(HarmonyPatch_SODialogue.SpecialOrder_OnFail_postfix)));
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
                        Game1.player.activeDialogueEvents.Add(PIKATOPIC, specialOrder.GetDaysLeft());
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

        private static void SpecialOrder_OnFail_postfix(ref SpecialOrder __instance)
        {
            try
            {
                // Check that this is Pika's Delivery Quest
                if (__instance.questKey.Value != PIKAQUEST)
                {
                    return;
                }
                
                if (Game1.player.activeDialogueEvents.ContainsKey(PIKATOPIC))
                {
                    Game1.player.activeDialogueEvents.Remove(PIKATOPIC);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"RSV: Error removing CT from failed special order:\n\n{ex}");
            }

        }

    }
}
