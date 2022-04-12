using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using StardewValley;
using StardewValley.Events;
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
        const string FAIRYFISH = "Fairytale Lionfish";
        const string FAIRYFLAG = "RSV.FairyMailFlag";
        const string NIGHTBLACK = "Nightblack Diamond";
        const string METEORFLAG = "RSV.MeteorMailFlag";

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            SpaceEvents.BeforeGiftGiven += BeforeGiftGiven;

            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.pickPersonalFarmEvent)),
                transpiler: new HarmonyMethod(typeof(BabyEventPatch).GetMethod("Transpiler"))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.pickFarmEvent)),
                postfix: new HarmonyMethod(typeof(TortsGifts), nameof(PickFarmEvent_Postfix))
            );
        }

        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.player.mailReceived.Contains(LOVERFLAG))
                Game1.player.RemoveMail(LOVERFLAG);
            else if (Game1.player.mailReceived.Contains(FAIRYFLAG))
                Game1.player.RemoveMail(FAIRYFLAG);
            else if (Game1.player.mailReceived.Contains(METEORFLAG))
                Game1.player.RemoveMail(METEORFLAG);
        }

        private static void BeforeGiftGiven(object sender, EventArgsBeforeReceiveObject e)
        {
            NPC giftee = e.Npc;
            if (giftee.Name != "Torts")
                return;

            Farmer gifter = Game1.player;
            StardewValley.Object gift = e.Gift;

            switch (gift.Name)
            {
                case MISTBLOOM:
                    Game1.weatherForTomorrow = Game1.weather_rain;
                    break;
                case FOXTAIL:
                    gifter.team.sharedDailyLuck.Value = 0.12;
                    break;
                case LOVERPIE:
                    gifter.mailReceived.Add(LOVERFLAG);
                    break;
                case FAIRYFISH:
                    gifter.mailReceived.Add(FAIRYFLAG);
                    break;
                case NIGHTBLACK:
                    gifter.mailReceived.Add(METEORFLAG);
                    break;
                default:
                    return;
            }

            DoPostGiftStuff(gifter, giftee, e);
        }

        private static void DoPostGiftStuff(Farmer gifter, NPC giftee, EventArgsBeforeReceiveObject e)
        {
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

        public static void PickFarmEvent_Postfix(ref FarmEvent __result)
        {
            if (__result != null)
                return;
            Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
            if (Game1.player.mailReceived.Contains(FAIRYFLAG) && random.NextDouble() < 0.25)
            {
                Log.Trace("RSV: Setting fairy event chance to 25%");
                __result = new FairyEvent();
            }
            else if (Game1.player.mailReceived.Contains(METEORFLAG) && random.NextDouble() < 0.10)
            {
                Log.Trace("RSV: Setting meteor event chance to 10%");
                __result = new SoundInTheNightEvent(1);
            }
        }

        public static class BabyEventPatch
        {
            public static float EditBabyChance(float chance)
            {
                try
                {
                    if (Game1.player.mailReceived.Contains(LOVERFLAG))
                    {
                        Log.Trace("RSV: Setting birth event chance to 50%");
                        chance = 0.5f;
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
                        ret.Add(new CodeInstruction(OpCodes.Call, typeof(BabyEventPatch).GetMethod("EditBabyChance")));
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

