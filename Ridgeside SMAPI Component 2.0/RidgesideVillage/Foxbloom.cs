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

namespace RidgesideVillage
{
    internal static class Foxbloom
    {
        static List<Vector2> spawn_spots;
        static bool spawned_today = false;

        static IModHelper Helper;
        static IMonitor Monitor;

        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            spawn_spots = new List<Vector2> { new Vector2(84, 126), new Vector2(37, 96), new Vector2(18, 76),
                new Vector2(146, 99), new Vector2(108, 58), new Vector2(118, 44), new Vector2(58, 11) };

            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.Player.Warped += OnWarped;
        }

        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            Game1.getLocationFromName(RSVConstants.L_FOREST).modData["RSV_foxbloomSpawned"] = "false";
            spawned_today = false;
        }

        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            if ((!CustomCPTokens.FoxbloomCanSpawn(e.NewLocation, spawned_today)) || e.NewLocation.modData["RSV_foxbloomSpawned"] == "true")
            {
                //Log.Trace("RSV: Not spawning Foxbloom.");
                return;
            }

            Random random = new();
            Vector2 spawn_spot = spawn_spots.ElementAt(random.Next(0, 7));
            try
            {
                UtilFunctions.SpawnForage(RSVConstants.IFOXBLOOM, e.NewLocation, spawn_spot, true);
                Log.Trace("RSV: Foxbloom spawned as forage.");
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
