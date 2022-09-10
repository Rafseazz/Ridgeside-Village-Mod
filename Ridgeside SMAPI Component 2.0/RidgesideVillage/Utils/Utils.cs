using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using Netcode;

namespace RidgesideVillage
{
    internal static class Log
    {
        internal static void Error(string msg) => ModEntry.ModMonitor.Log(msg, StardewModdingAPI.LogLevel.Error);
        internal static void Alert(string msg) => ModEntry.ModMonitor.Log(msg, StardewModdingAPI.LogLevel.Alert);
        internal static void Warn(string msg) => ModEntry.ModMonitor.Log(msg, StardewModdingAPI.LogLevel.Warn);
        internal static void Info(string msg) => ModEntry.ModMonitor.Log(msg, StardewModdingAPI.LogLevel.Info);
        internal static void Debug(string msg) => ModEntry.ModMonitor.Log(msg, StardewModdingAPI.LogLevel.Debug);
        internal static void Trace(string msg) => ModEntry.ModMonitor.Log(msg, StardewModdingAPI.LogLevel.Trace);
        internal static void Verbose(string msg) => ModEntry.ModMonitor.VerboseLog(msg);

    }

    internal static class UtilFunctions
    {
        //3 hours of trying and tears :c
        internal static void StartEvent(StardewValley.Event EventObj, string locationName, int x, int y){
            if (Game1.currentLocation.Name.Equals(locationName))
            {
                Game1.delayedActions.Add(new DelayedAction(500, delegate
                 {
                     Game1.currentLocation.startEvent(EventObj);
                 }));
                Game1.fadeScreenToBlack();
            }
            else
            {

                LocationRequest warpRequest = Game1.getLocationRequest(locationName);
                warpRequest.OnLoad += delegate
                {
                    Game1.currentLocation.currentEvent = EventObj;
                };
                Game1.warpFarmer(warpRequest, x, y, Game1.player.FacingDirection);
            }
            
        }

        /// <summary>
        /// Yields all tiles around a specific tile.
        /// </summary>
        /// <param name="tile">Vector2 location of tile.</param>
        /// <param name="radius">A radius to search in.</param>
        /// <returns>All tiles within radius.</returns>
        /// <remarks>This actually returns a square, not a circle.</remarks>
        public static IEnumerable<Point> YieldSurroundingTiles(Vector2 tile, int radius = 1)
        {
            int x = (int)tile.X;
            int y = (int)tile.Y;
            for (int xdiff = -radius; xdiff <= radius; xdiff++)
            {
                for (int ydiff = -radius; ydiff <= radius; ydiff++)
                {
                    yield return new Point(x + xdiff, y + ydiff);
                }
            }
        }

        /// <summary>
        /// Yields an iterator over all tiles on a location.
        /// </summary>
        /// <param name="location">Location to check.</param>
        /// <returns>IEnumerable of all tiles.</returns>
        public static IEnumerable<Vector2> YieldAllTiles(GameLocation location)
        {
            for (int x = 0; x < location.Map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < location.Map.Layers[0].LayerHeight; y++)
                {
                    yield return new Vector2(x, y);
                }
            }
        }

        /// <summary>
        /// Sort strings, taking into account CultureInfo of currently selected language.
        /// </summary>
        /// <param name="enumerable">IEnumerable of strings to sort.</param>
        /// <returns>A sorted list of strings.</returns>
        public static List<string> ContextSort(IEnumerable<string> enumerable)
        {
            List<string> outputlist = enumerable.ToList();
            outputlist.Sort(GetCurrentLanguageComparer(ignoreCase: true));
            return outputlist;
        }

        /// <summary>
        /// Returns a StringComparer for the current language the player is using.
        /// </summary>
        /// <param name="ignoreCase">Whether or not to ignore case.</param>
        /// <returns>A string comparer.</returns>
        public static StringComparer GetCurrentLanguageComparer(bool ignoreCase = false)
            => StringComparer.Create(Game1.content.CurrentCulture, ignoreCase);

        public static IEnumerable<NPC> GetBirthdayNPCs(SDate day)
        {
            foreach (NPC npc in Utility.getAllCharacters())
            {
                if (npc.isBirthday(day.Season, day.Day))
                {
                    yield return npc;
                }
            }
        }

        /// <summary>Generates a object from an index and places it on the specified map and tile.</summary>
        /// <param name="index">The parent sheet index (a.k.a. object ID) of the object type to spawn.</param>
        /// <param name="location">The GameLocation where the forage should be spawned.</param>
        /// <param name="tile">The x/y coordinates of the tile where the forage should be spawned.</param>
        /// <param name="destroyOvernight">Whether the object should be destroyed overnight.</param>
        public static bool SpawnForage(int index, GameLocation location, Vector2 tile, bool destroyOvernight)
        {
            StardewValley.Object forageObj;
            forageObj = new StardewValley.Object(tile, index, null, false, true, false, true); //generate the object (use the constructor that allows pickup)
            if (destroyOvernight)
                forageObj.destroyOvernight = true;

            Log.Verbose($"Spawning forage object for RSV. Type: {forageObj.DisplayName}. Location: {tile.X}, {tile.Y} ({location.Name}).");
            return location.dropObject(forageObj, tile * 64f, Game1.viewport, true, null); //attempt to place the object and return success/failure
        }

        public static int GetWeather(GameLocation location)
        {
            // special case: day events override weather in the valley
            if (!(location is IslandLocation))
            {
                if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) || (SaveGame.loaded?.weddingToday ?? Game1.weddingToday))
                    return Game1.weather_sunny;
            }

            // get from weather data
            LocationWeather model = Game1.netWorldState.Value.GetWeatherForLocation(location.GetLocationContext());
            if (model != null)
            {
                if (model.isSnowing.Value)
                    return Game1.weather_snow;
                if (model.isRaining.Value)
                    return model.isLightning.Value ? Game1.weather_lightning : Game1.weather_rain;
                if (model.isDebrisWeather.Value)
                    return Game1.weather_debris;
            }
            return Game1.weather_sunny;
        }

        public static bool IsSomeoneHere(int x, int y, int w, int h)
        {
            NetCollection<NPC> characters = Game1.currentLocation.characters;

            bool isSomeoneHere = false;
            foreach (NPC character in characters)
            {
                //Tiles in Rectangle(14, 12, 3, 2) are behind the counter
                Rectangle behindCounterArea = new Rectangle(x * 64, y * 64, w * 64, h * 64);
                isSomeoneHere = isSomeoneHere || behindCounterArea.Contains((int)character.Position.X, (int)character.Position.Y);
            }
            return isSomeoneHere;
        }

        public static void TryAddQuest(int id)
        {
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                farmer.addQuest(id);
            }
        }

        public static void TryRemoveQuest(int id)
        {
            if (!Game1.player.hasQuest(id))
                return;

            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                if (farmer.hasQuest(id))
                    farmer.removeQuest(id);
            }

        }

        public static void TryCompleteQuest(int id)
        {
            if (!Game1.player.hasQuest(id))
                return;

            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                if (farmer.hasQuest(id))
                    farmer.completeQuest(id);
            }
        }
    }  

}
