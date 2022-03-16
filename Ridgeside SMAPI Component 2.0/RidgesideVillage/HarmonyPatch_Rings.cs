using StardewModdingAPI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using System.Reflection;

namespace RidgesideVillage
{

    internal static class HarmonyPatch_Rings
    {
        private static IModHelper Helper { get; set; }

        public static string STEALTHRING = "Glove of the Assassin";
        public static int StealthRingId = -1;
        public static string RAERING = "Raeriyala's Ring";
        public static int RaeRingId = -1;
        public static string BELRING = "Belinda's Ring";
        public static int BelRingId = -1;

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            Log.Trace($"Applying Harmony Patch \"{nameof(HarmonyPatch_Rings)}.");
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.withinPlayerThreshold), new Type[] { typeof(int) }),
                prefix: new HarmonyMethod(typeof(HarmonyPatch_Rings), nameof(WithinPlayerThreshold_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Ghost), nameof(Ghost.behaviorAtGameTick)),
                prefix: new HarmonyMethod(typeof(HarmonyPatch_Rings), nameof(Ghost_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Ghost), "updateAnimation"),
                prefix: new HarmonyMethod(typeof(HarmonyPatch_Rings), nameof(Ghost_Prefix))
            );
        }

        private static int GetRingId(string obj)
        {
            return ExternalAPIs.JA.GetObjectId(obj);
        }

        private static void WithinPlayerThreshold_Prefix(NPC __instance, ref int threshold)
        {
            if (StealthRingId == -1)
            {
                StealthRingId = GetRingId(STEALTHRING);
            }
            if ((__instance is Monster) && HasRingEquipped(StealthRingId))
            {
                threshold = Math.Max(threshold / 2, 2);
            }
        }
        private static bool Ghost_Prefix(Ghost __instance)
        {
            if (StealthRingId == -1)
            {
                StealthRingId = GetRingId(STEALTHRING);
            }
            if (HasRingEquipped(StealthRingId) &&
                ((__instance.Position.X - Game1.viewport.X > Game1.viewport.Width) ||
                (__instance.Position.Y - Game1.viewport.Y > Game1.viewport.Height)))
            {
                __instance.Halt();
                __instance.faceGeneralDirection(__instance.Player.getStandingPosition());
                return false;
            }
            return true;
        }

        /// <summary>Get whether the player has any ring with the given ID equipped.</summary>
        /// <param name="id">The ring ID to match.</param>
        public static bool HasRingEquipped(int id)
        {

            if (Game1.player.leftRing.Value?.GetEffectsOfRingMultiplier(id) is int count && count > 0)
            {
                return true;
            }
            if (Game1.player.rightRing.Value?.GetEffectsOfRingMultiplier(id) is int count2 && count2 > 0) { return true; }

            if (ExternalAPIs.MR?.CountEquippedRings(Game1.player, id) is int count3 && count3 > 0) { return true; }
            return false;
        }

    }

}