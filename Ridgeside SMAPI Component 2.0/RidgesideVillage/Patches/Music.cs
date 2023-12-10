using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewModdingAPI.Events;

namespace RidgesideVillage
{
    internal static class Music
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }

        static Dictionary<string, string> CueToSongMap;


        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            Log.Trace($"Applying Harmony Patch \"{nameof(Music)}\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getSongTitleFromCueName)),
                prefix: new HarmonyMethod(typeof(Music), nameof(getSongTitleFromCueName_prefix))
            );

            CueToSongMap = Helper.ModContent.Load<Dictionary<string, string>>("assets/MusicDisplayNames.json");

            Helper.Events.Player.Warped += OnWarped;
            Helper.Events.GameLoop.TimeChanged += OnTimeChanged;

        }

        internal static bool getSongTitleFromCueName_prefix(string cueName, ref string __result)
        {
            if (CueToSongMap.TryGetValue(cueName, out var title))
            {
                __result = title;
                return false;
            }
            return true;
        }

        internal static void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation is null) return;
            if (e.NewLocation.Name.Equals(RSVConstants.L_HOTEL) && JuneAtPiano())
                Game1.changeMusicTrack("JunePiano");
            else if (!e.NewLocation.Name.Equals(RSVConstants.L_HOTEL) && Game1.getMusicTrackName() == "JunePiano")
            {
                Game1.updateMusic();
                if (Game1.getMusicTrackName() == "JunePiano")
                    Game1.changeMusicTrack("none");
            }
        }

        internal static void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (Game1.currentLocation is null) return;
            if (Game1.currentLocation.Name.Equals(RSVConstants.L_HOTEL) && JuneAtPiano() && Game1.getMusicTrackName() != "JunePiano")
                Game1.changeMusicTrack("JunePiano");
            if (Game1.currentLocation.Name.Equals(RSVConstants.L_HOTEL) && !JuneAtPiano() && Game1.getMusicTrackName() == "JunePiano")
                Game1.changeMusicTrack("none");
        }

        internal static bool JuneAtPiano()
        {
            NPC June = Game1.getCharacterFromName("June");
            if (!June.currentLocation.Name.Equals(RSVConstants.L_HOTEL)) return false;
            Vector2 pos = June.Tile;
            if ((pos.X == 13) && (pos.Y == 14)) return true;
            return false;
        }
    }
}
