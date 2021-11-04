using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{
    //Corrects the location name in the "X has begun in Y" message
    internal static class HarmonyPatch_Animations
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }


        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;
            Log.Trace($"Applying Harmony Patch \"{nameof(HarmonyPatch_Animations)}\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), "startRouteBehavior"),
                postfix: new HarmonyMethod(typeof(HarmonyPatch_Animations), nameof(startRouteBehavior_Postifx))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), "finishRouteBehavior"),
                prefix: new HarmonyMethod(typeof(HarmonyPatch_Animations), nameof(finishRouteBehavior_Prefix))
            );
        }

        private static bool finishRouteBehavior_Prefix(ref NPC __instance, string behaviorName)
        {
            try
            {
                if (behaviorName.Length > 0 && behaviorName[0] == '"')
                {
                    return true;
                }
                switch (behaviorName)
                {
                    case "carmen_fish":
                    case "blair_fish":
                    case "kenneth_fixfront":
                    case "kenneth_fixright":
                    case "kenneth_fixback":
                    case "kenneth_fixleft":
                        __instance.reloadSprite();
                        __instance.Sprite.SpriteWidth = 16;
                        __instance.Sprite.SpriteHeight = 32;
                        __instance.Sprite.UpdateSourceRect();
                        __instance.drawOffset.Value = Vector2.Zero;
                        __instance.Halt();
                        __instance.movementPause = 1;
                        break;
                }
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(finishRouteBehavior_Prefix)}\" has encountered an error. \n{e.ToString()}");
                return true;
            }

        }

        internal static void startRouteBehavior_Postifx(ref NPC __instance, string behaviorName)
        {
            try
            {
                if (behaviorName.Length > 0 && behaviorName[0] == '"')
                {
                    return;
                }
                switch (behaviorName)
                {
                    case "carmen_fish":
                    case "blair_fish":
                    case "kenneth_fixfront":
                    case "kenneth_fixright":
                    case "kenneth_fixback":
                    case "kenneth_fixleft":
                        __instance.extendSourceRect(0, 32);
                        __instance.Sprite.tempSpriteHeight = 64;
                        __instance.drawOffset.Value = new Vector2(0f, 96f);
                        __instance.Sprite.ignoreSourceRectUpdates = false;
                        if (Utility.isOnScreen(Utility.Vector2ToPoint(__instance.Position), 64, __instance.currentLocation))
                        {
                            __instance.currentLocation.playSoundAt("slosh", __instance.getTileLocation());
                        }
                        break;
                }
                
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(startRouteBehavior_Postifx)}\" has encountered an error. \n{e.ToString()}");
            }
        }

    }
}
