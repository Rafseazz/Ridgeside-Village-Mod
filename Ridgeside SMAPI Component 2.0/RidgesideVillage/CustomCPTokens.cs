using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Network;
using StardewValley.GameData.Shirts;

namespace RidgesideVillage
    {
    class CustomCPTokens
        {
        internal static IModHelper Helper;
        private readonly IManifest ModManifest;
        public static int FoxbloomDay;

        private ModConfig Config {
            get => ModEntry.Config;
            set => ModEntry.Config = value;
            }

        public CustomCPTokens(IMod mod) {
            Helper = mod.Helper;
            ModManifest = mod.ModManifest;
            }

        public void RegisterTokens() {
            var cp = ExternalAPIs.CP;

            cp.RegisterToken(this.ModManifest, "SpouseGender", () =>
            {
                // or save is currently loading
                if (SaveGame.loaded?.player != null || Context.IsWorldReady)
                {
                    var Spouse = Game1.getCharacterFromName(Game1.player.spouse);
                    if (Spouse != null)
                    {
                        string gender;
                        switch (Spouse.Gender)
                        {
                            case 0:
                                gender = "male";
                                break;
                            case 1:
                                gender = "female";
                                break;
                            default:
                                gender = "undefined";
                                break;
                        }
                        return new[] { gender };
                    }
                }
                // no save loaded (e.g. on the title screen)
                return null;
            });

            cp.RegisterToken(this.ModManifest, "IreneTraveling", () =>
            {
                // or save is currently loading
                if (SaveGame.loaded?.player != null || Context.IsWorldReady)
                {
                    var Spouse = Game1.getCharacterFromName(Game1.player.spouse);
                    if (Spouse != null)
                    {
                        string gender;
                        switch (Spouse.Gender)
                        {
                            case 0:
                                gender = "male";
                                break;
                            case 1:
                                gender = "female";
                                break;
                            default:
                                gender = "undefined";
                                break;
                        }
                        return new[] { gender };
                    }
                }
                // no save loaded (e.g. on the title screen)
                return null;
            });

            cp.RegisterToken(this.ModManifest, "Celebrant", () =>
            {
                // or save is currently loading
                if (SaveGame.loaded?.player != null || Context.IsWorldReady)
                {
                    return new[] { HotelMenu.GetTodaysBirthdayNPC() };
                }
                // no save loaded (e.g. on the title screen)
                return new[] { "" };
            });

            cp.RegisterToken(this.ModManifest, "CelebrantDisplayName", () =>
            {
                // or save is currently loading
                if (SaveGame.loaded?.player != null || Context.IsWorldReady)
                {
                    string NPCName = HotelMenu.GetTodaysBirthdayNPC();
                    NPC npc = Game1.getCharacterFromName(NPCName);
                    if(npc != null)
                    {
                        return new[] { npc.displayName };
                    }
                }
                // no save loaded (e.g. on the title screen)
                return new[] { "" };
            });

            cp.RegisterToken(this.ModManifest, "FoxbloomDay", () => {
                int? randomseed = (int?)Game1.stats?.DaysPlayed;
                if (randomseed is not null)
                {   //Seed the random with a seed that only changes every 28 days
                    Random random = new Random((int)Game1.uniqueIDForThisGame + ((randomseed.Value - 1) / 28));
                    FoxbloomDay = random.Next(1, 5) * 7;
                    return new[] { FoxbloomDay.ToString() };
                }
                return null; //return null for an unready token.
            });

            cp.RegisterToken(this.ModManifest, "ShirtNameFromId", new ShirtName());

            cp.RegisterToken(this.ModManifest, "FoxbloomSpawned", new FoxbloomSpawned());

            cp.RegisterToken(this.ModManifest, "RSVInstallDay", () =>
            {
                if (!Config.ProgressiveStory)
                {
                    return new[] { "0" };
                }
                // save is loading or loaded
                if (Game1.MasterPlayer is not null && Context.IsWorldReady)
                {
                    var FarmModData = Game1.MasterPlayer.modData;
                    const string key = "RSVInstallDay";
                    if (FarmModData.TryGetValue(key, out string day))
                    {
                        return new[] { day };
                    }
                    else
                    {
                        if (Game1.player.eventsSeen.Contains(RSVConstants.E_BUSSTOP_INTRO))
                        {
                            FarmModData[key] = "0";
                            return new[] { "0" };
                        }
                        else
                        {
                            FarmModData[key] = Game1.stats.DaysPlayed.ToString();
                            return new[] { Game1.stats.DaysPlayed.ToString() };
                        }
                    }
                }
                // no save loaded (e.g. on the title screen)
                return null;
            });

            cp.RegisterToken(this.ModManifest, "ZayneWeeklyVisitDays", () =>
            {
                if (Game1.MasterPlayer is not null && Context.IsWorldReady)
                {
                    int? randomseed = (int?)Game1.stats?.DaysPlayed;
                    if (randomseed is not null)
                    {   //Seed the random with a seed that changes weekly
                        Random random = new Random((int)Game1.uniqueIDForThisGame + int.Parse(RSVConstants.E_ZAYNE_INTRO) + ((randomseed.Value - 1) / 7));
                        List<string> visits = GetFestivalDaysAndBday("Zayne");
                        //Log.Debug("RSV: Festival days and birthday for Zayne are " + visits.ToString());
                        if (!visits.Contains("Sunday") && Game1.player.eventsSeen.Contains(RSVConstants.E_ZAYNE_6H))
                        {
                            // Bryle visits every Sunday after 6 heart event
                            visits.Add("Sunday");
                        }
                        List<string> weekdays = new List<string>() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" }.Except(visits).ToList();
                        if (Game1.player.eventsSeen.Contains(RSVConstants.E_ZAYNE_6H))
                        {
                            visits = visits.Concat(weekdays.OrderBy(x => random.Next()).Take(4 - visits.Count)).ToList();
                        }
                        else if (Game1.player.eventsSeen.Contains(RSVConstants.E_ZAYNE_2H))
                        {
                            visits = visits.Concat(weekdays.OrderBy(x => random.Next()).Take(4 - visits.Count)).ToList();
                        }
                        else if (Game1.player.eventsSeen.Contains(RSVConstants.E_ZAYNE_INTRO))
                        {
                            visits = visits.Concat(weekdays.OrderBy(x => random.Next()).Take(3 - visits.Count)).ToList();
                        }
                        if (visits is not null)
                            return new[] { string.Join(",", visits.ToArray()) };
                    }
                }
                return null; //return null for an unready token.
            });

            cp.RegisterToken(this.ModManifest, "BryleWeeklyVisitDays", () => {
                if (Game1.MasterPlayer is not null && Context.IsWorldReady)
                {
                    int? randomseed = (int?)Game1.stats?.DaysPlayed;
                    if (randomseed is not null)
                    {   //Seed the random with a seed that changes weekly
                        Random random = new Random((int)Game1.uniqueIDForThisGame + int.Parse(RSVConstants.E_BRYLE_INTRO) + ((randomseed.Value - 1) / 7));
                        List<string> visits = GetFestivalDaysAndBday("Bryle");
                        if (!visits.Contains("Wednesday") && Game1.dayOfMonth < 8 && Game1.player.eventsSeen.Contains(RSVConstants.E_BRYLE_8H))
                        {
                            // Bryle's Ninja House visit
                            visits.Add("Wednesday");
                        }
                        //Log.Debug("RSV: Festival days and birthday for Bryle are " + visits.ToString());
                        List<string> weekdays = new List<string>() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" }.Except(visits).ToList();
                        if (Game1.player.eventsSeen.Contains(RSVConstants.E_BRYLE_8H))
                        {
                            visits = visits.Concat(weekdays.OrderBy(x => random.Next()).Take(6 - visits.Count)).ToList();
                        }
                        else if (Game1.player.eventsSeen.Contains(RSVConstants.E_BRYLE_INTRO))
                        {
                            visits = visits.Concat(weekdays.OrderBy(x => random.Next()).Take(4 - visits.Count)).ToList();
                        }
                        if (visits is not null)
                            return new[] { string.Join(",", visits.ToArray()) };
                    }
                }
                return null; //return null for an unready token.
            });
        }

        internal class ShirtName
        {
            /*********
            ** Fields
            *********/
            /// <summary>The name of the shirt at the given ID as of the last context update.</summary>
            private IDictionary<string, ShirtData> clothes;

            /*********
            ** Public methods
            *********/
            /****
            ** Metadata
            ****/
            /// <summary>Get whether the token allows input arguments.</summary>
            public bool AllowsInput()
            {
                return true;
            }

            /// <summary>Whether the token may return multiple values for the given input.</summary>
            /// <param name="input">The input arguments, if applicable.</param>
            public bool CanHaveMultipleValues(string input = null)
            {
                return false;
            }

            /****
            ** State
            ****/
            /// <summary>Update the values when the context changes.</summary>
            /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
            public bool UpdateContext()
            {
                var old_clothes = clothes;
                clothes = Helper.GameContent.Load<IDictionary<string, ShirtData>>(PathUtilities.NormalizeAssetName("Data/Shirts"));
                /*
                if (clothes.Equals(old_clothes))
                    Log.Debug("RSV: not updating context for ShirtName");
                else
                    Log.Debug("RSV: time for context update for ShirtName!");
                */
                return !clothes.Equals(old_clothes);
            }

            /// <summary>Get whether the token is available for use.</summary>
            public bool IsReady()
            {
                return (SaveGame.loaded?.player != null || Context.IsWorldReady);
            }

            /// <summary>Get the current values.</summary>
            /// <param name="input">The input arguments, if applicable.</param>
            public IEnumerable<string> GetValues(string input)
            {
                var shirtData = ItemRegistry.GetData("(S)" + input);
                if (!shirtData.IsErrorItem)
                {
                    yield return shirtData.DisplayName;
                }
                yield return "";
            }
        }

        internal class FoxbloomSpawned
        {
            /*********
        ** Fields
        *********/
            /// <summary>Whether or not the Foxbloom has spawned today.</summary>
            static public bool spawned_today = false;


            /*********
            ** Public methods
            *********/
            /****
            ** Metadata
            ****/
            /// <summary>Get whether the token allows input arguments (e.g. an NPC name for a relationship token).</summary>
            public bool AllowsInput()
            {
                return false;
            }

            /// <summary>Whether the token may return multiple values for the given input.</summary>
            /// <param name="input">The input arguments, if applicable.</param>
            public bool CanHaveMultipleValues(string input = null)
            {
                return false;
            }

            /****
            ** State
            ****/
            /// <summary>Update the values when the context changes.</summary>
            /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
            public bool UpdateContext()
            {
                GameLocation here = Game1.currentLocation;
                if (here == null)
                    return false;
                if (!FoxbloomCanSpawn(here, spawned_today))
                    return false;
                Log.Debug("RSV: Foxbloom spawned - Updating context for CP token");
                spawned_today = true;
                return true;
            }

            /// <summary>Get whether the token is available for use.</summary>
            public bool IsReady()
            {
                if (Game1.currentLocation == null)
                    return false;
                return true;
            }

            /// <summary>Get the current values.</summary>
            /// <param name="input">The input arguments, if applicable.</param>
            public IEnumerable<string> GetValues(string input)
            {
                return new[] { spawned_today.ToString() };
            }

        }

        public static bool FoxbloomCanSpawn(GameLocation here, bool spawned_today)
        {
            if (here.Name != RSVConstants.L_FOREST || spawned_today)
            {
                //Log.Trace("RSV: Not Ridge Forest OR Foxbloom already spawned today.");
                return false;
            }
            if (Game1.dayOfMonth != FoxbloomDay || !here.GetWeather().Equals(Game1.weather_sunny))
            {
                Log.Trace($"RSV: Today ({Game1.dayOfMonth}) not Foxbloom Day ({FoxbloomDay}) OR weather not clear.");
                return false;
            }
            if (!Game1.player.Items.ContainsId("IRELICFOXMASK"))
            {
                Log.Trace("RSV: Player does not have Relic Fox Mask in inventory.");
                return false;
            }
            Log.Trace("RSV: Foxbloom can spawn!");
            return true;
        }

        public static List<string> GetFestivalDaysAndBday(string name)
        {
            List<string> visits = new List<string>();
            var date = Game1.dayOfMonth;
            switch (Game1.currentSeason)
            {
                case "spring":
                    if (date > 7 && date < 15)
                    {
                        visits.Add("Saturday");
                    }
                    else if (date > 21 && date <= 28)
                    {
                        visits.Add("Wednesday");
                    }
                    break;
                case "summer":
                    if (name == "Bryle" && date < 8)
                    {
                        // Bryle's birthday 
                        visits.Add("Monday");
                    }
                    else if (date > 7 && date < 15)
                    {
                        visits.Add("Thursday");
                    }
                    else if (date > 21 && date <= 28)
                    {
                        visits.Add("Sunday");
                    }
                    break;
                case "fall":
                    if (name == "Zayne" && date < 8)
                    {
                        // Bryle's birthday 
                        visits.Add("Tuesday");
                    }
                    if (date > 14 && date < 22)
                    {
                        visits.Add("Tuesday");
                        visits.Add("Saturday");
                    }
                    else if (date > 21 && date <= 28)
                    {
                        visits.Add("Saturday");
                    }
                    break;
                case "winter":
                    if (date > 7 && date < 15)
                    {
                        visits.Add("Monday");
                    }
                    else if (date > 21 && date <= 28)
                    {
                        visits.Add("Thursday");
                        visits.Add("Sunday");
                    }
                    break;
            }
            return visits.ToList();
        }

    }
}
