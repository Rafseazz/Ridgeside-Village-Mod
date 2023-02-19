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
using StardewModdingAPI.Events;
    
namespace RidgesideVillage
{
    internal static class Animations
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }
        private static string[] npcs = { "Torts", "Carmen", "Blair", "Kenneth", "June" };


        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;
            Log.Trace($"Applying Harmony Patch \"{nameof(Animations)}\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), "startRouteBehavior"),
                postfix: new HarmonyMethod(typeof(Animations), nameof(startRouteBehavior_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), "finishRouteBehavior"),
                prefix: new HarmonyMethod(typeof(Animations), nameof(finishRouteBehavior_Prefix))
            );
            Helper.Events.GameLoop.DayEnding += DayEnd;
        }

        internal static void startRouteBehavior_Postfix(ref NPC __instance, string behaviorName)
        {
            try
            {
                if (behaviorName.Length > 0 && behaviorName[0] == '"')
                {
                    return;
                }
                switch (behaviorName)
                {
                    case "torts_invisible":
                        __instance.extendSourceRect(48, -16);
                        __instance.Sprite.SpriteWidth = 64;
                        __instance.Sprite.SpriteHeight = 16;
                        __instance.Sprite.ignoreSourceRectUpdates = false;
                        __instance.Sprite.currentFrame = 0;
                        break;
                    case "blair_side_fish":
                        __instance.extendSourceRect(16, 0);
                        __instance.Sprite.SpriteWidth = 32;
                        __instance.Sprite.ignoreSourceRectUpdates = false;
                        __instance.Sprite.currentFrame = 16;
                        if (Utility.isOnScreen(Utility.Vector2ToPoint(__instance.Position), 64, __instance.currentLocation))
                        {
                            __instance.currentLocation.playSoundAt("slosh", __instance.getTileLocation());
                        }
                        break;
                    case "june_piano":
                        __instance.extendSourceRect(16, 0);
                        __instance.Sprite.SpriteWidth = 32;
                        __instance.Sprite.ignoreSourceRectUpdates = false;
                        __instance.Sprite.currentFrame = 8;
                        break;
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
                Log.Error($"Harmony patch \"{nameof(startRouteBehavior_Postfix)}\" has encountered an error. \n{e}");
            }
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
                    case "torts_invisible":
                    case "blair_side_fish":
                    case "blair_fish":
                    case "carmen_fish":
                    case "kenneth_fixfront":
                    case "kenneth_fixright":
                    case "kenneth_fixback":
                    case "kenneth_fixleft":
                    case "june_piano":
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
                Log.Error($"Harmony patch \"{nameof(finishRouteBehavior_Prefix)}\" has encountered an error. \n{e}");
                return true;
            }

        }

        private static void DayEnd(object sender, DayEndingEventArgs args)
        {// reset sprites in case user ended day while animations were in progress
            try
            {
                foreach(string name in npcs)
                {
                    NPC npc = Game1.getCharacterFromName(name);
                    npc.Sprite.SpriteHeight = 32;
                    npc.Sprite.SpriteWidth = 16;
                    npc.Sprite.ignoreSourceRectUpdates = false;
                    npc.Sprite.UpdateSourceRect();
                    npc.drawOffset.Value = Vector2.Zero;
                    npc.IsInvisible = false;
                }
            }
            catch (Exception e)
            {
                Log.Warn($"Failed in RSV Animations Day End reset:\n{e}");
            }


        }

    }
}
