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
        static IModHelper Helper;

        Texture2D image;
        Rectangle targetRectangle;
        Vector2 topLeft;
        float scale;

        internal static void Setup(IModHelper Helper)
        {
            ImageMenu.Helper = Helper;
            TileActionHandler.RegisterTileAction("ShowImage", Open);
        }

        internal static void Open(string tileAction, Vector2 position)
        {
            //parse string
            //has form "ShowImage "path/to/file" scale [i18nkey]"

            var quoteSplit = tileAction.Split('"');
            if (quoteSplit.Length < 3)
            {
                Log.Debug($"Error in {tileAction}. (Perhaps you missed a \"?");
                return;
            }

            string path = quoteSplit[1];

            var split = quoteSplit[2].Trim().Split(' ');

            float scale = 1f;

            if(!float.TryParse(split[0],out scale))
            {
                scale = 4f;
            }

            bool showTextBefore = false;
            if (split.Length >= 2  && split[1].Length > 0)
            {
                Log.Debug("has i18n key argument");
                var i18n_key = split[1];
                string text = Helper.Translation.Get(i18n_key);
                if (text != null && !text.StartsWith("(no"))
                {
                    showTextBefore = true;
                    string tileActionShortened = $"ShowImage \"{path}\" {scale}";
                    Game1.activeClickableMenu = new DialogueBox(text);
                    Game1.afterDialogues = delegate
                    {
                        ImageMenu.Open(tileActionShortened, position);
                    };
                }
                else
                {
                    Log.Debug($"No translation found for {i18n_key}: {text}");
                }
            }
            if (!showTextBefore)
            {
                Texture2D image = ModEntry.Helper.GameContent.Load<Texture2D>(PathUtilities.NormalizePath(path));
                Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen((int)(image.Width * scale), (int)(image.Height * scale));
                Game1.activeClickableMenu = new ImageMenu((int)topLeft.X, (int)topLeft.Y, scale, image);
            }
            
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
