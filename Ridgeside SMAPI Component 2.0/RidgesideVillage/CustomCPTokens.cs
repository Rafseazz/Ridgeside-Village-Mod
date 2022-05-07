using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace RidgesideVillage
    {
    class CustomCPTokens
        {
        private readonly IModHelper Helper;
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

            cp.RegisterToken(this.ModManifest, "EnableRidgesideMusic", () => new string[] { Config.enableRidgesideMusic.ToString() });

            cp.RegisterToken(this.ModManifest, "RepeatCableCarCutscene", () => new string[] { Config.repeatCableCarCutscene.ToString() });

            cp.RegisterToken(this.ModManifest, "EnableOtherNPCsInCableCar", () => new string[] { Config.enableOtherNPCsInCableCar.ToString() });

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
                int? randomseed = (int?)(Game1.stats?.daysPlayed ?? SaveGame.loaded?.stats?.daysPlayed);
                if (randomseed is not null)
                {   //Seed the random with a seed that only changes every 28 days
                    Random random = new Random((int)Game1.uniqueIDForThisGame + ((randomseed.Value - 1) / 28));
                    FoxbloomDay = random.Next(1, 5) * 7;
                    return new[] { FoxbloomDay.ToString() };
                }
                return null; //return null for an unready token.
            });

            cp.RegisterToken(this.ModManifest, "FoxbloomSpawned", new FoxbloomSpawned());
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
            if (here.Name != "Custom_Ridgeside_RidgeForest" || spawned_today)
            {
                Log.Trace("RSV: Not Ridge Forest OR Foxbloom already spawned today.");
                return false;
            }
            if (Game1.dayOfMonth != FoxbloomDay || UtilFunctions.GetWeather(here) % 2 != 0)
            {
                Log.Trace("RSV: Not Foxbloom Day OR weather not clear.");
                return false;
            }
            if (!Game1.player.hasItemInInventoryNamed("Relic Fox Mask"))
            {
                Log.Trace("RSV: Player does not have Relic Fox Mask in inventory.");
                return false;
            }
            Log.Trace("RSV: Foxbloom can spawn!");
            return true;
        }

    }
}
