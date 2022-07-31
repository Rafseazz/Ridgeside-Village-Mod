using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RidgesideVillage
    {
    class ConfigMenu
        {
        private readonly IModHelper Helper;
        private readonly IManifest ModManifest;

        private ModConfig Config {
            get => ModEntry.Config;
            set => ModEntry.Config = value;
            }

        public ConfigMenu(IMod mod) {
            Helper = mod.Helper;
            ModManifest = mod.ModManifest;
            }

        public void RegisterMenu() {
            var Gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (Gmcm is null) return;

            Gmcm.RegisterModConfig(ModManifest, () => Config = new ModConfig(), () => Helper.WriteConfig(Config));

            //Misc Config Page
            Gmcm.RegisterLabel(ModManifest, "Mod Configuration", "Ridgeside SMAPI Component");
            Gmcm.RegisterSimpleOption(ModManifest, "Show Villagers on the map", "Show Villagers on the map", () => Config.ShowVillagersOnMap, (bool val) => Config.ShowVillagersOnMap = val);

        }
        }
    }
