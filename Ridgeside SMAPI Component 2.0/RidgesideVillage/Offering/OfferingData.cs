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

        internal OfferingData()
        {
            lookup = new Dictionary<string, OfferEntry>();
            lookup = ModEntry.Helper.Data.ReadJsonFile<Dictionary<string, OfferEntry>>("assets//OfferingData.json");
        }

    }

    enum OfferingType
    {
        WaterPlants,
        GrowPlants,
        Buff,
        ForecastRain,
        ForecastSun,
        BoostLuck,
        BabyChance,
        FairyChance,
        MeteorChance
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
                    UtilFunctions.WaterPlants(Game1.getFarm());
                    break;
                case OfferingType.Buff:
                    this.ApplyBuff();
                    break;
                case OfferingType.GrowPlants:
                    this.GrowPlants();
                    break;
                case OfferingType.ForecastRain:
                    Game1.weatherForTomorrow = Game1.weather_rain;
                    break;
                case OfferingType.ForecastSun:
                    Game1.weatherForTomorrow = Game1.weather_sunny;
                    break;
                case OfferingType.BoostLuck:
                    Game1.player.team.sharedDailyLuck.Value = 0.12;
                    break;
                case OfferingType.BabyChance:
                    Game1.player.mailReceived.Add(RSVConstants.M_TORTSLOVE);
                    break;
                case OfferingType.MeteorChance:
                    Game1.player.mailReceived.Add(RSVConstants.M_TORTSMETEOR);
                    break;
                /*
                case OfferingType.FairyChance:
                    Game1.player.mailReceived.Add(RSVConstants.M_TORTSFAIRY);
                    break;
                */
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
                                String forage = crop.getRandomWildCropForSeason(Game1.season);
                                var Farm = Game1.getFarm();

                                //check if an object is already there
                                if (!Farm.objects.ContainsKey(dirt.Tile))
                                {
                                    Game1.getFarm().objects.Add(dirt.Tile, new StardewValley.Object(forage, 1)
                                    {
                                        IsSpawnedObject = true,
                                        CanBeGrabbed = true
                                    });
                                    Log.Verbose($"RSV: Forage crop fully grown at {dirt.Tile.X}, {dirt.Tile.Y}.");
                                    crop = null;
                                    dirt.destroyCrop(false);

                                }
                            }
                            else
                            {
                                crop.growCompletely();
                                Log.Verbose($"RSV: Regular crop fully grown at {dirt.Tile.X}, {dirt.Tile.Y}.");
                            }

                            n++;
                        }
                    }
                }
            }

        }

        private void ApplyBuff()
        {
            string id = "BlessingBuff";
            switch (this.BuffType.ToLower())
            {
                case "goblinscurse":
                    id = Buff.goblinsCurse;
                    break;
                case "slimed":
                    id = Buff.slimed;
                    break;
                case "evileye":
                    id = Buff.evilEye;
                    break;
                case "tipsy":
                    id = Buff.tipsy;
                    break;
                case "fear":
                    id = Buff.fear;
                    break;
                case "frozen":
                    id = Buff.frozen;
                    break;
                case "yobablessing":
                    id = Buff.yobaBlessing;
                    break;
                case "nauseous":
                    id = Buff.nauseous;
                    break;
                case "darkness":
                    id = Buff.darkness;
                    break;
                case "weakness":
                    id = Buff.weakness;
                    break;
                case "squidinkravioli":
                    id = Buff.squidInkRavioli;
                    break;
            }

            Buff buff = new Buff(id);
            if (id.Equals("BlessingBuff"))
            {
                switch (this.BuffType.ToLower())
                {
                    case "farming":
                        buff.effects.FarmingLevel.Value = Value;
                        break;
                    case "fishing":
                        buff.effects.FishingLevel.Value = Value;
                        break;
                    case "mining":
                        buff.effects.MiningLevel.Value = Value;
                        break;
                    case "luck":
                        buff.effects.LuckLevel.Value = Value;
                        break;
                    case "foraging":
                        buff.effects.ForagingLevel.Value = Value;
                        break;
                    case "maxstamina":
                        buff.effects.MaxStamina.Value = Value;
                        break;
                    case "speed":
                        buff.effects.Speed.Value = Value;
                        break;
                    case "defense":
                        buff.effects.Defense.Value = Value;
                        break;
                    case "attack":
                        buff.effects.Attack.Value = Value;
                        break;
                }
            }

            buff.millisecondsDuration = buff.totalMillisecondsDuration = this.Duration * 1000;
            buff.displaySource = Game1.getCharacterFromName("Raeriyala").displayName;
            Game1.player.applyBuff(buff);

        }
    }
}
