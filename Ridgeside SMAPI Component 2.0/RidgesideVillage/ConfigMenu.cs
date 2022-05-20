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
            Gmcm.RegisterLabel(ModManifest, "Cable Car Configuration", "Choose how the cable car cutscenes work");
            Gmcm.RegisterSimpleOption(ModManifest, "Enable Cable Car Cutscene", "Enable cable car cutscene", () => Config.repeatCableCarCutscene, (bool val) => Config.repeatCableCarCutscene = val);
            Gmcm.RegisterSimpleOption(ModManifest, "Enable Other NPCs in Cable Car", "Enable the chances of other NPCs to appear riding the Cable Car (Along with LASV, ES and SVE NPCs)", () => Config.enableOtherNPCsInCableCar, (bool val) => Config.enableOtherNPCsInCableCar = val);
            Gmcm.RegisterLabel(ModManifest, "Mod Audio Configuration", "Choose how the mod's audio works");
            Gmcm.RegisterSimpleOption(ModManifest, "Enable Ridgeside Village Music", "Enables music for Ridgeside Village", () => Config.enableRidgesideMusic, (bool val) => Config.enableRidgesideMusic = val);

        }
        }
    }
