using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewModdingAPI;

namespace RidgesideVillage
{
    internal static class Backgrounds
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }

        private static AlphaTestEffect ate;
        const string TORTSREALM = "Custom_Ridgeside_TortsRealm";

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;
            Log.Trace($"Applying Harmony Patch \"{nameof(Backgrounds)}\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(Background), nameof(Background.draw)),
                postfix: new HarmonyMethod(typeof(Backgrounds), nameof(Background_draw_postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), "resetLocalState"),
                postfix: new HarmonyMethod(typeof(Backgrounds), nameof(GameLocation_resetLocalState_postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.cleanupBeforePlayerExit)),
                postfix: new HarmonyMethod(typeof(Backgrounds), nameof(GameLocation_cleanUpBeforePlayerExit_postfix))
            );
        }

        public static void Background_draw_postfix(Background __instance, SpriteBatch b)
        {
            if (__instance is TortsBackground bg)
                bg.Draw(b);
            
        }

        public static void GameLocation_resetLocalState_postfix(ref GameLocation __instance)
        {
            if (__instance.NameOrUniqueName == TORTSREALM)
                Game1.background = new TortsBackground();
        }

        public static void GameLocation_cleanUpBeforePlayerExit_postfix(ref GameLocation __instance)
        {
            if (__instance.NameOrUniqueName == TORTSREALM)
                Game1.background = null;
        }

    }
}