using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;

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

        public static float PingPong(float t, float length)
        {
            float f = t % (length * 2);
            return f > length ? 2 * length - f : f;
        }
    }
    

}
