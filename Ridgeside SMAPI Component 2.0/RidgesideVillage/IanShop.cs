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
        const string willWaterPlants = "RSV.WillWaterPlants";
        const string waterDeadline = "RSVwaterdl.";
        const string waterPlantsFlagSmall = "RSV.WaterPlantsJobSmall";
        const string waterPlantsFlagMedium = "RSV.WaterPlantsJobMedium";
        const string waterPlantsFlagLarge = "RSV.WaterPlantsJobLarge";
        const int waterPlantsPriceSmall = 1000;
        const int waterPlantsPriceMedium = 2500;
        const int waterPlantsPriceLarge = 5000;
        const int wpsmall = 120;
        const int wpmedium = 480;
        const int wplarge = 960;
        const int daysWillWater = 3;

        const string willFixFences = "RSV.WillFixFences";
        const int perfenceprice = 6;

        static IModHelper Helper;
        static IMonitor Monitor;
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            TileActionHandler.RegisterTileAction("IanCounter", OpenIanMenu);

            //Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            //Will water plots if player has flag
            if (Game1.player.mailReceived.Contains(willWaterPlants))
            {
                WaterThePlants();
                string daysPassed = $"{Game1.Date.TotalDays}";
                var toRemove = new List<string>();
                foreach (var entry in Game1.player.mailReceived)
                {
                    if (entry.StartsWith(waterDeadline) && entry.Contains(daysPassed))
                    {
                        Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("IanShop.Deadline"), HUDMessage.newQuest_type));
                        Game1.player.mailReceived.Remove(willWaterPlants);
                        if (Game1.player.mailReceived.Contains(waterPlantsFlagSmall))
                        {
                            toRemove.Add(waterPlantsFlagSmall);
                        }
                        if (Game1.player.mailReceived.Contains(waterPlantsFlagMedium))
                        {
                            toRemove.Add(waterPlantsFlagMedium);
                        }
                        if (Game1.player.mailReceived.Contains(waterPlantsFlagLarge))
                        {
                            toRemove.Add(waterPlantsFlagLarge);
                        }
                        toRemove.Add(entry);
                    }

                    else if (entry.StartsWith(waterDeadline) && !entry.Contains(daysPassed))
                    {
                        int n;
                        var split = entry.Split('.');
                        n = Int32.Parse(split[1]);
                        if (Int32.Parse(daysPassed) > n)
                        {
                            Log.Debug("Removed Odd Jobs Watering Service Flag due to time travel.");
                            Game1.player.mailReceived.Remove(willWaterPlants);
                            if (Game1.player.mailReceived.Contains(waterPlantsFlagSmall))
                            {
                                toRemove.Add(waterPlantsFlagSmall);
                            }
                            if (Game1.player.mailReceived.Contains(waterPlantsFlagMedium))
                            {
                                toRemove.Add(waterPlantsFlagMedium);
                            }
                            if (Game1.player.mailReceived.Contains(waterPlantsFlagLarge))
                            {
                                toRemove.Add(waterPlantsFlagLarge);
                            }
                            toRemove.Add(entry);
                        }
                    }
                }
                foreach (var entry in toRemove)
                {
                    Game1.player.mailReceived.Remove(entry);
                }
                if (toRemove.Count > 0)
                {
                    Game1.player.mailReceived.Remove(willWaterPlants);
                }
            }
            
            if (Game1.player.mailReceived.Contains(willFixFences))
            {
                FixTheFences();
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("Ian.HasFixedFences"), HUDMessage.newQuest_type));
                Game1.player.mailReceived.Remove(willFixFences);
            }
        }

        private static void OpenIanMenu(string tileActionString, Vector2 position)
        {
            OpenIanMenu(tileActionString);
        }
        private static void OpenIanMenu(string tileActionString = "")
        {
            if (Context.IsMainPlayer)
            {
                IanCounterMenu();
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("OnlyFarmOwner"));
            }
        }
        private static void IanCounterMenu()
        {
            var responses = new List<Response>
            {
                new Response("waterPlants", Helper.Translation.Get("IanShop.WaterPlants")),
                new Response("fixFences", Helper.Translation.Get("IanShop.fixFences")),
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
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShopBye"));
                }
            };

            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("OpenIanShop"), responses, responseActions);
        }

        private static void WaterPlantsMenu()
        {
            if (!Game1.player.mailReceived.Contains(willWaterPlants))
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
                            Game1.player.mailReceived.Add(waterPlantsFlagSmall);
                            Game1.player.mailReceived.Add(willWaterPlants);
                            Game1.player.mailReceived.Add(waterDeadline + (Game1.Date.TotalDays + daysWillWater));
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
                            Game1.player.mailReceived.Add(waterPlantsFlagMedium);
                            Game1.player.mailReceived.Add(willWaterPlants);
                            Game1.player.mailReceived.Add(waterDeadline + (Game1.Date.TotalDays + daysWillWater));
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
                            Game1.player.mailReceived.Add(waterPlantsFlagLarge);
                            Game1.player.mailReceived.Add(willWaterPlants);
                            Game1.player.mailReceived.Add(waterDeadline + (Game1.Date.TotalDays + daysWillWater));
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.Thankyou"));
                        };
                    },
                    delegate
                    {
                        IanCounterMenu();
                    }
                };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("IanWaterPlantsMenu"), responses, responseActions);
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanIsAlreadyWatering"));
            }
        }

        private static void WaterThePlants()
        {
            if (Game1.IsRainingHere(Game1.getLocationFromName("Farm")))
            {
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("IanShop.Raining"), HUDMessage.newQuest_type));
                if (Game1.player.mailReceived.Contains(waterPlantsFlagSmall))
                {
                    Game1.player.Money += (waterPlantsPriceSmall / 3);
                }
                else if (Game1.player.mailReceived.Contains(waterPlantsFlagMedium))
                {
                    Game1.player.Money += (waterPlantsPriceMedium / 3);
                }
                else if (Game1.player.mailReceived.Contains(waterPlantsFlagLarge))
                {
                    Game1.player.Money += (waterPlantsPriceLarge / 3);
                }
            }

            else //water plants
            {
                int limit = 0;
                var receivedMail = Game1.player.mailReceived;
                if (receivedMail.Contains(waterPlantsFlagSmall))
                {
                    limit = wpsmall + 1;
                }
                else if (receivedMail.Contains(waterPlantsFlagMedium))
                {
                    limit = wpmedium + 1;
                }
                else if (receivedMail.Contains(waterPlantsFlagLarge))
                {
                    limit = wplarge + 1;
                }
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("IanShop.HasWatered"), HUDMessage.newQuest_type));
                int n = 0;
                foreach (var pair in Game1.getLocationFromName("Farm").terrainFeatures.Pairs)
                {
                    if(n >= limit)
                    {
                        break;
                    }
                    if (pair.Value is HoeDirt dirt && dirt.state.Value == 0)
                    {
                        dirt.state.Value = 1;
                        n++;
                    }
                }
            }
        }
      
        private static void FixFencesMenu()
        {
            int n = 0;
            foreach (Fence fence in Game1.getFarm().Objects.Values.OfType<Fence>())
            {
                n++;
            }

            if (!Game1.player.mailReceived.Contains(willFixFences) && n > 0)
            {
                var responses = new List<Response>
                {
                    new Response("fixFence", n + Helper.Translation.Get("IanShop.Fences") + (n * perfenceprice) + "$"),
                    new Response("cancel", Helper.Translation.Get("IanShop.Cancel"))
                };
                var responseActions = new List<Action>
                {
                    delegate
                    {
                        if(Game1.player.Money >= n * perfenceprice)
                        {
                            Game1.player.Money -= (n * perfenceprice);
                            Game1.player.mailReceived.Add(willFixFences);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Ian.FenceThanks"));
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
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Ian.YouHaveNoFences"));
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Ian.AlreadyWillFix"));
            }
        }

        private static void FixTheFences()
        {;
            foreach (Fence fence in Game1.getFarm().Objects.Values.OfType<Fence>())
            {
                fence.repair();
                fence.health.Value *= 2f;
                fence.maxHealth.Value = fence.health.Value;
                if (fence.isGate.Value)
                    fence.health.Value *= 2f;
            }
        }
    }
  
}
