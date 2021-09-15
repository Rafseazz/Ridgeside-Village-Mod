using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace RidgesideVillage
{
    internal class MapData
    {
        public Dictionary<string, MapLocation> Locations { get; set; }

        public MapData()
        {
            Locations = ModEntry.Helper.Content.Load<Dictionary<string, MapLocation>>(PathUtilities.NormalizeAssetName("RSV/RSVMapData"), ContentSource.GameContent);

            foreach (var entry in this.Locations.Values)
            {
                if (!String.IsNullOrEmpty(entry.Inhabitants))
                {
                    entry.InhabitantsList = entry.Inhabitants.Split('/').Select(name => name.Trim());
                }

                entry.AreaRect = entry.Area.AsRect();
            }
        }

    }

    public class MapLocation
    {
        public JsonRectangle Area { get; set; }
        internal Rectangle AreaRect;
        public string Inhabitants { get; set; }
        internal IEnumerable<string> InhabitantsList;
        public string Text { get; set; }

    }

    public struct JsonRectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public JsonRectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>Creates an equivalent rectangle instance.</summary>
        /// <returns>A new rectangle matching this instance.</returns>
        public Rectangle AsRect() { return new Rectangle(X, Y, Width, Height); }
    }


}
