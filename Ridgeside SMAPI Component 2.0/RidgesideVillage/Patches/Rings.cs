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



        private static void WithinPlayerThreshold_Prefix(NPC __instance, ref int threshold)
        {
            if ((__instance is Monster) && HasRingEquipped(RSVConstants.I_STEALTHRING))
            {
                threshold = Math.Max(threshold / 2, 2);
            }
        }
        private static bool Ghost_Prefix(Ghost __instance)
        {
            if (HasRingEquipped(RSVConstants.I_STEALTHRING) &&
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
            if (((__instance.ItemId == RSVConstants.I_RAERING) && (ring.ItemId == RSVConstants.I_BELRING)) || ((__instance.ItemId == RSVConstants.I_BELRING) && (ring.ItemId == RSVConstants.I_RAERING)))
            {
                Ring both_ring = new(RSVConstants.I_BOTHRING);
                __result = both_ring;
                return false;
            }
            return true;
        }

        private static bool CanCombine_Prefix(ref Ring __instance, Ring ring, ref bool __result)
        {
            // looks like {BELRING, RAERING} when sorted
            string[] rsv_rings = { RSVConstants.I_STEALTHRING, RSVConstants.I_BELRING, RSVConstants.I_RAERING, RSVConstants.I_BOTHRING };
            string[] curr_rings = { __instance.ItemId, ring.ItemId };
            var overlap = rsv_rings.Intersect(curr_rings);
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
            if ((curr_rings[0] == RSVConstants.I_BELRING) && (curr_rings[1] == RSVConstants.I_RAERING) || (curr_rings[0] == RSVConstants.I_RAERING) && (curr_rings[1] == RSVConstants.I_BELRING))
            {
                __result = true;
                return false;
            }
            __result = false;
            return false;
        }

        /// <summary>Get whether the player has any ring with the given ID equipped.</summary>
        /// <param name="id">The ring ID to match.</param>
        public static bool HasRingEquipped(string id)
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