using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using StardewValley;
using StardewModdingAPI.Events;
using SpaceCore.Events;
using HarmonyLib;

namespace RidgesideVillage
{
    internal static class TortsGifts
    {
        static IModHelper Helper;
        const string MISTBLOOM = "Mountain Mistbloom";
        const string FOXTAIL = "Old Lucky Foxtail Charm";
        const string LOVERPIE = "Strawberry Lover Pie";
        const string LOVERFLAG = "RSV.LoverMailFlag";

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            SpaceEvents.BeforeGiftGiven += BeforeGiftGiven;

            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.pickPersonalFarmEvent)),
                transpiler: new HarmonyMethod(typeof(BirthEventPatch).GetMethod("Transpiler"))
            );
        }

        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.player.mailReceived.Contains(LOVERFLAG))
                Game1.player.RemoveMail(LOVERFLAG);
        }

        private static void BeforeGiftGiven(object sender, EventArgsBeforeReceiveObject e)
        {
            NPC giftee = e.Npc;
            if (giftee.Name != "Torts")
                return;

            StardewValley.Object gift = e.Gift;

            if (gift.Name == MISTBLOOM)
            {
                e.Cancel = true;
                Game1.weatherForTomorrow = Game1.weather_rain;
            }
            else if (gift.Name == FOXTAIL)
            {
                e.Cancel = true;
                IReflectedField<float> dailyLuck = Helper.Reflection.GetField<float>(Game1.player, "DailyLuck");
                dailyLuck.SetValue(dailyLuck.GetValue() + 0.1f);
            }
            else if (gift.Name == LOVERPIE)
            {
                e.Cancel = true;
                Game1.player.mailReceived.Add(LOVERFLAG);
            }
        }

        public static class BirthEventPatch
        {
            public static float ReplaceBirthChance(float chance)
            {
                if (Game1.player.mailReceived.Contains(LOVERFLAG))
                    chance = 0.66f;
                return chance;
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, ILGenerator ilgen)
            {
                //Log.Debug($"RSV: Hopefully transpiling...");
                List<CodeInstruction> ret = new();

                foreach (var insn in insns)
                {
                    if (insn.opcode == OpCodes.Ldc_R8 && (insn.operand?.Equals(0.05) == true))
                    {
                        ret.Add(new CodeInstruction(OpCodes.Ldc_R8, typeof(BirthEventPatch).GetMethod("ReplaceBirthChance")));
                    }
                    else
                    {
                        ret.Add(insn);
                    }
                    //Log.Debug($"RSV: {ret.Last()}");
                }
                return ret;
            }

        }
    }
}

