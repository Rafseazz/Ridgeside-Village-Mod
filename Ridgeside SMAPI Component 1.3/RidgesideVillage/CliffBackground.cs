using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;

namespace RidgesideVillage
{
    internal class CliffBackground
    {
        static IModHelper Helper => ModEntry.Helper;
        static IMonitor Monitor => ModEntry.ModMonitor;

        const string cliffName = "Custom_Ridgeside_RSVCliff";

        int mapWidth;
        int mapHeight;
        int centerX;
        int centerY;
        Point MapCenter;
        Point textureCenter;
        int scaleFactor = 2;

        static Texture2D texture;
        static Texture2D skyTexture;
        private bool isSubscribed = false;
        internal CliffBackground() {
            Helper.Events.Player.Warped += OnWarped;

            texture = Helper.Content.Load<Texture2D>("assets\\mountains.png");
            skyTexture = Helper.Content.Load<Texture2D>("assets\\sky.png");
            textureCenter = new Point(texture.Width / 2, texture.Height / 2);

        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.OldLocation.Name.Equals(cliffName))
            {
                if (!e.NewLocation.Name.Equals(cliffName))
                {
                    Helper.Events.Display.RenderingWorld -= OnRenderingWorld;
                    this.isSubscribed = false;
                }
            }
            else if (e.NewLocation.Name.Equals(cliffName) && !this.isSubscribed)
            {
                Helper.Events.Display.RenderingWorld += OnRenderingWorld;
                this.isSubscribed = true;
                this.setup();
            }
        }

        private void setup()
        {
            GameLocation location = Game1.getLocationFromName(cliffName);
            mapHeight = location.Map.DisplayHeight;
            mapWidth = location.Map.DisplayWidth;
            centerX = mapWidth / 2;
            centerY = mapHeight / 2;
            MapCenter = new Point(mapHeight / 2, mapWidth / 2);
            Monitor.Log($"height {mapHeight} width {mapWidth}");

        }

        private void OnRenderingWorld(object sender, RenderingWorldEventArgs e)
        {
            SpriteBatch spriteBatch = e.SpriteBatch;
            Rectangle viewPort = Game1.viewport.ToXna();
            Vector2 origin = new Vector2(-viewPort.Center.X / 8, -viewPort.Center.Y / 8) * Game1.options.zoomLevel;
            float scale = 4f;
            spriteBatch.Draw(skyTexture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
            spriteBatch.Draw(texture, origin, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
            //debug warp custom_ridgeside_rsvcliff
        }
    }


}
