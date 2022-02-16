using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{
    public class ModEntry : Mod
    {
        internal static IMonitor ModMonitor { get; set; }
        internal new static IModHelper Helper { get; set; }

        internal static ModConfig Config;

        private ConfigMenu ConfigMenu;
        private CustomCPTokens CustomCPTokens;
        private Patcher Patcher;

        private SpiritShrine SpiritShrine;

        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            Helper = helper;

            if (!Helper.ModRegistry.IsLoaded("Rafseazz.RSVCP"))
            {
                Log.Error("Ridgeside Village appears to be installed incorrectly. Delete it and reinstall it please. If you need help, visit our Discord server!");
                return;
            }

            ConfigMenu = new ConfigMenu(this);
            CustomCPTokens = new CustomCPTokens(this);
            Patcher = new Patcher(this);

            Patcher.PerformPatching();

            HotelMenu.Initialize(this);
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.DayEnding += OnDayEnding;


            Minecarts.Initialize(this);
            SpiritRealm.Initialize(this);

            SpecialOrders.Initialize(this);

            IanShop.Initialize(this);

            Elves.Initialize(this);

            Greenhouses.Initialize(this);

            Loan.Initialize(this);

            PaulaClinic.Initialize(this);
            Offering.OfferingTileAction.Initialize(this);
            //not done (yet?)
            //new CliffBackground();

            Helper.ConsoleCommands.Add("LocationModData", "show ModData of given location", printLocationModData);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            forgetRepeatableEvents();
        }

        private void forgetRepeatableEvents()
        {
            string path = PathUtilities.NormalizePath("assets/RepeatableEvents.json");
            var data = Helper.Content.Load<Dictionary<string, List<int>>>(path);
            if(data.TryGetValue("RepeatEvents", out List<int> repeatableEvents))
            {
                foreach(var entry in repeatableEvents)
                {
                    Game1.player.eventsSeen.Remove(entry);
                }
            }
            if (data.TryGetValue("RepeatResponses", out List<int> repeatableResponses))
            {
                foreach (var entry in repeatableResponses)
                {
                    Game1.player.dialogueQuestionsAnswered.Remove(entry);
                }
            }
            Log.Trace("Removed all repeatable events");
        }

        private void printLocationModData(string arg1, string[] arg2)
        {
            if (arg2.Length < 1)
            {
                Log.Info("Location parameter needed");
                return;
            }
            GameLocation location = Game1.getLocationFromName(arg2[0]);
            if (location != null)
            {
                foreach(var key in location.modData.Keys)
                {
                    Log.Info($"{key}: {location.modData[key]}");
                }
            }
            Log.Info("Done");
        }

        private void OnGameLaunched(object sender, EventArgs e)
        {
            TileActionHandler.Initialize(Helper);
            ImageMenu.Setup(Helper);
            MapMenu.Setup(Helper);
            TrashCans.Setup(Helper);
            RSVWorldMap.Setup(Helper);
            ExternalAPIs.Initialize(Helper);



            Config = Helper.ReadConfig<ModConfig>();

            if (!Helper.ModRegistry.IsLoaded("spacechase0.JsonAssets"))
            {
                Log.Error("JSON Assets is not loaded! This mod *requires* JSON Assets!");
                return;
            }

            // Custom CP Token Set-up
            CustomCPTokens.RegisterTokens();

            Helper.ConsoleCommands.Add("RSV_reset_pedestals", "", this.ResetPedestals);
            Helper.ConsoleCommands.Add("RSV_open_portal", "", this.OpenPortal);

            // Generic Mod Config Menu setup
            //ConfigMenu.RegisterMenu();
        }

        private void OpenPortal(string arg1, string[] arg2)
        {
            if (Context.IsWorldReady && this.SpiritShrine != null)
            {
                this.SpiritShrine.OpenPortal(arg1, arg2);
            }
        }

        private void ResetPedestals(string arg1, string[] arg2)
        {
            if (Context.IsWorldReady && this.SpiritShrine != null)
            {
                this.SpiritShrine.ResetPedestals(arg1, arg2);
            }
        }

        private void OnSaveLoaded(object sender, EventArgs ex)
        {
            try
            {
                Config = Helper.ReadConfig<ModConfig>();
            }
            catch (Exception e)
            {
                Log.Debug($"Failed to load config settings. Will use default settings instead. Error: {e}");
                Config = new ModConfig();
            }


            SpiritShrine = new SpiritShrine(this);


            //mark greenhouses as greenhouses, so trees can be planted
            List<string> locationsNames = new List<string>() { "Custom_Ridgeside_AguarCaveTemporary", "Custom_Ridgeside_RSVGreenhouse1", "Custom_Ridgeside_RSVGreenhouse2" };
            foreach (var name in locationsNames)
            {
                GameLocation location = Game1.getLocationFromName(name);
                if (location == null)
                {
                    Log.Trace($"{name} is null");
                    continue;
                }
                location.isGreenhouse.Value = true;
                Log.Trace($"{name} set to greenhouse");
            }

            //remove corrupted Fire SO if the player shouldnt have it
            var team = Game1.player.team;
            if (Game1.player.IsMainPlayer && team.SpecialOrderActive(("RSV.UntimedSpecialOrder.SpiritRealmFlames")))
            {
                //if player has NOT seen portal opening or HAS seen the cleansing event remove the fire quest
                if (!Game1.player.eventsSeen.Contains(75160256) || Game1.player.eventsSeen.Contains(75160263))
                {
                    for (int i = 0; i<team.specialOrders.Count; i++)
                    {
                        if (team.specialOrders[i].questKey.Equals("RSV.UntimedSpecialOrder.SpiritRealmFlames"))
                        {
                            team.specialOrders.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }
        private static void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            if ((Game1.player.mailReceived.Contains("RSV.TakenLoan")) & (Game1.player.IsMainPlayer))
            {
                Log.Trace($"MaiveLoan - begin interest calculations");
                int[] shippingCategoryTotals = new int[5];
                ICollection<Item> ShippingBin = (ICollection<Item>)Game1.getFarm().getShippingBin(Game1.player);
                Log.Trace($"MaiveLoan - Got shipping bin");
                foreach (Item item in ShippingBin)
                {
                    Log.Trace($"MaiveLoan - {item.Name} in shipping bin");
                    StardewValley.Object obj = (StardewValley.Object)(object)((item is StardewValley.Object) ? item : null);
                    if (obj != null)
                    {
                        int shippingCategory = GetShippingCategory(obj);
                        int sellPrice = obj.sellToStorePrice(-1L) * ((Item)obj).Stack;
                        shippingCategoryTotals[shippingCategory] += sellPrice;
                        Log.Trace($"MaiveLoan - {obj.Name} full price: {sellPrice}");
                    }
                }
                for (int i = 0; i < shippingCategoryTotals.Length; i++)
                {
                    int amount = shippingCategoryTotals[i] * 5 / 100;
                    Log.Trace($"MaiveLoan - deducting {amount} G");
                    if (amount > 0)
                    {
                        Game1.player.Money -= amount;
                        Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("RSV.LoanInterest")));
                        Log.Trace($"MaiveLoan - player money is {Game1.player.Money}");
                    }
                }
            }
        }

        public static int GetShippingCategory(StardewValley.Object obj)
        {
            return GetShippingCategory(((Item)obj).ParentSheetIndex, ((Item)obj).Category);
        }

        public static int GetShippingCategory(int objectID, int objectCategory)
        {
            switch (objectID)
            {
                case 296:
                case 396:
                case 402:
                case 406:
                case 410:
                case 414:
                case 418:
                    return 1;
                default:
                    switch (objectCategory)
                    {
                        case -20:
                        case -4:
                            return 2;
                        case -15:
                        case -12:
                        case -2:
                            return 3;
                        case -80:
                        case -79:
                        case -75:
                        case -26:
                        case -14:
                        case -6:
                        case -5:
                            return 0;
                        case -81:
                        case -27:
                        case -23:
                            return 1;
                        default:
                            return 4;
                    }
            }
        }

    }
}
