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

        public void RegisterMenu()
        {
            var i18n = Helper.Translation;

            var GMCM = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (GMCM is not null)
            {
                GMCM.Register(ModManifest, () => Config = new ModConfig(), () => Helper.WriteConfig(Config));

                //Misc Config Page
                //Gmcm.RegisterLabel(ModManifest, "Mod Configuration", "Ridgeside SMAPI Component");
                GMCM.AddBoolOption(ModManifest, () => Config.ShowVillagersOnMap, (bool val) => Config.ShowVillagersOnMap = val, () => i18n.Get("ShowVillagersOnMap"), () => i18n.Get("ShowVillagersOnMap.Description"));
                GMCM.AddBoolOption(ModManifest, () => Config.ProgressiveStory, (bool val) => Config.ProgressiveStory = val, () => i18n.Get("ProgressiveStory"), () => i18n.Get("ProgressiveStory.Description"));
                GMCM.AddBoolOption(ModManifest, () => Config.RepeatCableCarCutscene, (bool val) => Config.RepeatCableCarCutscene = val, () => i18n.Get("RepeatCableCarCutscene"), () => i18n.Get("RepeatCableCarCutscene.Description"));
                GMCM.AddBoolOption(ModManifest, () => Config.EnableOtherNPCsInCableCar, (bool val) => Config.EnableOtherNPCsInCableCar = val, () => i18n.Get("EnableOtherNPCsInCableCar"), () => i18n.Get("EnableOtherNPCsInCableCar.Description"));
                GMCM.AddBoolOption(ModManifest, () => Config.EnableRidgesideMusic, (bool val) => Config.EnableRidgesideMusic = val, () => i18n.Get("EnableRidgesideMusic"), () => i18n.Get("EnableRidgesideMusic.Description"));
                GMCM.AddBoolOption(ModManifest, () => Config.RSVNPCSAttendFestivals, (bool val) => Config.RSVNPCSAttendFestivals = val, () => i18n.Get("RSVNPCSAttendFestivals"), () => i18n.Get("RSVNPCSAttendFestivals.Description"));
                GMCM.AddBoolOption(ModManifest, () => Config.ExpandedFestivalMaps, (bool val) => Config.ExpandedFestivalMaps = val, () => i18n.Get("ExpandedFestivalMaps"), () => i18n.Get("ExpandedFestivalMaps.Description"));
                GMCM.AddBoolOption(ModManifest, () => Config.EasyIntroduction, (bool val) => Config.EasyIntroduction = val, () => i18n.Get("EasyIntroduction"), () => i18n.Get("EasyIntroduction.Description"));
                GMCM.AddBoolOption(ModManifest, () => Config.EnableBetterBusStop, (bool val) => Config.EnableBetterBusStop = val, () => i18n.Get("EnableBetterBusStop"), () => i18n.Get("EnableBetterBusStop.Description"));
                GMCM.AddBoolOption(ModManifest, () => Config.PoleAtBackwoods, (bool val) => Config.PoleAtBackwoods = val, () => i18n.Get("PoleAtBackwoods"), () => i18n.Get("PoleAtBackwoods.Description"));
                GMCM.AddBoolOption(ModManifest, () => Config.SeasonalRSVMap, (bool val) => Config.SeasonalRSVMap = val, () => i18n.Get("SeasonalRSVMap"), () => i18n.Get("SeasonalRSVMap.Description"));
                GMCM.AddBoolOption(ModManifest, () => Config.EnableTouristNPCs, (bool val) => Config.EnableTouristNPCs = val, () => i18n.Get("EnableTouristNPCs"), () => i18n.Get("EnableTouristNPCs.Description"));
                GMCM.AddBoolOption(ModManifest, () => Config.EnableWTDRCompat, (bool val) => Config.EnableWTDRCompat = val, () => i18n.Get("EnableWTDRCompat"), () => i18n.Get("EnableWTDRCompat.Description"));
                GMCM.AddBoolOption(ModManifest, () => Config.ImmersiveJioMarriageDialogue, (bool val) => Config.ImmersiveJioMarriageDialogue = val, () => i18n.Get("ImmersiveJioMarriageDialogue"), () => i18n.Get("ImmersiveJioMarriageDialogue.Description"));
            }
            

            //Register CP Config Tokens
            var CP = ExternalAPIs.CP;
            if (CP is not null)
            {
                CP.RegisterToken(this.ModManifest, "ShowVillagersOnMap", () => new string[] { Config.ShowVillagersOnMap.ToString() });
                CP.RegisterToken(this.ModManifest, "ProgressiveStory", () => new string[] { Config.ProgressiveStory.ToString() });
                CP.RegisterToken(this.ModManifest, "RepeatCableCarCutscene", () => new string[] { Config.RepeatCableCarCutscene.ToString() });
                CP.RegisterToken(this.ModManifest, "EnableOtherNPCsInCableCar", () => new string[] { Config.EnableOtherNPCsInCableCar.ToString() });
                CP.RegisterToken(this.ModManifest, "EnableRidgesideMusic", () => new string[] { Config.EnableRidgesideMusic.ToString() });
                CP.RegisterToken(this.ModManifest, "RSVNPCSAttendFestivals", () => new string[] { Config.RSVNPCSAttendFestivals.ToString() });
                CP.RegisterToken(this.ModManifest, "ExpandedFestivalMaps", () => new string[] { Config.ExpandedFestivalMaps.ToString() });
                CP.RegisterToken(this.ModManifest, "EasyIntroduction", () => new string[] { Config.EasyIntroduction.ToString() });
                CP.RegisterToken(this.ModManifest, "EnableBetterBusStop", () => new string[] { Config.EnableBetterBusStop.ToString() });
                CP.RegisterToken(this.ModManifest, "PoleAtBackwoods", () => new string[] { Config.PoleAtBackwoods.ToString() });
                CP.RegisterToken(this.ModManifest, "SeasonalRSVMap", () => new string[] { Config.SeasonalRSVMap.ToString() });
                CP.RegisterToken(this.ModManifest, "EnableTouristNPCs", () => new string[] { Config.EnableTouristNPCs.ToString() });
                CP.RegisterToken(this.ModManifest, "EnableWTDRCompat", () => new string[] { Config.EnableWTDRCompat.ToString() });
                CP.RegisterToken(this.ModManifest, "ImmersiveJioMarriageDialogue", () => new string[] { Config.ImmersiveJioMarriageDialogue.ToString() });

            }

        }

        
    }
}
