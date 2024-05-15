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
    internal class MapMenu:IClickableMenu
    {


        static string MapPath = PathUtilities.NormalizeAssetName("LooseSprites/RSVMap");
        static IModHelper Helper;

        Texture2D image;
        MapData MapData;
        Rectangle MapRectangle;
        Vector2 TopLeft;
        Dictionary<string, Texture2D> NameTexturePairs;

        internal static void Setup(IModHelper Helper)
        {
            MapMenu.Helper = Helper;
            GameLocation.RegisterTileAction("ShowMap", Open);
        }

        internal static bool Open(GameLocation location, string[] arg2, Farmer farmer, Point point)
        {
            Texture2D image = Helper.GameContent.Load<Texture2D>(MapPath);
            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen((int)(image.Width), (int)(image.Height));
            Game1.activeClickableMenu = new MapMenu((int)topLeft.X, (int)topLeft.Y, image);
            return true;
        }
        internal MapMenu(int x, int y, Texture2D mapTexture):
            base(x, y, mapTexture.Width, mapTexture.Height, showUpperRightCloseButton: true)
        {
            //move close button a little
            this.upperRightCloseButton.bounds.X += 18;
            this.upperRightCloseButton.bounds.Y -= 20;
            this.image = mapTexture;

            MapData = new MapData();

            TopLeft = Utility.getTopLeftPositionForCenteringOnScreen((int)(image.Width), (int)(image.Height));
            MapRectangle = new Rectangle((int)TopLeft.X, (int)TopLeft.Y, (int)(image.Width), (int)(image.Height));

            //setup textures
            NameTexturePairs = new Dictionary<string, Texture2D>();
            foreach(var entry in this.MapData.Locations.Values)
            {
                if(entry.InhabitantsList == null)
                {
                    continue;
                }

                foreach(var inhabitant in entry.InhabitantsList)
                {
                    if (this.NameTexturePairs.ContainsKey(inhabitant))
                    {
                        continue;
                    }
                    if (inhabitant.StartsWith("A:"))
                    {
                        NameTexturePairs[inhabitant] = Game1.content.Load<Texture2D>("Animals\\" + inhabitant.Substring(2));
                    }
                    else
                    {                        
                        //skip married and unsocial characters
                        var npc = Game1.getCharacterFromName(inhabitant);
                        if(npc == null || (npc.isMarried() || !npc.CanSocialize))
                        {
                            Log.Trace($"excluded {inhabitant} married: {npc.isMarried()}, canSocialize: {npc.CanSocialize}");
                            continue;
                        }
                        this.NameTexturePairs[inhabitant] = Game1.content.Load<Texture2D>("Characters\\" + inhabitant);
                    }
                }
            }


        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
            Game1.DrawBox(MapRectangle.X, MapRectangle.Y, MapRectangle.Width, MapRectangle.Height);
           
            b.Draw(image, this.TopLeft, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            var mousePos = Game1.getMousePosition(true);
            int x = mousePos.X -  (int)this.TopLeft.X;
            int y = mousePos.Y - (int)this.TopLeft.Y;

            foreach (var entry in this.MapData.Locations.Values)
            {
                if (entry.AreaRect.Contains(x, y))
                {
                    IClickableMenu.drawHoverText(b, entry.Text, Game1.smallFont);
                    int CharPosX = (int)mousePos.X + 40;
                    int CharPosY = (int)mousePos.Y + 2;

                    var sourceRect = new Rectangle(0, 0, 16, 20);
                    var animalSourceRect = new Rectangle(0, 0, 16, 16);
                    if(entry.InhabitantsList == null)
                    {
                        continue;
                    }
                    foreach (var characterName in entry.InhabitantsList)
                    {
                        if(this.NameTexturePairs.TryGetValue(characterName, out Texture2D texture) && texture != null)
                        {
                            if (characterName.StartsWith("A:"))
                            {
                                b.Draw(texture, new Vector2(CharPosX, CharPosY), animalSourceRect, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.5f);
                                CharPosX += 36;
                            }
                            else
                            {
                                b.Draw(texture, new Vector2(CharPosX, CharPosY - 30), sourceRect, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
                                CharPosX += 50;
                            }
                        }
                    }
                    break;
                }
            }
            base.draw(b);
            base.drawMouse(b);

        }

        public override bool overrideSnappyMenuCursorMovementBan()
        {
            return true;
        }

    }
}
