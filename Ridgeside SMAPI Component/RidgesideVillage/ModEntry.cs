using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;

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

            new HotelMenu().Initialize(this);
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;

            new Minecarts().Initialize(this);
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;

        }




        private void OnGameLaunched(object sender, EventArgs e)
        {
            
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
