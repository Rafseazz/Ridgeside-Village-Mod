using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Network;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{
    internal static class Foxbloom
    {
        const string FOXBLOOM = "Foxbloom";
        const string FOXMASK = "Relic Fox Mask";
        static List<Vector2> spawn_spots = new();
        static bool spawned_today = false;

        static IModHelper Helper;
        static IMonitor Monitor;

        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            var list = new List<Vector2> { new Vector2(84, 126), new Vector2(37, 96), new Vector2(18, 76),
                new Vector2(146, 99), new Vector2(108, 58), new Vector2(118, 44), new Vector2(58, 11) };
            spawn_spots = list;

            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.Player.Warped += OnWarped;
        }

        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            Game1.getLocationFromName("Custom_Ridgeside_RidgeForest").modData["RSV_foxbloomSpawned"] = "false";
        }

        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            if ((e.NewLocation.Name != "Custom_Ridgeside_RidgeForest") || spawned_today)
                return;
            if ((Game1.dayOfMonth != CustomCPTokens.FoxbloomDay) || (UtilFunctions.GetWeather(e.NewLocation) != Game1.weather_sunny))
                return;
            if (!Game1.player.hasItemInInventoryNamed(FOXMASK))
                return;
            if (e.NewLocation.modData["RSV_foxbloomSpawned"] == "true")
                return;

            Random random = new();
            Vector2 spawn_spot = spawn_spots.ElementAt(random.Next(0, 7));
            int FOXBLOOMID = ExternalAPIs.JA.GetObjectId(FOXBLOOM);
            try
            {
                UtilFunctions.SpawnForage(FOXBLOOMID, e.NewLocation, spawn_spot, true);
                spawned_today = true;
                e.NewLocation.modData["RSV_foxbloomSpawned"] = "true";
            }
            catch(Exception ex)
            {
                Log.Error($"RSV: Error spawning Foxbloom at {spawn_spot.X}, {spawn_spot.Y}\n{ex}");
            }
        }

    }

  
}
