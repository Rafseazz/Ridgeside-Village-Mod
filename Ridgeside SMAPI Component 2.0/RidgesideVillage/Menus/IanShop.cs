using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{
    internal static class IanShop
    {
        const string willWaterPlants = "IanShop.WaterPlants";
        const int waterPlantsPriceSmall = 1000;
        const int waterPlantsPriceMedium = 2500;
        const int waterPlantsPriceLarge = 5000;
        const int wpsmall = 120;
        const int wpmedium = 480;
        const int wplarge = 960;
        const int daysWillWater = 3;
        const int daysWillPet = 7;

        const string willFixFences = "IanShop.fixFences";
        const string willPetAnimals = "IanShop.PetAnimals";
        const int perFencePrice = 6;
        const int perAnimalPrice = 60;

        const int UNLOCKEVENT = 75160387;
        public const string FARMUPGRADED = "RSV.SummitFarmRedone";
        const string MINECARTSFIXED = "RSV.FixedMinecart";
        private static bool canRenovate = false;

        static IModHelper Helper;
        static IMonitor Monitor;
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            TileActionHandler.RegisterTileAction("IanCounter", OpenIanMenu);
        }

        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            canRenovate = Game1.MasterPlayer.eventsSeen.Contains(UNLOCKEVENT) && !Game1.MasterPlayer.mailReceived.Contains(FARMUPGRADED);

            if (Game1.IsMasterGame)
            {
                var FarmModData = Game1.getFarm().modData;

                if (FarmModData.ContainsKey(willFixFences))
                {
                    FixTheFences();
                    Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("IanShop.HasFixedFences"), HUDMessage.newQuest_type));
                    FarmModData.Remove(willFixFences);
                }

                if (FarmModData.ContainsKey(willPetAnimals))
                {
                    if (!FarmModData.TryGetValue(willPetAnimals, out string value) || !int.TryParse(value, out int daysLeft) || daysLeft <= 0)
                    {
                        FarmModData.Remove(willPetAnimals);
                        return;
                    }
                    else if (daysLeft == 1)
                    {
                        FarmModData.Remove(willPetAnimals);
                    }
                    else
                    {
                        FarmModData[willPetAnimals] = (daysLeft - 1).ToString();
                    }
                    petAnimals();
                    Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("IanShop.HasPetAnimals"), HUDMessage.newQuest_type));
                }
                Helper.Events.GameLoop.OneSecondUpdateTicked += waterPlantsIfNeeded;
            }
        }

        private static void petAnimals()
        {
            var FarmAnimals = Game1.getFarm().getAllFarmAnimals();
            foreach(var farmAnimal in FarmAnimals)
            {
                farmAnimal.pet(Game1.player, is_auto_pet: false);
            }
        }

        //Will water plants if player has flag
        //format is daysLeft/Size
        private static void waterPlantsIfNeeded(object sender, OneSecondUpdateTickedEventArgs e)
        {

            Helper.Events.GameLoop.OneSecondUpdateTicked -= waterPlantsIfNeeded;
            var farmModData = Game1.getFarm().modData;
            if (Game1.IsRainingHere(Game1.getFarm()))
            {
                return;
            }
            if (farmModData.TryGetValue(willWaterPlants, out string value))
            {
                var valueSplit = value.Split('/');
                if (valueSplit.Length != 2)
                {
                    return;
                }

                int wateringDaysLeft = int.Parse(valueSplit[0]);
                int numberOfTiles = int.Parse(valueSplit[1]);

                if(wateringDaysLeft <= 0)
                {
                    //shouldnt happen
                    farmModData.Remove(willWaterPlants);
                    return;
                }
                else if(wateringDaysLeft == 0)
                {
                    //last day. remove flag
                    Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("IanShop.Deadline"), HUDMessage.newQuest_type));
                    farmModData.Remove(willWaterPlants);
                }
                else
                {
                    wateringDaysLeft--;
                    farmModData[willWaterPlants] = $"{wateringDaysLeft}/{numberOfTiles}";

                }
                WaterThePlants(numberOfTiles);
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("IanShop.HasWatered"), HUDMessage.newQuest_type));
            }
        }

        private static void OpenIanMenu(string tileActionString, Vector2 position)
        {
            OpenIanMenu(tileActionString);
        }
        private static void OpenIanMenu(string tileActionString = "")
        {
            IanCounterMenu();
            //should be fine now for non-host players, too


            /*
            if (Context.IsMainPlayer)
            {
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("OnlyFarmOwner"));
            }*/
        }
        private static void IanCounterMenu()
        {
            var responses = new List<Response>
            {
                new Response("waterPlants", Helper.Translation.Get("IanShop.WaterPlants")),
                new Response("fixFences", Helper.Translation.Get("IanShop.fixFences")),
                new Response("willPet", Helper.Translation.Get("IanShop.PetAnimals")),
                new Response("cancel", Helper.Translation.Get("IanShop.Cancel"))
            };
            var responseActions = new List<Action>
            {
                delegate
                {
                    WaterPlantsMenu();
                },
                delegate
                {
                    FixFencesMenu();
                },
                delegate
                {
                    PetAnimalsMenu();
                },
                delegate
                {
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.Bye"));
                }
            };

            if (canRenovate)
            {
                responses.Insert(3, new Response("renovate", Helper.Translation.Get("IanShop.SummitFarm")));
                responseActions.Insert(3, delegate { RenovateOptions(); });
            }
            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("IanShop.Open"), responses, responseActions);
        }

        private static void PetAnimalsMenu()
        {
            int n = Game1.getFarm().getAllFarmAnimals().Count();

            if (!Game1.getFarm().modData.ContainsKey(willPetAnimals) && n > 0)
            {
                var responses = new List<Response>
                {
                    new Response("petAnimals", Helper.Translation.Get("IanShop.PetAnimalsSelection") + (n * perAnimalPrice) + "$"),
                    new Response("cancel", Helper.Translation.Get("IanShop.Cancel"))
                };
                var responseActions = new List<Action>
                {
                    delegate
                    {
                        if(Game1.player.Money >= n * perAnimalPrice)
                        {
                            Game1.player.Money -= (n * perAnimalPrice);
                            Game1.getFarm().modData.Add(willPetAnimals, daysWillPet.ToString());
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.PetThanks"));
                        }
                        else
                        {
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("NotEnoughMoney"));
                        }
                    },
                    delegate
                    {
                        IanCounterMenu();
                    }
                };
                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("IanShop.PetMenu"), responses, responseActions);
            }
            else if (n <= 0)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.YouHaveNoAnimals"));
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.AlreadyWillPet"));
            }
        }

        private static void WaterPlantsMenu()
        {
            if (!Game1.getFarm().modData.ContainsKey(willWaterPlants))
            {
                var responses = new List<Response>
                {
                    new Response("small", wpsmall + Helper.Translation.Get("IanShop.WaterInfo") + waterPlantsPriceSmall + "$"),
                    new Response("medium", wpmedium + Helper.Translation.Get("IanShop.WaterInfo") + waterPlantsPriceMedium + "$"),
                    new Response("large", wplarge + Helper.Translation.Get("IanShop.WaterInfo") + waterPlantsPriceLarge + "$"),
                    new Response("cancel", Helper.Translation.Get("IanShop.Cancel"))
                };
                var responseActions = new List<Action>
                {
                    delegate
                    {
                        if(Game1.player.Money < waterPlantsPriceSmall)
                        {
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("NotEnoughMoney"));
                        }
                        else
                        {
                            Game1.player.Money -= waterPlantsPriceSmall;
                            Game1.getFarm().modData.Add(willWaterPlants, $"{daysWillWater}/{wpsmall}");
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.Thankyou"));
                        };
                    },
                    delegate
                    {
                        if(Game1.player.Money < waterPlantsPriceMedium)
                        {
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("NotEnoughMoney"));
                        }
                        else
                        {
                            Game1.player.Money -= waterPlantsPriceMedium;
                            Game1.getFarm().modData.Add(willWaterPlants, $"{daysWillWater}/{wpmedium}");
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.Thankyou"));
                        };
                    },
                    delegate
                    {
                        if(Game1.player.Money < waterPlantsPriceLarge)
                        {
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("NotEnoughMoney"));
                        }
                        else
                        {
                            Game1.player.Money -= waterPlantsPriceLarge;
                            Game1.getFarm().modData.Add(willWaterPlants, $"{daysWillWater}/{wplarge}");
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.Thankyou"));
                        };
                    },
                    delegate
                    {
                        IanCounterMenu();
                    }
                };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("IanShop.WaterPlantsMenu"), responses, responseActions);
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.AlreadyWatering"));
            }
        }

        internal static void WaterThePlants(int maxNumberToWater)
        {
            int n = 0;
            foreach (var pair in Game1.getFarm().terrainFeatures.Pairs)
            {
                if(n >= maxNumberToWater)
                {
                    break;
                }

                if (pair.Value is HoeDirt dirt && dirt.state.Value == 0 && dirt.crop != null)
                {
                    dirt.state.Value = 1;
                    n++;
                }
            }
            
        }
      
        private static void FixFencesMenu()
        {
            int n = Game1.getFarm().Objects.Values.OfType<Fence>().Count();

            if (!Game1.getFarm().modData.ContainsKey(willFixFences) && n > 0)
            {
                var responses = new List<Response>
                {
                    new Response("fixFence", n + Helper.Translation.Get("IanShop.Fences") + (n * perFencePrice) + "$"),
                    new Response("cancel", Helper.Translation.Get("IanShop.Cancel"))
                };
                var responseActions = new List<Action>
                {
                    delegate
                    {
                        if(Game1.player.Money >= n * perFencePrice)
                        {
                            Game1.player.Money -= (n * perFencePrice);
                            Game1.getFarm().modData.Add(willFixFences, "");
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.FenceThanks"));
                        }
                        else
                        {
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("NotEnoughMoney"));
                        }
                    },
                    delegate
                    {
                        IanCounterMenu();
                    }
                };
                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("IanShop.FenceMenu"), responses, responseActions);
            }
            else if (n <= 0)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.YouHaveNoFences"));
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.AlreadyWillFix"));
            }
        }

        private static void FixTheFences()
        {
            foreach (Fence fence in Game1.getFarm().Objects.Values.OfType<Fence>())
            {
                fence.repair();
                fence.health.Value *= 2f;
                fence.maxHealth.Value = fence.health.Value;
                if (fence.isGate.Value)
                    fence.health.Value *= 2f;
            }
        }

        private static void RenovateOptions()
        {
            if(!Game1.MasterPlayer.mailReceived.Contains(MINECARTSFIXED))
            {
                NPC sean = Game1.getCharacterFromName("Sean");
                sean.CurrentDialogue.Clear();
                sean.CurrentDialogue.Push(new Dialogue(Helper.Translation.Get("IanShop.BrokenCarts"), sean));
                Game1.drawDialogue(sean);
            }
            else
            {
                Game1.activeClickableMenu = new SummitRenovateMenu();
            }
        }

    }
  
}
