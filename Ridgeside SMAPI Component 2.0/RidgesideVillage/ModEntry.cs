using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using SpaceCore.Events;
using AtraCore.Framework.ItemResolvers;

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

            if (!new InstallationChecker().checkInstallation(helper))
            {
                return;
            }

            new SaveMigration(helper);

            ConfigMenu = new ConfigMenu(this);
            CustomCPTokens = new CustomCPTokens(this);

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            SpaceEvents.OnEventFinished += OnEventFinished;

            helper.Events.Content.AssetRequested += OnAssetRequested;

            BgUtils.Initialize(this);

            TortsBackground.Initialize(this);
            CableCarBackground.Initialize(this);
            SummitRenovateMenu.Initialize(this);

            BloomProjectile.Initialize(this);
            MistProjectile.Initialize(this);
            Mistblade.Initialize(this);

            Patcher = new Patcher(this);
            Patcher.PerformPatching();

            HotelMenu.Initialize(this);

            SpiritRealm.Initialize(this);

            SpecialOrders.Initialize(this);

            Questing.QuestController.Initialize(this);

            IanShop.Initialize(this);

            Elves.Initialize(this);

            Greenhouses.Initialize(this);

            Loan.Initialize(this);

            WarpTotem.Initialize(this);

            PaulaClinic.Initialize(this);

            Offering.OfferingTileAction.Initialize(this);

            NightlyEvent.Initialize(this);

            NinjaBooks.Initialize(this);

            Foxbloom.Initialize(this);

            TravelingCart.Initialize(this);
            ChooseKQuery.Initialize(this);
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            AssetManager.LoadEmptyJsons(e);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            forgetRepeatableEvents();
        }

        private void forgetRepeatableEvents()
        {
            string path = PathUtilities.NormalizePath("assets/RepeatableEvents.json");
            var data = Helper.ModContent.Load<Dictionary<string, List<string>>>(path);
            if (data.TryGetValue("RepeatEvents", out List<string> repeatableEvents))
            {
                foreach (var entry in repeatableEvents)
                {
                    Game1.player.eventsSeen.Remove(entry);
                }
            }
            if (data.TryGetValue("RepeatResponses", out List<string> repeatableResponses))
            {
                foreach (var entry in repeatableResponses)
                {
                    Game1.player.dialogueQuestionsAnswered.Remove(entry);
                }
            }
            Log.Trace("Removed all repeatable events");
        }

        private void OnGameLaunched(object sender, EventArgs e)
        {
            ImageMenu.Setup(Helper);
            MapMenu.Setup(Helper);
            //TrashCans.Setup(Helper);
            RSVWorldMap.Setup(Helper);
            ExternalAPIs.Initialize(Helper);

            Config = Helper.ReadConfig<ModConfig>();

            // Custom CP Token Set-up
            CustomCPTokens.RegisterTokens();

            Helper.ConsoleCommands.Add("RSV_LocationModData", "show ModData of given location", printLocationModData);
            Helper.ConsoleCommands.Add("RSV_RemoveEquipment", "Remove all clothes and equipment from farmer", RemoveEquipment);
            Helper.ConsoleCommands.Add("RSV_ResetPedestals", "", ResetPedestals);
            Helper.ConsoleCommands.Add("RSV_OpenPortal", "", OpenPortal);
            Helper.ConsoleCommands.Add("RSV_ToggleCaveSpawn", "", ToggleCaveSpawn);
            Helper.ConsoleCommands.Add("RSV_ToggleLockedNPC", "", ToggleLockedNPC);
            // RSV_rivera_secret in Patches/WalletItem
            // Quest commands in Questing/QuestController
            // Secret santa gift test in Patches/SecretSantaGift
            // Artifact reset in Patches/TreasureItems

            // Generic Mod Config Menu setup
            ConfigMenu.RegisterMenu();
        }

        /* 
        //   SMAPI Events
        */

        private void OnSaveLoaded(object sender, EventArgs ex)
        {
            try
            {
                Config = Helper.ReadConfig<ModConfig>();
            }
            catch (Exception e)
            {
                Log.Debug($"RSV: Failed to load config settings. Will use default settings instead. Error details:\n{e}");
                Config = new ModConfig();
            }


            SpiritShrine = new SpiritShrine(this);

            //remove corrupted Fire SO if the player shouldnt have it
            var team = Game1.player.team;
            if (Game1.player.IsMainPlayer && team.SpecialOrderActive(RSVConstants.SO_CLEANSING))
            {
                //if player has NOT seen portal opening or HAS seen the cleansing event remove the fire quest
                if (!Game1.player.eventsSeen.Contains(RSVConstants.E_OPENPORTAL) || Game1.player.eventsSeen.Contains(RSVConstants.E_CLEANSED))
                {
                    for (int i = 0; i < team.specialOrders.Count; i++)
                    {
                        if (team.specialOrders[i].questKey.Equals(RSVConstants.SO_CLEANSING))
                        {
                            team.specialOrders.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        // Make it 12:30 AM after Ember of Resolutions event
        private void OnEventFinished(object sender, EventArgs e)
        {
            Event current = Game1.CurrentEvent;
#nullable enable
            string? name = current.FestivalName;
#nullable disable
            //Log.Trace($"RSV: Current event festivalName is {name}");
            if (name == "Ember of Resolutions")
            {
                try
                {
                    var festival = Helper.Reflection.GetField<Dictionary<string, string>>(current, "festivalData").GetValue()["file"];
                    if (festival != null && festival.Equals("winter28"))
                    {
                        Game1.timeOfDayAfterFade = 2430;
                        if (Game1.IsMasterGame)
                        {
                            foreach (GameLocation i in Game1.locations)
                            {
                                foreach (Vector2 position in new List<Vector2>(i.objects.Keys))
                                {
                                    // Since it's already set to 10 PM
                                    if (i.objects[position].minutesElapsed(150))
                                    {
                                        i.objects.Remove(position);
                                    }
                                }
                                if (i is Farm)
                                {
                                    (i as Farm).timeUpdate(150);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"RSV: Error at end of EoR festival:\n{ex}");
                }
            }
            //Log.Trace("RSV: Done with OnEventFinished");
        }

        /* 
        //   Console Commands
        */

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
                foreach (var key in location.modData.Keys)
                {
                    Log.Info($"{key}: {location.modData[key]}");
                }
            }
            Log.Info("Done");
        }

        private void RemoveEquipment(string arg1, string[] arg2)
        {
            Game1.player.hat.Value = null;
            Game1.player.shirt.Value = "-1";
            Game1.player.changeShirt("-1");
            Game1.player.shirtItem.Value = null;
            Game1.player.pants.Value = "-1";
            //Game1.player.changePants(Color.White);
            Game1.player.pantsItem.Value = null;
            Game1.player.UpdateClothing();

            try { Game1.player.boots?.Value.onUnequip(Game1.player); } catch { }
            Game1.player.boots.Value = null;
            Game1.player.changeShoeColor("12");

            try { Game1.player.leftRing?.Value.onUnequip(Game1.player); } catch { }
            Game1.player.leftRing.Value = null;
            try { Game1.player.rightRing?.Value.onUnequip(Game1.player); } catch { }
            Game1.player.rightRing.Value = null;
        }

        private void OpenPortal(string arg1, string[] arg2)
        {
            if (Context.IsWorldReady && this.SpiritShrine != null)
            {
                SpiritShrine.OpenPortal(arg1, arg2);
            }
        }

        private void ResetPedestals(string arg1, string[] arg2)
        {
            if (Context.IsWorldReady && this.SpiritShrine != null)
            {
                SpiritShrine.ResetPedestals(arg1, arg2);
            }
        }

        private void ToggleCaveSpawn(string arg1, string[] arg2)
        {
            if (SaveGame.loaded?.player != null || Context.IsWorldReady)
            {
                if (!Game1.player.IsMainPlayer)
                {
                    Log.Warn("Command failed.\nThis command can only be used by the host player in a multiplayer game.");
                    return;
                }
                if (!Game1.player.eventsSeen.Contains(RSVConstants.E_AGUAR_8H))
                {
                    Log.Warn("Command failed.\nThis command can only be used after Aguar's cave has been unlocked.");
                    return;
                }   
                if (Game1.player.dialogueQuestionsAnswered.Contains(RSVConstants.R_AGUAR_FLOWERS))
                {
                    Game1.player.dialogueQuestionsAnswered.Remove(RSVConstants.R_AGUAR_FLOWERS);
                    Game1.player.dialogueQuestionsAnswered.Add(RSVConstants.R_AGUAR_FRUIT);
                    Log.Info("Command succeeded.\nAguar cave spawn changed from FLOWERS to FRUIT.");
                }
                else if (Game1.player.dialogueQuestionsAnswered.Contains(RSVConstants.R_AGUAR_FRUIT))
                {
                    Game1.player.dialogueQuestionsAnswered.Remove(RSVConstants.R_AGUAR_FRUIT);
                    Game1.player.dialogueQuestionsAnswered.Add(RSVConstants.R_AGUAR_FLOWERS);
                    Log.Info("Command succeeded.\nAguar cave spawn changed from FRUIT to FLOWERS.");
                }

            }
        }

        private void ToggleLockedNPC(string arg1, string[] arg2)
        {
            if (SaveGame.loaded?.player != null || Context.IsWorldReady)
            {
                if (!Game1.player.IsMainPlayer)
                {
                    Log.Warn("Command failed.\nThis command can only be used by the host player in a multiplayer game.");
                    return;
                }
                var name = arg2[0].ToLower();
                if (!Dateables.unlock_rules.ContainsKey(name))
                {
                    Log.Warn("Command failed.\nNPC name not recognized. Please enter one of Anton, Bryle, Faye, Irene, Paula, or Zayne.");
                    return;
                }
                var unlock_rule = Dateables.unlock_rules[name].Split('/');
                if (!Game1.player.eventsSeen.Contains(unlock_rule[0]))
                {
                    Log.Warn("Command failed.\n8 heart event for " + name.ToUpper() + " not yet seen.");
                    return;
                }
                if (unlock_rule[1] == "r")
                {
                    int date_id = int.Parse(unlock_rule[2]);
                    int rival_id;
                    switch(name)
                    {
                        case "irene":
                            rival_id = date_id - 1;
                            break;
                        case "anton":
                            rival_id = date_id + 2;
                            break;
                        default:
                            rival_id = date_id + 1;
                            break;
                    }
                    if (Game1.player.dialogueQuestionsAnswered.Contains(date_id.ToString()))
                    {
                        Game1.player.dialogueQuestionsAnswered.Remove(date_id.ToString());
                        Game1.player.dialogueQuestionsAnswered.Add(rival_id.ToString());
                        Log.Info("NPC " + name.ToUpper() + " is no longer dateable.");
                    }
                    else
                    {
                        if (Game1.player.dialogueQuestionsAnswered.Contains(rival_id.ToString()))
                            Game1.player.dialogueQuestionsAnswered.Remove(rival_id.ToString());
                        Game1.player.dialogueQuestionsAnswered.Add(date_id.ToString());
                        Log.Info("NPC " + name.ToUpper() + " is now dateable.");
                    }
                }
                else if (unlock_rule[1] == "!m")
                {
                    string mail_id = unlock_rule[2];
                    if (Game1.player.mailReceived.Contains(mail_id))
                    {
                        Game1.player.mailReceived.Remove(mail_id);
                        Log.Info("NPC " + name.ToUpper() + " is now dateable.");
                    }
                    else
                    {
                        Game1.player.mailReceived.Add(mail_id);
                        Log.Info("NPC " + name.ToUpper() + " is no longer dateable.");
                    }
                }
                Log.Info("(Change may not take effect until you change locations.)");
            }
        }
    }
}
