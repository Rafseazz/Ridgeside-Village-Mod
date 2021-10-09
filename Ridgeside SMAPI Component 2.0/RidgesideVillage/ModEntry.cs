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

        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            Helper = helper;

            ConfigMenu = new ConfigMenu(this);
            CustomCPTokens = new CustomCPTokens(this);
            Patcher = new Patcher(this);

            HotelMenu.Initialize(this);
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            Minecarts.Initialize(this);
            SpiritRealm.Initialize(this);

            SpecialOrders.Initialize(this);

            IanShop.Initialize(this);

            Greenhouses.Initialize(this);

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
            if(location != null)
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
            TileActionHandler.Initialize(this);
            ImageMenu.Setup(Helper);
            MapMenu.Setup(Helper);
            TrashCans.Setup(Helper);
            RSVWorldMap.Setup(Helper);
            ExternalAPIs.Initialize(Helper);
            //var mapData = new MapData();
            //Helper.Data.WriteJsonFile("MapData.json", mapData);


            Config = Helper.ReadConfig<ModConfig>();

            if (!Helper.ModRegistry.IsLoaded("spacechase0.JsonAssets"))
            {
                Log.Error("JSON Assets is not loaded! This mod *requires* JSON Assets!");
                return;
            }
            Patcher.PerformPatching();

            // Custom CP Token Set-up
            CustomCPTokens.RegisterTokens();

            // Generic Mod Config Menu setup
            //ConfigMenu.RegisterMenu();
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
        }

    }
}
