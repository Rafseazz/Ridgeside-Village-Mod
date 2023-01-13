using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RidgesideVillage.Offering
{
    internal class OfferingData
    {
        internal Dictionary<string, OfferEntry> lookup;

        static OfferingData Data;
        internal OfferingData()
        {
            lookup = new Dictionary<string, OfferEntry>();
            lookup = ModEntry.Helper.Data.ReadJsonFile<Dictionary<string, OfferEntry>>("assets//OfferingData.json");
            Data = this;
        }

    }

    enum OfferingType
    {
        WaterPlants,
        GrowPlants,
        Buff,
    }

    internal class OfferEntry
    {
        //strength for buff or amount for watering/growing
        public int Value { get; set; }
        //Buff duration (if its a buff, otherwise ignored)
        public int Duration { get; set; }
        public OfferingType Effect { get; set; }
        // name of Buff if it's a buff/debuff, otherwise ignored
        public string BuffType { get; set; }
        //key of corresponding entry in event file
        public string ScriptKey { get; set; }

        //Apply the effect
        internal void Apply()
        {

            switch (this.Effect)
            {
                case OfferingType.WaterPlants:
                    IanShop.WaterThePlants(Game1.getFarm(), this.Value);
                    break;
                case OfferingType.Buff:
                    this.ApplyBuff();
                    break;
                case OfferingType.GrowPlants:
                    this.GrowPlants();
                    break;
            }
            return;
        }

        private void GrowPlants()
        {
            int n = 0;
            var locations = new List<GameLocation>() { Game1.getFarm(), Game1.getLocationFromName(RSVConstants.L_SUMMITFARM) };
            foreach (var location in locations)
            {
                if (location is not null)
                {
                    foreach (var pair in location.terrainFeatures.Pairs)
                    {
                        if (n >= this.Value)
                        {
                            break;
                        }
                        if (pair.Value is HoeDirt dirt && dirt.crop != null)
                        {
                            Crop crop = dirt.crop;
                            bool harvestable = crop.currentPhase.Value >= crop.phaseDays.Count - 1 && (!crop.fullyGrown.Value || crop.dayOfCurrentPhase.Value <= 0);

                            if (harvestable)
                            {
                                continue;
                            }
                            else if (crop.isWildSeedCrop())
                            {
                                int forage = crop.getRandomWildCropForSeason(Game1.currentSeason);
                                Game1.getFarm().objects.Add(dirt.currentTileLocation, new StardewValley.Object(dirt.currentTileLocation, forage, 1)
                                {
                                    IsSpawnedObject = true,
                                    CanBeGrabbed = true
                                });
                                Log.Verbose($"RSV: Forage crop fully grown at {dirt.currentTileLocation.X}, {dirt.currentTileLocation.Y}.");
                                crop = null;
                                dirt.destroyCrop(dirt.currentTileLocation, false, Game1.getFarm());
                            }
                            else
                            {
                                crop.growCompletely();
                                Log.Verbose($"RSV: Regular crop fully grown at {dirt.currentTileLocation.X}, {dirt.currentTileLocation.Y}.");
                            }

                            n++;
                        }
                    }
                }
            }

        }

        private void ApplyBuff()
        {
            int index = -1;
            switch (this.BuffType.ToLower())
            {
                case "farming":
                    index = Buff.farming;
                    break;
                case "fishing":
                    index = Buff.fishing;
                    break;
                case "mining":
                    index = Buff.mining;
                    break;
                case "luck":
                    index = Buff.luck;
                    break;
                case "foraging":
                    index = Buff.foraging;
                    break;
                case "crafting":
                    index = Buff.crafting;
                    break;
                case "maxstamina":
                    index = Buff.maxStamina;
                    break;
                case "speed":
                    index = Buff.speed;
                    break;
                case "defense":
                    index = Buff.defense;
                    break;
                case "attack":
                    index = Buff.attack;
                    break;
                case "goblinscurse":
                    index = Buff.goblinsCurse;
                    break;
                case "slimed":
                    index = Buff.slimed;
                    break;
                case "evileye":
                    index = Buff.evilEye;
                    break;
                case "chickenedout":
                    index = Buff.chickenedOut;
                    break;
                case "tipsy":
                    index = Buff.tipsy;
                    break;
                case "fear":
                    index = Buff.fear;
                    break;
                case "frozen":
                    index = Buff.frozen;
                    break;
                case "yobablessing":
                    index = Buff.yobaBlessing;
                    break;
                case "nauseous":
                    index = Buff.nauseous;
                    break;
                case "darkness":
                    index = Buff.darkness;
                    break;
                case "weakness":
                    index = Buff.weakness;
                    break;
                case "squidinkravioli":
                    index = Buff.squidInkRavioli;
                    break;
            }

            if (index != -1)
            {
                Buff buff;
                if (index == 7)
                {
                    //needs to be updated as drink buff as stamina as otherBuff is bugged
                    //probably fixed in 1.6
                    string sourceString = Game1.getCharacterFromName("Raeriyala").displayName;
                    buff = new Buff(0, 0, 0, 0, 0, 0, 0, this.Value, 0, 0, 0, 0, this.Duration, sourceString, sourceString);
                    Game1.buffsDisplay.tryToAddDrinkBuff(buff);
                    Game1.player.Stamina = Game1.player.MaxStamina;
                }
                else
                {
                    buff = new Buff(index);
                    buff.buffAttributes[index] = Value;
                    buff.millisecondsDuration = buff.totalMillisecondsDuration = this.Duration * 1000;
                    buff.displaySource = Game1.getCharacterFromName("Raeriyala").displayName;
                    Game1.buffsDisplay.addOtherBuff(buff);

                }
            }
        }
    }
}
