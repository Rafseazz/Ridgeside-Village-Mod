using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Network;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{
    //This section is heavily inspired by tlitookilakin's Warp Network code, which can be found here:
    //https://github.com/tlitookilakin/WarpNetwork/tree/master/WarpNetwork

    internal static class WarpTotem
    {
        static readonly string Destination = "Custom_Ridgeside_Ridge";
        static readonly int Dest_X = 18;
        static readonly int Dest_Y = 10;

        static Color color = Color.Orange;

        static IModHelper Helper;
        static IMonitor Monitor;
        private static IJsonAssetsApi JsonAssets => ExternalAPIs.JA;
        public static int Totem = -1; 
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;

        }

        private static void OnButtonPressed(object sender, EventArgs e)
        {
            if (Totem == -1)
            {
                Totem = JsonAssets.GetObjectId("Warp Totem: Ridgeside");
            }
            try
            {
                if (Game1.player.CurrentItem?.ParentSheetIndex == Totem)
                {
                    DoTotemWarpEffects(Game1.player, (f) => DirectWarp());
                }
            }
            catch (Exception ex)
            {
                Log.Debug($"Could not find warp totem ID. Error: {ex}");
            }
        }

        public static bool DirectWarp()
        {
            if (!(Game1.getLocationFromName(Destination) is null) || !Game1.isFestival())
            {
                // Don't go if Ember of Resolutions is on, or player is at a festival
                if (!((Game1.Date.DayOfMonth == 28) && (Game1.Date.Season == "winter") && (Game1.timeOfDay < 2200)))
                {
                    Game1.warpFarmer(Destination, Dest_X, Dest_Y, flip: false);
                    return true;
                }
                else
                {
                    Monitor.Log("Failed to warp to '" + Destination + "': Ember of Resolutions festival not ready.", LogLevel.Debug);
                    Game1.drawObjectDialogue(Game1.parseText(Helper.Translation.Get("RSV.WarpFestival")));
                    return false;
                }
            }
            else
            {
                Monitor.Log("Failed to warp to '" + Destination + "': Location not found or player is at festival.", LogLevel.Error);
                Game1.drawObjectDialogue(Game1.parseText(Helper.Translation.Get("RSV.WarpFail")));
                return false;
            }
        }

        private static void DoTotemWarpEffects(Farmer who, Func<Farmer, bool> action)
        {
            who.jitterStrength = 1f;
            who.currentLocation.playSound("warrior", NetAudio.SoundContext.Default);
            who.faceDirection(2);
            who.canMove = false;
            who.temporarilyInvincible = true;
            who.temporaryInvincibilityTimer = -4000;
            Game1.changeMusicTrack("none", false, Game1.MusicContext.Default);
            who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
            {
                new FarmerSprite.AnimationFrame(57, 2000, false, false,  null, false),
                new FarmerSprite.AnimationFrame( (short) who.FarmerSprite.CurrentFrame, 0, false, false, new AnimatedSprite.endOfAnimationBehavior((f) => {
                    if (action(f))
                    {
                        who.reduceActiveItemByOne();
                    } else
                    {
                        who.temporarilyInvincible = false;
                        who.temporaryInvincibilityTimer = 0;
                    }
                }), true)
            }, null);
            // reflection
            Multiplayer mp = ModEntry.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            // --
            mp.broadcastSprites(who.currentLocation,
            new TemporaryAnimatedSprite(Totem, 9999f, 1, 999, who.Position + new Vector2(0.0f, -96f), false, false, false, 0.0f)
            {
                motion = new Vector2(0.0f, -1f),
                scaleChange = 0.01f,
                alpha = 1f,
                alphaFade = 0.0075f,
                shakeIntensity = 1f,
                initialPosition = who.Position + new Vector2(0.0f, -96f),
                xPeriodic = true,
                xPeriodicLoopTime = 1000f,
                xPeriodicRange = 4f,
                layerDepth = 1f
            },
            new TemporaryAnimatedSprite(Totem, 9999f, 1, 999, who.Position + new Vector2(-64f, -96f), false, false, false, 0.0f)
            {
                motion = new Vector2(0.0f, -0.5f),
                scaleChange = 0.005f,
                scale = 0.5f,
                alpha = 1f,
                alphaFade = 0.0075f,
                shakeIntensity = 1f,
                delayBeforeAnimationStart = 10,
                initialPosition = who.Position + new Vector2(-64f, -96f),
                xPeriodic = true,
                xPeriodicLoopTime = 1000f,
                xPeriodicRange = 4f,
                layerDepth = 0.9999f
            },
            new TemporaryAnimatedSprite(Totem, 9999f, 1, 999, who.Position + new Vector2(64f, -96f), false, false, false, 0.0f)
            {
                motion = new Vector2(0.0f, -0.5f),
                scaleChange = 0.005f,
                scale = 0.5f,
                alpha = 1f,
                alphaFade = 0.0075f,
                delayBeforeAnimationStart = 20,
                shakeIntensity = 1f,
                initialPosition = who.Position + new Vector2(64f, -96f),
                xPeriodic = true,
                xPeriodicLoopTime = 1000f,
                xPeriodicRange = 4f,
                layerDepth = 0.9988f
            });
            Game1.screenGlowOnce(color, false, 0.005f, 0.3f);
            Utility.addSprinklesToLocation(who.currentLocation, who.getTileX(), who.getTileY(), 16, 16, 1300, 20, color, null, true);
        }

    }

  
}
