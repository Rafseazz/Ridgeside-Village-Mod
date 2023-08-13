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
        const int perCropPrice = 5;
        const int perFencePrice = 10;
        const int perAnimalPrice = 10;

        const string willWaterPlants = "IanShop.WaterPlants";
        const string willFixFences = "IanShop.FixFences";
        const string willPetAnimals = "IanShop.PetAnimals";

        static IModHelper Helper;
        static IMonitor Monitor;
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            TileActionHandler.RegisterTileAction("IanCounter", OpenIanMenu);
        }

        [EventPriority(EventPriority.High)]
        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {

            if (Game1.IsMasterGame)
            {
                // Farming services
                var FarmModData = Game1.getFarm().modData;

                if (FarmModData.ContainsKey(willFixFences))
                {
                    fixFences(FarmModData);
                }

                if (FarmModData.ContainsKey(willPetAnimals))
                {
                    petAnimalsDaily(FarmModData);
                }

                if (FarmModData.ContainsKey(willWaterPlants))
                {
                    Helper.Events.GameLoop.OneSecondUpdateTicked += waterPlantsDaily;
                }

                // Construction services
                if (Game1.player.activeDialogueEvents.TryGetValue(RSVConstants.CT_HOUSEUPGRADE, out int housect) && housect == 0)
                {
                    Game1.player.mailReceived.Add(RSVConstants.M_HOUSEUPGRADED);
                    Game1.player.activeDialogueEvents.Remove(RSVConstants.CT_HOUSEUPGRADE);
                    Game1.player.activeDialogueEvents.Remove(RSVConstants.CT_ACTIVECONSTRUCTION);
                }
                if (Game1.player.activeDialogueEvents.TryGetValue(RSVConstants.CT_CLIMATE, out int climatect) && climatect == 0)
                {
                    Game1.getLocationFromName(RSVConstants.L_SUMMITFARM).isGreenhouse.Value = true;
                    Game1.player.mailReceived.Add(RSVConstants.M_CLIMATECONTROLLED);
                    Game1.player.activeDialogueEvents.Remove(RSVConstants.CT_CLIMATE);
                    Game1.player.activeDialogueEvents.Remove(RSVConstants.CT_ACTIVECONSTRUCTION);
                }
                if (Game1.player.activeDialogueEvents.TryGetValue(RSVConstants.CT_SPRINKLER, out int sprinklerct) && sprinklerct == 0)
                {
                    Game1.player.mailReceived.Add(RSVConstants.M_GOTSPRINKLERS);
                    Game1.player.activeDialogueEvents.Remove(RSVConstants.CT_SPRINKLER);
                    Game1.player.activeDialogueEvents.Remove(RSVConstants.CT_ACTIVECONSTRUCTION);
                }
                if (Game1.player.activeDialogueEvents.TryGetValue(RSVConstants.CT_OREAREA, out int orect) && orect == 0)
                {
                    Game1.player.mailReceived.Add(RSVConstants.M_OREAREAOPENED);
                    Game1.player.activeDialogueEvents.Remove(RSVConstants.CT_OREAREA);
                    Game1.player.activeDialogueEvents.Remove(RSVConstants.CT_ACTIVECONSTRUCTION);
                }
                if (Game1.player.activeDialogueEvents.TryGetValue(RSVConstants.CT_SHED, out int shedct) && shedct == 0)
                {
                    Game1.player.mailReceived.Add(RSVConstants.M_SHEDADDED);
                    Game1.player.activeDialogueEvents.Remove(RSVConstants.CT_SHED);
                    Game1.player.activeDialogueEvents.Remove(RSVConstants.CT_ACTIVECONSTRUCTION);
                }

                if (Game1.player.mailReceived.Contains(RSVConstants.M_GOTSPRINKLERS))
                {
                    UtilFunctions.WaterPlants(Game1.getLocationFromName(RSVConstants.L_SUMMITFARM));
                }
            }
        }

        private static void petAnimalsDaily(ModDataDictionary modData)
        {
            var FarmAnimals = Game1.getFarm().getAllFarmAnimals();
            int price = perAnimalPrice * FarmAnimals.Count;
            if (Game1.player.Money < price)
            {
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("IanShop.NotEnoughMoneyHUD").ToString().Replace("{{serviceName}}", Helper.Translation.Get("IanShop.PetAnimals")), HUDMessage.newQuest_type));
                modData.Remove(willPetAnimals);
            }
            foreach(var farmAnimal in FarmAnimals)
            {
                farmAnimal.pet(Game1.player, is_auto_pet: false);
            }
            Game1.player.Money -= price;
            Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("IanShop.HasPetAnimals") + price + "g", HUDMessage.newQuest_type));
            Log.Debug($"RSV: {FarmAnimals.Count} animals pet for {price}g");
        }

        //Will water plants if player has flag
        //format is daysLeft/Size
        private static void waterPlantsDaily(object sender, OneSecondUpdateTickedEventArgs e)
        {

            Helper.Events.GameLoop.OneSecondUpdateTicked -= waterPlantsDaily;
            var farmModData = Game1.getFarm().modData;
            if (Game1.IsRainingHere(Game1.getFarm()))
            {
                return;
            }
            int n = UtilFunctions.WaterPlants(Game1.getFarm());
            int price = perCropPrice * n;
            Game1.player.Money -= price;
            Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("IanShop.HasWatered") + price + "g", HUDMessage.newQuest_type));
            Log.Debug($"RSV: {n} crops watered for {price}g");
            if (Game1.player.Money <= 0)
            {
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("IanShop.NotEnoughMoneyHUD").ToString().Replace("{{serviceName}}", Helper.Translation.Get("IanShop.WaterPlants")), HUDMessage.newQuest_type));
                farmModData.Remove(willWaterPlants);
            }
        }

        private static void fixFences(ModDataDictionary modData)
        {
            int n = 0;
            foreach (Fence fence in Game1.getFarm().Objects.Values.OfType<Fence>())
            {
                fence.repair();
                fence.health.Value *= 2f;
                fence.maxHealth.Value = fence.health.Value;
                if (fence.isGate.Value)
                    fence.health.Value *= 2f;
                n++;
            }
            int price = perFencePrice * n;
            Game1.player.Money -= price;
            Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("IanShop.HasFixedFences") + price + "g", HUDMessage.newQuest_type));
            Log.Debug($"RSV: {n} fences fixed for {price}g");
            modData.Remove(willFixFences);
        }

        private static void OpenIanMenu(string tileActionString, Vector2 position)
        {
            OpenIanMenu(tileActionString);
        }
        private static void OpenIanMenu(string tileActionString = "")
        {
            bool isSomeoneHere = UtilFunctions.IsSomeoneHere(8, 13, 2, 2);
            if (isSomeoneHere)
            {
                IanCounterMenu();
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.Closed"));
            }
            
            
        }
        private static void IanCounterMenu()
        {
            var responses = new List<Response>
            {
                new Response("waterPlants", Helper.Translation.Get("IanShop.WaterPlants")),
                new Response("fixFences", Helper.Translation.Get("IanShop.FixFences")),
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

            if (Game1.MasterPlayer.eventsSeen.Contains(RSVConstants.E_SUMMITUNLOCK))
            {
                responses.Insert(3, new Response("renovate", Helper.Translation.Get("IanShop.SummitFarm")));
                responseActions.Insert(3, delegate { RenovateOptions(); });
            }
            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("IanShop.Open"), responses, responseActions);
        }

        private static void PetAnimalsMenu()
        {
            var modData = Game1.getFarm().modData;
            int n = Game1.getFarm().getAllFarmAnimals().Count();
            if (n > 0)
            {
                var responses = new List<Response>
                {
                    new Response("petAnimals", Helper.Translation.Get("IanShop.PetAnimalsSelection").ToString().Replace("{{amount}}", perAnimalPrice.ToString())),
                    new Response("cancelService", Helper.Translation.Get("IanShop.CancelService")),
                    new Response("cancel", Helper.Translation.Get("IanShop.Cancel"))
                };
                var responseActions = new List<Action>
                {
                    delegate
                    {
                        if (modData.ContainsKey(willPetAnimals))
                        {
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.ServiceAlreadyActive").ToString().Replace("{{serviceName}}", Helper.Translation.Get("IanShop.PetAnimals").ToString().ToLower()));
                        }
                        else if (Game1.player.Money < n * perAnimalPrice)
                        {
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("NotEnoughMoney"));
                        }
                        else
                        {
                            Game1.getFarm().modData.Add(willPetAnimals, perCropPrice.ToString());
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.ServiceStarted").ToString().Replace("{{serviceName}}", Helper.Translation.Get("IanShop.PetAnimals").ToString().ToLower()));
                        }
                    },
                    delegate
                    {
                        var modData = Game1.getFarm().modData;
                        if (modData.ContainsKey(willPetAnimals))
                        {
                            Game1.getFarm().modData.Remove(willPetAnimals);
                        }
                        Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.ServiceCancelled").ToString().Replace("{{serviceName}}", Helper.Translation.Get("IanShop.PetAnimals")));
                    },
                    delegate
                    {
                        IanCounterMenu();
                    }
                };
                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("IanShop.PetAnimalsMenu"), responses, responseActions);
            }
            else if (n <= 0)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.NoAnimals"));
            }
        }

        private static void WaterPlantsMenu()
        {
            var modData = Game1.getFarm().modData;
            var responses = new List<Response>
            {
                new Response("waterPlants", Helper.Translation.Get("IanShop.WaterPlantsSelection").ToString().Replace("{{amount}}", (perCropPrice).ToString())),
                new Response("cancelService", Helper.Translation.Get("IanShop.CancelService")),
                new Response("cancel", Helper.Translation.Get("IanShop.Cancel"))
            };
            var responseActions = new List<Action>
            {
                delegate
                {
                    if (!modData.ContainsKey(willWaterPlants))
                    {
                        modData.Add(willWaterPlants, perCropPrice.ToString());
                        Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.ServiceStarted").ToString().Replace("{{serviceName}}", Helper.Translation.Get("IanShop.WaterPlants").ToString().ToLower()));
                    }
                    else
                    {
                        Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.ServiceAlreadyActive").ToString().Replace("{{serviceName}}", Helper.Translation.Get("IanShop.WaterPlants").ToString().ToLower()));
                    }
                },
                delegate
                {
                    if (modData.ContainsKey(willWaterPlants))
                    {
                        modData.Remove(willWaterPlants);
                        Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.ServiceCancelled").ToString().Replace("{{serviceName}}", Helper.Translation.Get("IanShop.WaterPlants")));
                        return;
                    }
                    else
                    {
                        Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.ServiceNotActive").ToString().Replace("{{serviceName}}", Helper.Translation.Get("IanShop.WaterPlants").ToString().ToLower()));
                    }
                },
                delegate
                {
                    IanCounterMenu();
                }
            };

            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("IanShop.WaterPlantsMenu"), responses, responseActions);
        }
      
        private static void FixFencesMenu()
        {
            int n = Game1.getFarm().Objects.Values.OfType<Fence>().Count();
            var modData = Game1.getFarm().modData;
            if (n > 0)
            {
                var responses = new List<Response>
                {
                    new Response("fixFence", Helper.Translation.Get("IanShop.FixFencesSelection").ToString().Replace("{{amount}}", (n).ToString()).Replace("{{price}}", (perFencePrice).ToString())),
                    new Response("cancelService", Helper.Translation.Get("IanShop.CancelService")),
                    new Response("cancel", Helper.Translation.Get("IanShop.Cancel"))
                };
                var responseActions = new List<Action>
                {
                    delegate
                    {
                        if (modData.ContainsKey(willFixFences))
                        {
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.ServiceAlreadyActive").ToString().Replace("{{serviceName}}", Helper.Translation.Get("IanShop.FixFences".ToString().ToLower())));
                        }
                        else if(Game1.player.Money < n * perFencePrice)
                        {
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("NotEnoughMoney"));
                        }
                        else
                        {
                            modData.Add(willFixFences, perFencePrice.ToString());
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.ServiceStarted").ToString().Replace("{{serviceName}}", Helper.Translation.Get("IanShop.FixFences").ToString().ToLower()));
                        }
                    },
                    delegate
                    {
                        if (modData.ContainsKey(willFixFences))
                        {
                            Game1.getFarm().modData.Remove(willFixFences);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.ServiceCancelled").ToString().Replace("{{serviceName}}", Helper.Translation.Get("IanShop.FixFences")));
                            return;
                        }
                        else
                        {
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.ServiceNotActive").ToString().Replace("{{serviceName}}", Helper.Translation.Get("IanShop.FixFences").ToString().ToLower()));
                        }
                    },
                    delegate
                    {
                        IanCounterMenu();
                    }
                };
                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("IanShop.FixFencesMenu"), responses, responseActions);
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.NoFences"));
            }
        }
        
        private static void RenovateOptions()
        {
            NPC worker = Game1.isRaining ? Game1.getCharacterFromName("Ian") : Game1.getCharacterFromName("Sean");
            if (!Game1.player.IsMainPlayer)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("IanShop.OnlyFarmOwner"));
            }
            else if (!Game1.MasterPlayer.mailReceived.Contains(RSVConstants.M_MINECARTSFIXED))
            {
                worker.CurrentDialogue.Clear();
                worker.CurrentDialogue.Push(new Dialogue(Helper.Translation.Get("IanShop.BrokenCarts"), worker));
                Game1.drawDialogue(worker);
            }
            else if (Game1.MasterPlayer.activeDialogueEvents.TryGetValue(RSVConstants.CT_ACTIVECONSTRUCTION, out int value) && value > 0)
            {
                worker.CurrentDialogue.Clear();
                worker.CurrentDialogue.Push(new Dialogue(Helper.Translation.Get("IanShop.AlreadyBuilding"), worker));
                Game1.drawDialogue(worker);
            }
            else
            {
                SummitRenovateMenu.tryOpenRenovateMenu();
            }
        }

    }
  
}
