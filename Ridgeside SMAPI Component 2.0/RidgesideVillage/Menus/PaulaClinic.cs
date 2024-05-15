﻿using StardewModdingAPI;
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
using Netcode;

namespace RidgesideVillage
{
    internal static class PaulaClinic
    {
        const int cost = 500;

        static IModHelper Helper;
        static IMonitor Monitor;
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            GameLocation.RegisterTileAction("PaulaCounter", OpenPaulaMenu);
        }

        private static bool OpenPaulaMenu(GameLocation location, string[] arg2, Farmer farmer, Point point)

        {
            bool isSomeoneHere = UtilFunctions.IsSomeoneHere(14, 12, 3, 2);
            if (isSomeoneHere && (Game1.player.health < (Game1.player.maxHealth * 0.8) || Game1.player.stamina < (Game1.player.MaxStamina * 0.8)))
            {
                ClinicChoices();
            }
            else if (!isSomeoneHere)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Clinic.PaulaNotHere"));
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Clinic.Healthy"));
            }
            return true;
        }
        private static void ClinicChoices()
        {
            var responses = new Response[]
            {
                new Response("healthcheckup", Helper.Translation.Get("Clinic.Health") + $" : {cost}$"),
                new Response("staminacheckup", Helper.Translation.Get("Clinic.Stamina") + $" : {cost}$"),
                new Response("cancel", Helper.Translation.Get("Exit.Text"))
            };
            var responseActions = new List<Action>
            {
                delegate
                {
                    HealthCheckup();
                },
                delegate
                {
                    StaminaCheckup();
                },
                delegate{}
            };

            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("Clinic.Choices"), responses, responseActions);
        }

        private static void HealthCheckup()
        {
            if(Game1.player.health < (Game1.player.maxHealth * 0.8) && Game1.player.Money >= cost)
            {
                var location = Game1.getLocationFromName(RSVConstants.L_CLINIC);

                location.TryGetLocationEvents(out var assetName, out var events);
                
                string eventString = events["healthCheckup"].Replace("{cost}", cost.ToString());

                UtilFunctions.StartEvent(new Event(eventString), RSVConstants.L_CLINIC, 16, 15);

                Game1.delayedActions.Add(new DelayedAction(2000, delegate {
                    Game1.player.health = Game1.player.maxHealth;
                }));
            }
            else if(Game1.player.health >= 100 && Game1.player.Money >= cost)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Clinic.HealthyHealth"));
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("NotEnoughMoney"));
            }
        }

        private static void StaminaCheckup()
        {
            if (Game1.player.stamina < (Game1.player.MaxStamina * 0.8) && Game1.player.Money >= cost)
            {
                var location = Game1.getLocationFromName(RSVConstants.L_CLINIC);

                location.TryGetLocationEvents(out var assetName, out var events);///fade/message \"{{i18n: 87620002.3}}\"/warp farmer 8 21/pause 600/fade unfade/pause 1000/pause 2000/playSound pickUpItem/pause 1500/playSound axe/pause 200/playSound healSound/pause 1500/fade unfade/pause 1000/speak Paula \"All done!\"/pause 500/end";

                string eventString = events["staminaCheckup"].Replace("{cost}", cost.ToString());

                UtilFunctions.StartEvent(new Event(eventString), RSVConstants.L_CLINIC, 16, 15);

                Game1.delayedActions.Add(new DelayedAction(2000, delegate {
                    Game1.player.Stamina = Game1.player.MaxStamina;
                }));
            }
            else if (Game1.player.stamina >= 100 && Game1.player.Money >= cost)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Clinic.HealthyStamina"));
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("NotEnoughMoney"));
            }
        }

    }
  
}
