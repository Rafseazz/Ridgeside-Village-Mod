using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewModdingAPI.Events;
using StardewModdingAPI;

//This Background is based off of spacechase0's SpaceBackground, which can be found here:
//https://github.com/spacechase0/StardewValleyMods/blob/develop/MoonMisadventures/Game/SpaceBackground.cs
//Thank you space for permission!

namespace RidgesideVillage
{
    public class TortsBackground : Background
    {
        static IModHelper Helper;
        static IMonitor Monitor;

        private Vector2 offset = Vector2.Zero;
        private Rectangle starTexRect = new Rectangle(0, 1453, 639, 195);
        //private TemporaryAnimatedSprite torts;
        private Texture2D torts;
        private static Vector2 torts_position;
        private readonly Color[] fromColors = new[]
                {
                    new Color( 190, 150, 255 ),
                    new Color( 250, 255, 170 ),
                    new Color( 150, 150, 255 )
                };
        private readonly Color[] toColors = new[]
                {
                    new Color( 255, 255, 255 ),
                    new Color( 255, 255, 255 ),
                    new Color( 255, 255, 255 )
                };

        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            Helper.Events.Display.RenderingWorld += OnRenderingWorld;
            Helper.Events.Display.RenderedWorld += OnRenderedWorld;
        }

        public TortsBackground()
        : base(new Color(0, 0, 12), false)
        {
            Log.Trace($"RSV: Creating Torts bg");
            torts = Helper.ModContent.Load<Texture2D>("assets/Torts.png");
            torts_position = new Vector2(10f, 6f) * Game1.tileSize;
            /*
            torts = new TemporaryAnimatedSprite(Helper.Content.GetActualAssetKey("assets/Torts.png"), new Rectangle(0, 0, 144, 112), new Vector2(11f, 5f) * Game1.tileSize, false, 0, Color.White)
            {
                scale = Game1.pixelZoom,
                animationLength = 16 * 16,
                interval = 250,
                totalNumberOfLoops = 99999,
            };
            */
        }

        public void Update(xTile.Dimensions.Rectangle viewport)
        {
            float Gametime = (float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
            UpdateTortsPosition(Gametime);
            // For TAS: torts.update(Game1.currentGameTime);
        }

        public void Draw(SpriteBatch b)
        {
            try
            {
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, depthStencilState: BgUtils.StencilBrighten);

                Rectangle display = new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height);
                b.Draw(Game1.staminaRect, display, Game1.staminaRect.Bounds, this.c, 0f, Vector2.Zero, SpriteEffects.None, 0f);

                Vector2[] posMods = new[]
                {
                    new Vector2( 0, 0 ),
                    new Vector2( starTexRect.Width / 3 * 1, starTexRect.Height / 3 * 1 ),
                    new Vector2( starTexRect.Width / 3 * 2, starTexRect.Height / 3 * 2 ),
                };
                float[] posMult = new[] { 0.1f, 0.3f, 0.5f };


                float incrx = Game1.viewport.Width / starTexRect.Width;
                float incry = Game1.viewport.Height / starTexRect.Height;
                float Gametime;

                for (int i = 0; i < 3; ++i)
                {
                    float sx = -(((Game1.viewport.X + offset.X) * posMult[i] + posMods[i].X) % (starTexRect.Width * Game1.pixelZoom));
                    float sy = -(((Game1.viewport.Y + offset.Y) * posMult[i] + posMods[i].Y) % (starTexRect.Height * Game1.pixelZoom));
                    for (int ix = -1; ix <= incrx + 1; ++ix)
                    {
                        for (int iy = -1; iy <= incry + 1; ++iy)
                        {
                            float rx = sx + ix * starTexRect.Width * Game1.pixelZoom;
                            float ry = sy + iy * starTexRect.Height * Game1.pixelZoom;

                            Gametime = (float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
                            Color lerpedColor = Color.Lerp(fromColors[i], toColors[i], MathF.Abs(MathF.Sin(Gametime / 2000 * MathF.PI)));
                            b.Draw(Game1.mouseCursors, new Vector2(rx, ry), starTexRect, lerpedColor, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.001f * i);
                        }
                    }
                }

                /*
                torts.Position = new Vector2(Game1.viewport.Width / 2 - torts.Texture.Width / 16, 100);
                torts.interval = 200;
                torts.draw(b, localPosition: true);
                */

                float multiplier = 1.1111f;
                b.Draw(torts, Game1.GlobalToLocal(Game1.viewport, torts_position), new Rectangle(0, 0, 144, 112), Color.White, 0, Vector2.Zero, Game1.pixelZoom*multiplier, SpriteEffects.None, 1);

                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, depthStencilState: BgUtils.StencilDarken);
            }
            catch (Exception ex)
            {
                Log.Error($"RSV: Error drawing TortsBackground:\n\n{ex}");
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
        }

        private static void OnRenderingWorld(object sender, RenderingWorldEventArgs e)
        {
            if (Game1.background is TortsBackground)
            {
                // This part doesn't do anything normally (https://github.com/MonoGame/MonoGame/issues/5441),
                // but SpriteMaster makes it work. So need this for compatibility.
                if (Game1.graphics.PreferredDepthStencilFormat != DepthFormat.Depth24Stencil8)
                {
                    Game1.graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
                    Game1.graphics.ApplyChanges();
                }

                BgUtils.DefaultStencilOverride = BgUtils.StencilDarken;
                Game1.graphics.GraphicsDevice.Clear(ClearOptions.Stencil, Color.Transparent, 0, 0);
            }
        }

        private static void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            BgUtils.DefaultStencilOverride = null;
        }

        private static void UpdateTortsPosition(float Gametime)
        {
            float horizontalMovement = MathF.Cos(Gametime / 3000 * MathF.PI) / 4;
            float verticalMovement = MathF.Sin(Gametime / 2500 * MathF.PI) / 5;
            torts_position += new Vector2(horizontalMovement, verticalMovement);
        }
    }
}