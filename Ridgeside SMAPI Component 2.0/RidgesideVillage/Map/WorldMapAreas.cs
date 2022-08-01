using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;

namespace RidgesideVillage
{
    /// <summary>
    /// class for the NPCLocations stuff on the RSV world map
    /// </summary>
    internal class WorldMapAreas
    {
        const string path = "assets/RSVWorldMapAreas.json";
        public Dictionary<string, MapArea> Areas { get; set; }
        public List<NPCMarker> NPCMarkers { get; set; }

        public WorldMapAreas()
        {
            RSVWorldMapAreasModel DataModel = ModEntry.Helper.ModContent.Load<RSVWorldMapAreasModel>(PathUtilities.NormalizePath(path));
            MapArea[] AreaList = DataModel.AreaList;
            Areas = new Dictionary<string, MapArea>();
            //initialize all areas
            foreach (var entry in AreaList)
            {
                var location = Game1.getLocationFromName(entry.MapName);
                if (location == null)
                {
                    continue;
                }
                var map = location.Map;
                entry.MapSize = new Vector2(map.GetLayer("Back").LayerWidth, map.GetLayer("Back").LayerHeight);
                entry.AreaRect = entry.Area.AsRect();

                Areas.Add(entry.MapName, entry);
            }

            NPCMarkers = new List<NPCMarker>();
            if (ModEntry.Config.ShowVillagersOnMap)
            {
                foreach (var npc in Utility.getAllCharacters())
                {
                    if (npc.CanSocialize && Areas.TryGetValue(npc.currentLocation.Name, out MapArea currentArea))
                    {
                        int yOffset = 0;
                        if (DataModel.DrawYOffsets.ContainsKey(npc.Name))
                        {
                            yOffset = DataModel.DrawYOffsets[npc.Name];
                        }
                        NPCMarkers.Add(new NPCMarker(npc, currentArea, yOffset));
                    }
                }
            }
           
        }
    }

    public class MapArea
    {
        public JsonRectangle Area { get; set; }
        public bool IsOutdoors { get; set; }
        internal Rectangle AreaRect;
        public string MapName { get; set; }
        public Vector2 MapSize { get; set; }


        public Vector2 GetWorldMapPosition(Vector2 localPosition)
        {
            if (!IsOutdoors)
            {
                return new Vector2(this.Area.X, this.Area.Y);
            }
            else
            {
                return new Vector2(this.Area.X + (localPosition.X/64f / MapSize.X * this.Area.Width), this.Area.Y + ((localPosition.Y/64f / MapSize.Y * this.Area.Height)));
            }
        }
    }

    public class NPCMarker
    {
        public Vector2 MapPosition;
        public string DisplayName;
        public string Name;
        public Texture2D Texture;
        public Rectangle SourceRectangle;
        
        public NPCMarker() {}
        public NPCMarker(NPC npc, MapArea mapArea, int yOffset = 0)
        {
            this.Name = npc.Name;
            this.DisplayName = npc.displayName;
            this.Texture = Game1.content.Load<Texture2D>(PathUtilities.NormalizeAssetName($"Characters//{npc.Name}"));
            this.MapPosition = mapArea.GetWorldMapPosition(npc.Position);
            this.SourceRectangle = new Rectangle(0, yOffset, 16, 16);
        }
        public void draw (SpriteBatch b)
        {
            b.Draw(this.Texture, this.MapPosition, this.SourceRectangle, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.5f);
        }

    }

    internal class RSVWorldMapAreasModel
    {
        public MapArea[] AreaList { get; set; }
        public Dictionary<string, int> DrawYOffsets {get; set;}
    }

}
