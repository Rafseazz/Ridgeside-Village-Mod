using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.Menus;
using StardewValley;
using StardewModdingAPI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{
    internal class ImageMenu:IClickableMenu
    {


        const string PictureFolder = "RSV/Pictures";

        Texture2D image;
        Rectangle targetRectangle;
        Vector2 topLeft;
        float scale;

        internal static void Setup()
        {
            TileActionHandler.RegisterTileAction("ShowImage", Open);
        }

        internal static void Open(string tileAction)
        {
            //parse string
            //has form "ShowImage "path/to/file" [scale]"
            var split = tileAction.Split('"');
            string parameters;
            if (split.Length < 3)
            {
                Log.Debug($"Error in {tileAction}");
                return;
            }
            string path = split[1];
            float scale = 1f;
            if(split[2].Length > 0)
            {
                var parameterSplit = split[2].Split(' ');
                if(!float.TryParse(parameterSplit[1],out scale))
                {
                    Log.Debug("Failed parsing sale {parameterSplit[0]}, showing in 4f");
                    scale = 4f;
                }

            }
            Log.Debug($"{tileAction}");
            Texture2D image = ModEntry.Helper.Content.Load<Texture2D>(PathUtilities.NormalizePath(path), ContentSource.GameContent);
            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen((int)(image.Width *scale), (int)(image.Height * scale));
            Game1.activeClickableMenu = new ImageMenu((int)topLeft.X, (int)topLeft.Y, scale, image);
            
        }
        internal ImageMenu(int x, int y, float scale, Texture2D image):base(x, y, (int) (image.Width * scale), (int) (image.Height * scale), true)
        {
            this.image = image;
            this.scale = scale;
            //move close button a little
            this.upperRightCloseButton.bounds.X += 18;
            this.upperRightCloseButton.bounds.Y -= 20;

            topLeft = new Vector2(x, y);
            targetRectangle = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(image.Width * scale), (int)(image.Height * scale));
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
            Game1.DrawBox(targetRectangle.X, targetRectangle.Y, targetRectangle.Width, targetRectangle.Height);
           
            b.Draw(image, this.topLeft, null, Color.White, 0f, Vector2.Zero, this.scale, SpriteEffects.None, 1f);
            base.draw(b);
            base.drawMouse(b);

        }
    }
}
