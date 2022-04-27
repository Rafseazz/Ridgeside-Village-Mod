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
            var cp = Helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");
            if (cp is null) {
                Log.Alert("Content Patcher is not installed- RSV requires CP to run. Please install CP and restart your game.");
                return;   
            }

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
            /// <summary>Whether or not the Foxbloom has been spawned.</summary>
            static public string spawned = "false";


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
                // Not Foxbloom day
                if (Game1.dayOfMonth != FoxbloomDay)
                    return false;

                // If Foxbloom already spawned today
                if (spawned == "true")
                    return false;

                if (Game1.getLocationFromName("Custom_Ridgeside_RidgeForest").modData["RSV_foxbloomSpawned"] == "true")
                {
                    // Foxbloom has spawned! Need to update
                    spawned = "true";
                    return true;
                }

                // Foxbloom has not spawned yet today
                return false;
            }

            /// <summary>Get whether the token is available for use.</summary>
            public bool IsReady()
            {
                return Context.IsWorldReady;
            }

            /// <summary>Get the current values.</summary>
            /// <param name="input">The input arguments, if applicable.</param>
            public IEnumerable<string> GetValues(string input)
            {
                return new[] { spawned };
            }
        }
    }
}
