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

            Farmer gifter = Game1.player;
            StardewValley.Object gift = e.Gift;

            if (gift.Name == MISTBLOOM)
            {
                Game1.weatherForTomorrow = Game1.weather_rain;
            }
            else if (gift.Name == FOXTAIL)
            {
                gifter.team.sharedDailyLuck.Value = 0.12;
            }
            else if (gift.Name == LOVERPIE)
            {
                gifter.mailReceived.Add(LOVERFLAG);
            }
            else
                return;

            e.Cancel = true;
            gifter.reduceActiveItemByOne();
            gifter.currentLocation.localSound("give_gift");
            gifter.friendshipData["Torts"].GiftsToday++;
            gifter.friendshipData["Torts"].GiftsThisWeek++;
            gifter.friendshipData["Torts"].LastGiftDate = new WorldDate(Game1.Date);
            giftee.CurrentDialogue.Clear();
            giftee.CurrentDialogue.Push(new Dialogue("...", giftee));
            Game1.drawDialogue(giftee);
        }

        public static class BirthEventPatch
        {
            public static float ReplaceBirthChance(float chance)
            {
                try
                {
                    if (Game1.player.mailReceived.Contains(LOVERFLAG))
                    {
                        Log.Trace($"RSV: Setting birth event chance to 2/3");
                        chance = 0.66f;
                    }
                }
                catch{}
                return chance;
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, ILGenerator ilgen)
            {
                List<CodeInstruction> ret = new();

                foreach (var insn in insns)
                {
                    if (insn.opcode == OpCodes.Ldc_R4 && (insn.operand?.Equals(0.05) == true))
                    {
                        ret.Add(new CodeInstruction(OpCodes.Call, typeof(BirthEventPatch).GetMethod("ReplaceBirthChance")));
                    }
                    else
                    {
                        ret.Add(insn);
                    }
                }
                return ret;
            }

        }
    }
}

