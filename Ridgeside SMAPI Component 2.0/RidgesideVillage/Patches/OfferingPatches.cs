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
    internal static class OfferingPatches
    {
        static IModHelper Helper;

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.pickPersonalFarmEvent)),
                transpiler: new HarmonyMethod(typeof(BabyEventPatch).GetMethod(nameof(BabyEventPatch.Transpiler)))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.pickFarmEvent)),
                postfix: new HarmonyMethod(typeof(OfferingPatches), nameof(PickFarmEvent_Postfix))
            );
        }

        public static void PickFarmEvent_Postfix(ref FarmEvent __result)
        {
            if (__result != null)
                return;
            Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
            if (Game1.player.mailReceived.Contains(RSVConstants.M_TORTSFAIRY) && random.NextDouble() < 0.25)
            {
                Log.Trace("RSV: Setting fairy event chance to 25%");
                __result = new FairyEvent();
                Game1.player.RemoveMail(RSVConstants.M_TORTSFAIRY);
            }
            else if (Game1.player.mailReceived.Contains(RSVConstants.M_TORTSMETEOR) && random.NextDouble() < 0.10)
            {
                Log.Trace("RSV: Setting meteor event chance to 10%");
                __result = new SoundInTheNightEvent(1);
                Game1.player.RemoveMail(RSVConstants.M_TORTSMETEOR);
            }
        }

        public static class BabyEventPatch
        {
            public static float EditBabyChance(float chance)
            {
                try
                {
                    if (Game1.player.mailReceived.Contains(RSVConstants.M_TORTSLOVE))
                    {
                        Log.Trace("RSV: Setting birth event chance to 50%");
                        chance = 0.5f;
                        Game1.player.RemoveMail(RSVConstants.M_TORTSLOVE);
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

