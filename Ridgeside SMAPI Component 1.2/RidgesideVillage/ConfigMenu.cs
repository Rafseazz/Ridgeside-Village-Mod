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
            Gmcm.RegisterLabel(ModManifest, "Mod Appearance Configuration", "Pick your preferred graphics");
            Gmcm.RegisterChoiceOption(ModManifest, "Pastoral Map Style", "Pick your preferred Pastoral fall season map-style", () => Config.pastoralMapStyle, (string val) => Config.pastoralMapStyle = val, ModConfig.pastoralMapStyleChoices);
            Gmcm.RegisterLabel(ModManifest, "Cable Car Configuration", "Choose how the cable car cutscenes work");
            Gmcm.RegisterSimpleOption(ModManifest, "Enable Cable Car Cutscene", "Enable cable car cutscene", () => Config.repeatCableCarCutscene, (bool val) => Config.repeatCableCarCutscene = val);
            Gmcm.RegisterSimpleOption(ModManifest, "Enable Other NPCs in Cable Car", "Enable the chances of other NPCs to appear riding the Cable Car (Along with LASV, ES and SVE NPCs)", () => Config.enableOtherNPCsInCableCar, (bool val) => Config.enableOtherNPCsInCableCar = val);
            Gmcm.RegisterLabel(ModManifest, "Mod Audio Configuration", "Choose how the mod's audio works");
            Gmcm.RegisterSimpleOption(ModManifest, "Enable Ridgeside Village Music", "Enables music for Ridgeside Village", () => Config.enableRidgesideMusic, (bool val) => Config.enableRidgesideMusic = val);
            Gmcm.RegisterPageLabel(ModManifest, "Go to: Portrait and Sprite Configuration", "", "Portrait and Sprite Configuration");

            //Portrait Config Page
            Gmcm.StartNewPage(ModManifest, "Portrait and Sprite Configuration");
            Gmcm.RegisterLabel(ModManifest, "Character Portrait Configuration", "Pick your preferred portraits");
            Gmcm.RegisterChoiceOption(ModManifest, "Aguar Portrait Style", "Pick your preferred Aguar Portrait", () => Config.aguarPortraitStyle, (string val) => Config.aguarPortraitStyle = val, ModConfig.aguarPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Alissa Portrait Style", "Pick your preferred Alissa Portrait", () => Config.alissaPortraitStyle, (string val) => Config.alissaPortraitStyle = val, ModConfig.alissaPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Bert Portrait Style", "Pick your preferred Bert Portrait", () => Config.bertPortraitStyle, (string val) => Config.bertPortraitStyle = val, ModConfig.bertPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Corine Portrait Style", "Pick your preferred Corine Portrait", () => Config.corinePortraitStyle, (string val) => Config.corinePortraitStyle = val, ModConfig.corinePortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Ezekiel Portrait Style", "Pick your preferred Ezekiel Portrait", () => Config.ezekielPortraitStyle, (string val) => Config.ezekielPortraitStyle = val, ModConfig.ezekielPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Flor Portrait Style", "Pick your preferred Flor Portrait", () => Config.florPortraitStyle, (string val) => Config.florPortraitStyle = val, ModConfig.florPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Freddie Portrait Style", "Pick your preferred Freddie Portrait", () => Config.freddiePortraitStyle, (string val) => Config.freddiePortraitStyle = val, ModConfig.freddiePortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Ian Portrait Style", "Pick your preferred Ian Portrait", () => Config.ianPortraitStyle, (string val) => Config.ianPortraitStyle = val, ModConfig.ianPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Jeric Portrait Style", "Pick your preferred Jeric Portrait", () => Config.jericPortraitStyle, (string val) => Config.jericPortraitStyle = val, ModConfig.jericPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Jio Portrait Style", "Pick your preferred Jio Portrait", () => Config.jioPortraitStyle, (string val) => Config.jioPortraitStyle = val, ModConfig.jioPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Keahi Portrait Style", "Pick your preferred Keahi Portrait", () => Config.keahiPortraitStyle, (string val) => Config.keahiPortraitStyle = val, ModConfig.keahiPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Kiwi Portrait Style", "Pick your preferred Kiwi Portrait", () => Config.kiwiPortraitStyle, (string val) => Config.kiwiPortraitStyle = val, ModConfig.kiwiPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Lenny Portrait Style", "Pick your preferred Lenny Portrait", () => Config.lennyPortraitStyle, (string val) => Config.lennyPortraitStyle = val, ModConfig.lennyPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Lola Portrait Style", "Pick your preferred Lola Portrait", () => Config.lolaPortraitStyle, (string val) => Config.lolaPortraitStyle = val, ModConfig.lolaPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Maddie Portrait Style", "Pick your preferred Maddie Portrait", () => Config.maddiePortraitStyle, (string val) => Config.aguarPortraitStyle = val, ModConfig.maddiePortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Olga Portrait Style", "Pick your preferred Olga Portrait", () => Config.olgaPortraitStyle, (string val) => Config.olgaPortraitStyle = val, ModConfig.olgaPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Phillip Portrait Style", "Pick your preferred Phillip Portrait", () => Config.phillipPortraitStyle, (string val) => Config.phillipPortraitStyle = val, ModConfig.phillipPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Pika Portrait Style", "Pick your preferred Pika Portrait", () => Config.pikaPortraitStyle, (string val) => Config.pikaPortraitStyle = val, ModConfig.pikaPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Richard Portrait Style", "Pick your preferred Richard Portrait", () => Config.richardPortraitStyle, (string val) => Config.richardPortraitStyle = val, ModConfig.richardPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Shiro Portrait Style", "Pick your preferred Shiro Portrait", () => Config.shiroPortraitStyle, (string val) => Config.shiroPortraitStyle = val, ModConfig.shiroPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Trinnie Portrait Style", "Pick your preferred Trinnie Portrait", () => Config.trinniePortraitStyle, (string val) => Config.trinniePortraitStyle = val, ModConfig.trinniePortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Ysabelle Portrait Style", "Pick your preferred Ysabelle Portrait", () => Config.ysabellePortraitStyle, (string val) => Config.ysabellePortraitStyle = val, ModConfig.ysabellePortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Yuuma Portrait Style", "Pick your preferred Yuuma Portrait", () => Config.yuumaPortraitStyle, (string val) => Config.yuumaPortraitStyle = val, ModConfig.yuumaPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Helen Portrait Style", "Pick your preferred Helen Portrait", () => Config.helenPortraitStyle, (string val) => Config.helenPortraitStyle = val, ModConfig.helenPortraitStyleChoices);
            Gmcm.RegisterChoiceOption(ModManifest, "Undreya Portrait Style", "Pick your preferred Undreya Portrait", () => Config.undreyaPortraitStyle, (string val) => Config.undreyaPortraitStyle = val, ModConfig.undreyaPortraitStyleChoices);
            Gmcm.RegisterLabel(ModManifest, "Character Sprite Configuration", "Pick your preferred sprites");
            Gmcm.RegisterChoiceOption(ModManifest, "Flor Sprite Style", "Pick your preferred Flor Sprite", () => Config.florSpriteStyle, (string val) => Config.florSpriteStyle = val, ModConfig.florSpriteStyleChoices);
            Gmcm.RegisterPageLabel(ModManifest, "Back to main page", "", "");

            }
        }
    }
