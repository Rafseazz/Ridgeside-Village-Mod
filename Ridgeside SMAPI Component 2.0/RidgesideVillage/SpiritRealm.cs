using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;
using StardewValley.SpecialOrders;

namespace RidgesideVillage
{
    internal static class SpiritRealm
    {
        static IModHelper Helper;
        static IMonitor Monitor;
        static bool IsRenderingFog;
        static Texture2D FogTexture;
        static Vector2 FogPosition;

        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;
            FogTexture = Helper.ModContent.Load<Texture2D>(PathUtilities.NormalizePath("assets/SpiritRealmFog.png"));

            GameLocation.RegisterTileAction("RSVTeleport", RSVWarp);
            GameLocation.RegisterTileAction("RSVCorruptedFire", CleanseCorruptedFire);
            Helper.Events.Player.Warped += OnWarped;
        }


        private static bool CleanseCorruptedFire(GameLocation location, string[] arg2, Farmer farmer, Point point)
        {
            if (!location.Equals(Game1.getLocationFromName(RSVConstants.L_SPIRITREALM)))
            {
                return false;
            }
            int Xcoord = point.X;
            int Ycoord = point.Y;

            string key = $"cleansed {Xcoord} {Ycoord}";
            if (location.modData.ContainsKey(key)){
                return true;
            }
            location.modData[key] = "t";
            Game1.playSound("shadowpeep");
            location.removeTile(Xcoord, Ycoord - 1, "Front");
            location.removeTile(Xcoord, Ycoord, "Buildings");

            var tilesheets = location.map.TileSheets;
            int i = 0;
            int index = -1;
            foreach(var tilesheet in tilesheets)
            {
                if (tilesheet.Id.Equals("zrsvspiritrealm"))
                {
                    index = i;
                }
                i++;
            }

            location.setMapTile(Xcoord, Ycoord, 1715, "Buildings", null, whichTileSheet: index);

            var extinguishSprite = new TemporaryAnimatedSprite("LooseSprites\\RSVSmoke", new Rectangle(0, 0, 16, 32), 100f, 10, 1, new Vector2(Xcoord, Ycoord - 1) * 64, false, false, ((Ycoord+0.1f) * 64f) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
            {
                delayBeforeAnimationStart = 0,
                drawAboveAlwaysFront = false
            };
            var multiplayer = Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            multiplayer.broadcastSprites(location, extinguishSprite);

            checkForCorruptedFireEvent();
            return true;
        }

        static void removeCorruptedFireTiles()
        {

            var location = Game1.getLocationFromName(RSVConstants.L_SPIRITREALM);
            var modData = location.modData;

            var tilesheets = location.map.TileSheets;
            int i = 0;
            int tileSheetIndex = -1;
            foreach (var tilesheet in tilesheets)
            {
                if (tilesheet.Id.Equals("zrsvspiritrealm"))
                {
                    tileSheetIndex = i;
                }
                i++;
            }

            foreach (var key in modData.Keys)
            {
                if (key.StartsWith("cleansed"))
                {
                    var split = key.Split(' ');
                    int xCoord = int.Parse(split[1]);
                    int yCoord = int.Parse(split[2]);
                    location.removeTile(xCoord, yCoord - 1, "Front");
                    location.removeTile(xCoord, yCoord, "Buildings");

                    location.setMapTile(xCoord, yCoord, 1715, "Buildings", null, whichTileSheet: tileSheetIndex);
                }
            }
        }

        static void checkForCorruptedFireEvent()
        {
            var location = Game1.getLocationFromName(RSVConstants.L_SPIRITREALM);
            var modData = location.modData;
            int counter = 0;
            foreach(var key in modData.Keys)
            {
                if (key.StartsWith("cleansed")){
                    counter++;
                }
            }

            if (Game1.player.team.SpecialOrderActive(RSVConstants.SO_CLEANSING))
            {
                SpecialOrder corruptedFireQuest = Game1.player.team.specialOrders.Where(so => so.questKey.Equals(RSVConstants.SO_CLEANSING)).FirstOrDefault();
                if(corruptedFireQuest != null)
                {
                    corruptedFireQuest.objectives[0].currentCount.Value = counter;
                    corruptedFireQuest.CheckCompletion();
                }
            }

            if(counter >= 5)
            {
                location.TryGetLocationEvents(out var assetName, out var events);
                string eventScript = null;
                foreach(var key in events.Keys)
                {
                    if (key.StartsWith(RSVConstants.E_CLEANSED))
                    {
                        eventScript = events[key];
                    }
                }
                if (!String.IsNullOrEmpty(eventScript))
                {
                    if (Game1.player.eventsSeen.Contains(RSVConstants.E_CLEANSED))
                    {
                        return;
                    }
                    UtilFunctions.StartEvent(new Event(eventScript, assetName, RSVConstants.E_CLEANSED), RSVConstants.L_SPIRITREALM, 10, 10);
                }
                else
                {
                    Log.Error("Event not found");
                }
               
            }
        }

        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation != null && e.NewLocation.Name.Equals(RSVConstants.L_SPIRITREALM))
            {
                if (!Game1.player.eventsSeen.Contains(RSVConstants.E_CLEANSED))
                {
                    e.NewLocation.waterColor.Value = new Color(250, 0, 100, 120);
                }
                else
                {
                    e.NewLocation.waterColor.Value = new Color(35, 214, 213, 120);
                }

                removeCorruptedFireTiles();

                //e.NewLocation.waterColor.Value = new Color(35, 214, 213, 120);
                if (!IsRenderingFog)
                {
                    IsRenderingFog = true;
                    //Helper.Events.Display.RenderedWorld += DrawFog;
                    FogPosition = new Vector2(0f);
                }
            }
            else if(IsRenderingFog)
            {
                IsRenderingFog = false;
                //Helper.Events.Display.RenderedWorld -= DrawFog;
            }
        }


        static Color color = Color.White * 0.4f;
        private static void DrawFog(object sender, RenderedWorldEventArgs e)
        {

            float TextureSize = 512f;
            //I've no idea what Im doing
            FogPosition -= Game1.getMostRecentViewportMotion();
            FogPosition.X %= TextureSize;
            FogPosition.Y %= TextureSize;
            for (float x = -1000f * Game1.options.zoomLevel + FogPosition.X; x < (float)Game1.graphics.GraphicsDevice.Viewport.Width + TextureSize; x += TextureSize)
            {
                for (float y = -TextureSize + FogPosition.Y; y < (float)(Game1.graphics.GraphicsDevice.Viewport.Height + 128); y += TextureSize)
                {
                    e.SpriteBatch.Draw(FogTexture, new Vector2(x, y), null, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                }
            }
        }

        internal static bool RSVWarp(GameLocation location, string[] arg2, Farmer farmer, Point point)
        {
            if (arg2.Length != 4)
            {
                Log.Error($"Error in TileAction {arg2} on tile {point} in {Game1.currentLocation.Name}");
                return false;
            }
            if (!(int.TryParse(arg2[2], out int xCoord) && int.TryParse(arg2[3], out int yCoord))){
                Log.Error($"Error in TileAction {arg2} on tile {point} in {Game1.currentLocation.Name}. Couldnt parse coordinates.");
                return false;
            }


            Farmer who = Game1.player;
            if(Game1.random.NextDouble() < 0.02)
            {
                who.currentLocation.playSound("cow");                
            }
            else
            {
                who.currentLocation.playSound("wand");
            }
            Game1.displayFarmer = false;
            Game1.player.temporarilyInvincible = true;
            Game1.player.temporaryInvincibilityTimer = -2000;
            Game1.player.freezePause = 1000;
            if (arg2[1].Equals(Game1.currentLocation.Name))
            {
                Game1.delayedActions.Add(new DelayedAction(1000, delegate
                    { WarpFarmerOnSameMap(xCoord, yCoord); }));
            }
            else
            {
                Game1.flashAlpha = 1f;
                DelayedAction.fadeAfterDelay(delegate { WarpFarmerToDifferentMap(arg2[1], xCoord, yCoord); }, 1000);
            }

            //draw stuff
            var boundingBox = Game1.player.GetBoundingBox();
            var warpEffect = new TemporaryAnimatedSprite("LooseSprites\\RSVWarp", new Rectangle(0, 0, 16, 32), 100f, 10, 1, new Vector2(boundingBox.X - 8f, boundingBox.Y-90f), false, false, ((yCoord + 0.1f) * 64f) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
            {
                delayBeforeAnimationStart = 0,
                drawAboveAlwaysFront = false
            };


            var multiplayer = Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            multiplayer.broadcastSprites(who.currentLocation, warpEffect);
            return true;
        }

        static void WarpFarmerToDifferentMap(string LocationName, int X, int Y)
        { 
            Game1.warpFarmer(LocationName, X, Y, flip: false);
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
        }

        static void WarpFarmerOnSameMap(int X, int Y)
        {
            Game1.player.position.Value = new Vector2(X, Y) * 64f - new Vector2(0f, Game1.player.Sprite.getHeight() - 48);
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
        }
    }
}

