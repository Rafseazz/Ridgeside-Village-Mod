using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RidgesideVillage
    {
    class Patcher
        {
        const int CURIOSITY_LURE = 856;
        const double BASE_CATCH_CHANCE = 0.2;
        const double CATCH_CHANCE_WITH_CURIOLURE = 0.27;  // Curiosity Lure increases chance by 7%
        const int MIN_FISHING = 3;

        static IModHelper Helper;
        static IManifest Manifest;
        static IJsonAssetsApi JsonAssetsAPI;

        public Patcher(IMod mod) {
            Helper = mod.Helper;
            Manifest = mod.ModManifest;
            }

        public void PerformPatching() {
            JsonAssetsAPI = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            var harmony = HarmonyInstance.Create(Manifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getFish)),
                postfix: new HarmonyMethod(typeof(Patcher), nameof(Patcher.GetFish_Postfix))
                );
            }

        [HarmonyPostfix]
        public static void GetFish_Postfix(GameLocation __instance, float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName, ref StardewValley.Object __result) {
            try {
                if (!__instance.IsUsingMagicBait(who)) {
                    return;
                    }
                string nameToUse = locationName ?? __instance.Name;
                Log.Trace($"Player {who.Name} using magic bait at {nameToUse} with original fish result is {__result.Name}");

                double catchChance =
                    (who.CurrentTool is StardewValley.Tools.FishingRod rod && rod.getBobberAttachmentIndex() == CURIOSITY_LURE)
                    ? CATCH_CHANCE_WITH_CURIOLURE
                    : BASE_CATCH_CHANCE
                    ;

                List<string> fish_names = new List<string>();
                switch (nameToUse) {
                    // Custom locations are added to the game without their prefixes
                    case "RidgesideVillage":
                        fish_names.Add("Bladetail Sturgeon");
                        fish_names.Add("Harvester Trout");
                        fish_names.Add("Lullaby Carp");
                        fish_names.Add("Pebble Back Crab");
                        break;
                    case "Ridge":
                        fish_names.Add("Caped Tree Frog");
                        fish_names.Add("Fixer Eel");
                        fish_names.Add("Golden Rose Fin");
                        break;
                    case "Beach":
                        // Although Beach has it's own location class, it calls the base class getFish function, so we're okay to just postfix that.
                        fish_names.Add("Cardia Septal Jellyfish");
                        fish_names.Add("Crimson Spiked Clam");
                        fish_names.Add("Fairytale Lionfish");
                        break;
                    default:
                        return;
                    }
                foreach (string fish in fish_names) {
                    int fish_id = JsonAssetsAPI.GetObjectId(fish);
                    // Currently this gives each fish a 20% chance to be caught, could be lower if we add more configuration
                    if (fish_id != -1 && !who.fishCaught.ContainsKey(fish_id) && who.FishingLevel >= MIN_FISHING && Game1.random.NextDouble() < catchChance) {
                        Log.Trace($"Fish {fish} (ID: {fish_id}) is caught: {who.fishCaught.ContainsKey(fish_id)}, setting fish result to this fish");
                        __result = new StardewValley.Object(fish_id, 1);
                        return;
                        }
                    else {
                        Log.Trace($"Fish {fish} (ID: {fish_id}) is caught: {who.fishCaught.ContainsKey(fish_id)}");
                        }
                    }
                return;
                }
            catch (Exception ex) {
                Log.Error($"Failed in {nameof(GetFish_Postfix)}:\n{ex}");
                }
            }

        }
    }
