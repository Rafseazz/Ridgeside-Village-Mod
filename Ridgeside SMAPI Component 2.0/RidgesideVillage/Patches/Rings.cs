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
using StardewValley.Menus;
using StardewValley.Objects;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using System.Reflection;

namespace RidgesideVillage
{

    internal static class Rings
    {
        private static IModHelper Helper { get; set; }

        const string STEALTHRING = "Glove of the Assassin";
        public static int StealthRingId = -1;

        const string RAERING = "The Fairy Doe Fox's Ring";
        const string BELRING = "The Beautiful Serpent's Ring";
        const string BOTHRING = "The Ring of the Ridge Deities";
        public static int BothRingId = -1;

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            Log.Trace($"Applying Harmony Patch \"{nameof(Rings)}.");
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.withinPlayerThreshold), new Type[] { typeof(int) }),
                prefix: new HarmonyMethod(typeof(Rings), nameof(WithinPlayerThreshold_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Ghost), nameof(Ghost.behaviorAtGameTick)),
                prefix: new HarmonyMethod(typeof(Rings), nameof(Ghost_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Ghost), "updateAnimation"),
                prefix: new HarmonyMethod(typeof(Rings), nameof(Ghost_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Ring), nameof(Ring.Combine)),
                prefix: new HarmonyMethod(typeof(Rings), nameof(Combine_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Ring), nameof(Ring.CanCombine)),
                prefix: new HarmonyMethod(typeof(Rings), nameof(CanCombine_Prefix))
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

        private static bool Combine_Prefix(ref Ring __instance, Ring ring, ref Item __result)
        {
            if (((__instance.Name == RAERING) && (ring.Name == BELRING)) || ((__instance.Name == BELRING) && (ring.Name == RAERING)))
            {
                if (BothRingId == -1)
                {
                    BothRingId = GetRingId(BOTHRING);
                }
                Ring both_ring = new(BothRingId);
                __result = both_ring;
                return false;
            }
            return true;
        }

        private static bool CanCombine_Prefix(ref Ring __instance, Ring ring, ref bool __result)
        {
            // looks like {BELRING, RAERING} when sorted
            string[] our_rings = { BELRING, BOTHRING, RAERING, STEALTHRING };
            string[] their_rings = { __instance.Name, ring.Name };
            var overlap = our_rings.Intersect(their_rings);
            int overlap_size = overlap.Count();
            if (overlap_size == 0)
            {
                return true;
            }
            Log.Info($"RSV: Attempt to combine RSV ring detected.");
            if (overlap_size == 1)
            {
                __result = false;
                return false;
            }
            // at this point 2 rings is the only option left
            Array.Sort(our_rings);
            if ((their_rings[0] == BELRING) && (their_rings[1] == RAERING))
            {
                __result = true;
                return false;
            }
            __result = false;
            return false;
        }

        /// <summary>Get whether the player has any ring with the given ID equipped.</summary>
        /// <param name="id">The ring ID to match.</param>
        public static bool HasRingEquipped(int id)
        {
            var rings = UtilFunctions.GetAllRings(Game1.player);
            foreach(var ring in rings)
            {
                if (ring.GetEffectsOfRingMultiplier(id) is int count && count > 0) {
                    return true;
                }
            }
            return false;
            /*
            if (Game1.player.leftRing.Value?.GetEffectsOfRingMultiplier(id) is int count && count > 0)
            {
                return true;
            }
            if (Game1.player.rightRing.Value?.GetEffectsOfRingMultiplier(id) is int count2 && count2 > 0) { return true; }

            if (ExternalAPIs.MR?.CountEquippedRings(Game1.player, id) is int count3 && count3 > 0) { return true; }
            return false;
            */
        }
    }

}