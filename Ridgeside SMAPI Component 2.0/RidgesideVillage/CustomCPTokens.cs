using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace RidgesideVillage
    {
    class CustomCPTokens
        {
        private readonly IModHelper Helper;
        private readonly IManifest ModManifest;

        private ModConfig Config {
            get => ModEntry.Config;
            set => ModEntry.Config = value;
            }

        public CustomCPTokens(IMod mod) {
            Helper = mod.Helper;
            ModManifest = mod.ModManifest;
            }

        public void RegisterTokens() {
            var cp = Helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");
            if (cp is null) {
                Log.Alert("Content Patcher is not installed- RSV requires CP to run. Please install CP and restart your game.");
                return;
                }

            cp.RegisterToken(this.ModManifest, "PastoralMapStyle", () => new string[] { Config.pastoralMapStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "EnableRidgesideMusic", () => new string[] { Config.enableRidgesideMusic.ToString() });

            cp.RegisterToken(this.ModManifest, "RepeatCableCarCutscene", () => new string[] { Config.repeatCableCarCutscene.ToString() });

            cp.RegisterToken(this.ModManifest, "EnableOtherNPCsInCableCar", () => new string[] { Config.enableOtherNPCsInCableCar.ToString() });

            cp.RegisterToken(this.ModManifest, "AguarPortraitStyle", () => new string[] { Config.aguarPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "AlissaPortraitStyle", () => new string[] { Config.alissaPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "BertPortraitStyle", () => new string[] { Config.bertPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "CorinePortraitStyle", () => new string[] { Config.corinePortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "EzekielPortraitStyle", () => new string[] { Config.ezekielPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "FlorPortraitStyle", () => new string[] { Config.florPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "FreddiePortraitStyle", () => new string[] { Config.freddiePortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "IanPortraitStyle", () => new string[] { Config.ianPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "JericPortraitStyle", () => new string[] { Config.jericPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "JioPortraitStyle", () => new string[] { Config.jioPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "KeahiPortraitStyle", () => new string[] { Config.keahiPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "KennethPortraitStyle", () => new string[] { Config.kennethPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "KiwiPortraitStyle", () => new string[] { Config.kiwiPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "LennyPortraitStyle", () => new string[] { Config.lennyPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "LolaPortraitStyle", () => new string[] { Config.lolaPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "MaddiePortraitStyle", () => new string[] { Config.maddiePortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "OlgaPortraitStyle", () => new string[] { Config.olgaPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "PhillipPortraitStyle", () => new string[] { Config.phillipPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "PikaPortraitStyle", () => new string[] { Config.pikaPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "RichardPortraitStyle", () => new string[] { Config.richardPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "ShiroPortraitStyle", () => new string[] { Config.shiroPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "TrinniePortraitStyle", () => new string[] { Config.trinniePortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "YsabellePortraitStyle", () => new string[] { Config.ysabellePortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "YuumaPortraitStyle", () => new string[] { Config.yuumaPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "HelenPortraitStyle", () => new string[] { Config.helenPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "UndreyaPortraitStyle", () => new string[] { Config.undreyaPortraitStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "FlorSpriteStyle", () => new string[] { Config.florSpriteStyle ?? "Default" });

            cp.RegisterToken(this.ModManifest, "SpouseGender", () =>
            {
                // or save is currently loading
                if (SaveGame.loaded?.player != null || Context.IsWorldReady)
                {
                    var Spouse = Game1.getCharacterFromName(Game1.player.spouse);
                    if (Spouse != null)
                    {
                        string gender;
                        switch (Spouse.Gender)
                        {
                            case 0:
                                gender = "male";
                                break;
                            case 1:
                                gender = "female";
                                break;
                            default:
                                gender = "undefined";
                                break;
                        }
                        return new[] { gender };
                    }
                }
                // no save loaded (e.g. on the title screen)
                return null;
            });

            cp.RegisterToken(this.ModManifest, "Celebrant", () =>
            {
                // or save is currently loading
                if (SaveGame.loaded?.player != null || Context.IsWorldReady)
                {
                    return new[] { HotelMenu.GetTodaysBirthdayNPC() };
                }
                // no save loaded (e.g. on the title screen)
                return new[] { "" };
            });

            cp.RegisterToken(this.ModManifest, "CelebrantDisplayName", () =>
            {
                // or save is currently loading
                if (SaveGame.loaded?.player != null || Context.IsWorldReady)
                {
                    string NPCName = HotelMenu.GetTodaysBirthdayNPC();
                    NPC npc = Game1.getCharacterFromName(NPCName);
                    if(npc != null)
                    {
                        return new[] { npc.displayName };
                    }
                }
                // no save loaded (e.g. on the title screen)
                return new[] { "" };
            });

            cp.RegisterToken(this.ModManifest, "FoxbloomDay", () => {
                int? randomseed = (int?)(Game1.stats?.daysPlayed ?? SaveGame.loaded?.stats?.daysPlayed);
                if (randomseed is not null)
                {   //Seed the random with a seed that only changes every 28 days
                    Random random = new Random((int)Game1.uniqueIDForThisGame + (randomseed.Value / 28));
                    return new[] { (random.Next(1, 5) * 7).ToString() };
                }
                return null; //return null for an unready token.
            });
        }
    }
}
