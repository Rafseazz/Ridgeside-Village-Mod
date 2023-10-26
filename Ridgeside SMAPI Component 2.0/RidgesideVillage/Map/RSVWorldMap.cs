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
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework.Input;

namespace RidgesideVillage
{
    internal class RSVWorldMap:IClickableMenu
    {


        static string MapPath = PathUtilities.NormalizeAssetName("LooseSprites/RSVWorldMap");
        static IModHelper Helper;

        Texture2D image;
        MapData MapData;
        WorldMapAreas NPCLocationData;
        NPCMarker farmerMarker;
        Rectangle MapRectangle;
        Vector2 TopLeft;
        Dictionary<string, Texture2D> NameTexturePairs;

        //Radius for which hovername appears for villagers in pixels
        const int Radius = 25;

        internal static void Setup(IModHelper helper)
        {
            Helper = helper;
            Helper.Events.Display.MenuChanged += OnMenuChanged;
        }

        private static void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if(e.NewMenu is GameMenu gameMenu)
            {
                if(gameMenu.currentTab == GameMenu.mapTab && Game1.currentLocation.Name.StartsWith("Custom_Ridgeside"))
                {
                    Open(gameMenu);
                }
            }
        }

        internal static void Open(IClickableMenu gameMenu)
        {
            if (gameMenu is GameMenu menu)
            {
                Texture2D image = Helper.GameContent.Load<Texture2D>(MapPath);
                Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen((int)(image.Width), (int)(image.Height));

                var mapMenu = new RSVWorldMap((int)topLeft.X, (int)topLeft.Y, image);
                menu.SetChildMenu(mapMenu);

            }
        }
        internal RSVWorldMap(int x, int y, Texture2D mapTexture):
            base(x, y, mapTexture.Width, mapTexture.Height, showUpperRightCloseButton: true)
        {
            //move close button a little
            this.upperRightCloseButton.bounds.X += 18;
            this.upperRightCloseButton.bounds.Y -= 20;
            this.image = mapTexture;

            MapData = new MapData("RSV/RSVWorldMapData");
            this.allClickableComponents = new List<ClickableComponent>();
            foreach (var entry in this.MapData.Locations.Values)
            {
                this.allClickableComponents.Add(new ClickableComponent(entry.AreaRect, entry.Text));
            }
            NPCLocationData = new WorldMapAreas();

            TopLeft = Utility.getTopLeftPositionForCenteringOnScreen((int)(image.Width), (int)(image.Height));
            MapRectangle = new Rectangle((int)TopLeft.X, (int)TopLeft.Y, (int)(image.Width), (int)(image.Height));

            foreach(var entry in NPCLocationData.NPCMarkers)
            {
                entry.MapPosition += TopLeft - new Vector2(16f, 16f); ;
            }
            if (NPCLocationData.Areas.ContainsKey(Game1.currentLocation.Name))
            {
                farmerMarker = new NPCMarker() {
                    DisplayName = Game1.player.displayName,
                    MapPosition = NPCLocationData.Areas[Game1.currentLocation.Name].GetWorldMapPosition(Game1.player.Position) + TopLeft - new Vector2(16f)
                };
            }
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

            foreach (var npcMarker in NPCLocationData.NPCMarkers)
            {
                npcMarker.draw(b);
            }


            if (farmerMarker != null)
            {
                Game1.player.FarmerRenderer.drawMiniPortrat(b, farmerMarker.MapPosition, 0.5f, 2f, 2, Game1.player);
            }

            bool drawPeopleOnAreaHover = true;

            //check if people have to be drawn
            var mousePos = Game1.getMousePosition(true);
            //16 offset because markersCoordinates are top-left
            int x = mousePos.X - 16;
            int y = mousePos.Y - 16;
            StringBuilder Names = new StringBuilder();
            List<string> NPCsToShow = new List<string>();
            foreach (var marker in NPCLocationData.NPCMarkers)
            {
                if (Math.Abs(marker.MapPosition.X - x) < Radius && Math.Abs(marker.MapPosition.Y - y) < Radius)
                {
                    Names.Append(marker.DisplayName);
                    Names.Append(", ");
                    NPCsToShow.Add(marker.Name);
                }
            }
            if (Names.Length > 2)
            {
                drawPeopleOnAreaHover = false;
                Names.Remove(Names.Length - 2, 2);
                IClickableMenu.drawHoverText(b, Names.ToString(), Game1.smallFont, yOffset: -70);

                int CharPosX = (int)mousePos.X + 40;
                int CharPosY = (int)mousePos.Y + 2 - 70;

                var sourceRect = new Rectangle(0, 0, 16, 20);
                foreach (var characterName in NPCsToShow)
                {
                    if (this.NameTexturePairs.TryGetValue(characterName, out Texture2D texture) && texture != null)
                    {
                        b.Draw(texture, new Vector2(CharPosX, CharPosY - 30), sourceRect, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
                        CharPosX += 50;
                    }
                }
            }


            mousePos = Game1.getMousePosition(true);
            x = mousePos.X -  (int)this.TopLeft.X;
            y = mousePos.Y - (int)this.TopLeft.Y;

            foreach (var entry in this.MapData.Locations.Values)
            {
                if (entry.AreaRect.Contains(x, y))
                {
                    IClickableMenu.drawHoverText(b, entry.Text, Game1.smallFont);
                    int CharPosX = (int)mousePos.X + 40;
                    int CharPosY = (int)mousePos.Y + 2;

                    var sourceRect = new Rectangle(0, 0, 16, 20);
                    var animalSourceRect = new Rectangle(0, 0, 16, 16);
                    if(entry.InhabitantsList == null || !drawPeopleOnAreaHover)
                    {
                        break;
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


            
 

            /*
            foreach (var key in NPCLocationData.Areas.Keys)
            {
                Rectangle targetRectanlge = NPCLocationData.Areas[key].AreaRect;
                targetRectanlge.X += (int)TopLeft.X;
                targetRectanlge.Y += (int)TopLeft.Y;
                b.Draw(Game1.staminaRect, targetRectanlge, Color.Orange * 0.4f);
            }*/



            base.draw(b);
            base.drawMouse(b);

        }

        public override void receiveKeyPress(Keys key)
        {
            switch (key)
            {
                case Keys.Escape:
                case Keys.M:
                    Game1.activeClickableMenu.exitThisMenu();
                    break;
            }
            base.receiveKeyPress(key);
        }

        public override bool overrideSnappyMenuCursorMovementBan()
        {
            return true;
        }

    }
}
