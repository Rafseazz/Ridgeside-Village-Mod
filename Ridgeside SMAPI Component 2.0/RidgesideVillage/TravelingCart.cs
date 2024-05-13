using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace RidgesideVillage
{
    internal static class TravelingCart
    {
        static IModHelper Helper;
        static IMonitor Monitor;
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            Helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.dayOfMonth % 7 != 0 && Game1.dayOfMonth % 7 % 3 == 0)
            {
                GameLocation greenhouse = Game1.getLocationFromName("Custom_Ridgeside_RSVTheHike");
                var features = greenhouse.terrainFeatures;
                foreach (var pair in features.Pairs)
                {
                    Vector2 tile = pair.Value.Tile;
                    if (pair.Value is Grass grass && (tile.X >= 48 && tile.X <= 56) && (tile.Y == 23 || tile.Y == 24))
                    {
                        features.Remove(pair.Key);
                    }
                }
            }
        }
    }
}
