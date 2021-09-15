using StardewModdingAPI;
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

            TileActionHandler.RegisterTileAction("PaulaCounter", OpenPaulaMenu);
        }

        private static void OpenPaulaMenu(string tileActionString = "")
        {
            if (Game1.player.health < (Game1.player.maxHealth * 0.8) || Game1.player.stamina < (Game1.player.MaxStamina * 0.8))
            {
                ClinicChoices();
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Clinic.Healthy"));
            }
        }
        private static void ClinicChoices()
        {
            var responses = new List<Response>
            {
                new Response("healthcheckup", Helper.Translation.Get("Clinic.Health") + $" : ${cost}"),
                new Response("staminacheckup", Helper.Translation.Get("Clinic.Stamina") + $" : ${cost}"),
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
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Clinic.Look"));
                Game1.player.Money -= cost;
                Game1.fadeScreenToBlack();
                Task.Delay(2000);
                Game1.playSound("pickUpItem");
                Task.Delay(1000);
                Game1.playSound("axe");
                Task.Delay(1000);
                Game1.player.health = Game1.player.maxHealth;
                Game1.playSound("healSound");
                Task.Delay(1000);
                Game1.fadeClear();
                Task.Delay(2000);
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Clinic.Done"));
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
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Clinic.Look"));
                Game1.player.Money -= cost;
                Game1.fadeScreenToBlack();
                Task.Delay(2000);
                Game1.playSound("pickUpItem");
                Task.Delay(1000);
                Game1.playSound("axe");
                Task.Delay(1000);
                Game1.player.stamina = Game1.player.MaxStamina;
                Game1.playSound("healSound");
                Task.Delay(1000);
                Game1.fadeClear();
                Task.Delay(2000);
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Clinic.Done"));
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
