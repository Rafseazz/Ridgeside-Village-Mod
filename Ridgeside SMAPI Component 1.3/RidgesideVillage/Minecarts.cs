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

namespace RidgesideVillage
{
    internal class Minecarts
    {
        IModHelper Helper;
        IMonitor Monitor;
        internal void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        internal void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            //Checks if player can move
            bool probablyDontCheck =
            !StardewModdingAPI.Context.CanPlayerMove
            || Game1.player.isRidingHorse()
            || Game1.currentLocation == null
            || Game1.eventUp
            || Game1.isFestival()
            || Game1.IsFading();
            //Will only trigger if player can move
            if (probablyDontCheck)
            {
                return;
            }

            if (!e.Button.IsActionButton())
                return;
            Vector2 clickedTile = Helper.Input.GetCursorPosition().GrabTile;
            string str = Game1.currentLocation.doesTileHaveProperty(((int)clickedTile.X), ((int)clickedTile.Y), "Action", "Buildings");
            
            if (str != null && str.Contains("RSVMinecart1"))
            {
                var choices = new List<Response>
                    {
                        new Response("loc2", Helper.Translation.Get("MinecartLocation.2")),
                        new Response("loc3", Helper.Translation.Get("MinecartLocation.3")),
                        new Response("cancel", Helper.Translation.Get("Exit.Text"))
                    };
                var selectionAction = new List<Action>
                    {
                        delegate
                        {
                            Game1.playSound("boulderCrack");
                            Game1.warpFarmer("Custom_Ridgeside_RidgesideVillage", 69, 16, false);
                        },
                        delegate
                        {
                            Game1.playSound("boulderCrack");
                            Game1.warpFarmer("Custom_Ridgeside_RidgesideVillage", 16, 82, false);
                        },
                        delegate { }
                    };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("RSV.Minecart.Question"), choices, selectionAction);
            }

            if (str != null && str.Contains("RSVMinecart2"))
            {
                var choices = new List<Response>
                    {
                        new Response("loc1", Helper.Translation.Get("MinecartLocation.1")),
                        new Response("loc3", Helper.Translation.Get("MinecartLocation.3")),
                        new Response("cancel", Helper.Translation.Get("Exit.Text"))
                    };
                var selectionAction = new List<Action>
                    {
                        delegate
                        {
                            Game1.playSound("boulderCrack");
                            Game1.warpFarmer("Custom_Ridgeside_RSVCableCar", 25, 18, false);
                        },
                        delegate
                        {
                            Game1.playSound("boulderCrack");
                            Game1.warpFarmer("Custom_Ridgeside_RidgesideVillage", 16, 82, false);
                        },
                        delegate { }
                    };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("RSV.Minecart.Question"), choices, selectionAction);
            }

            if (str != null && str.Contains("RSVMinecart3"))
            {
                var choices = new List<Response>
                    {
                        new Response("loc1", Helper.Translation.Get("MinecartLocation.1")),
                        new Response("loc2", Helper.Translation.Get("MinecartLocation.2")),
                        new Response("cancel", Helper.Translation.Get("Exit.Text"))
                    };
                var selectionAction = new List<Action>
                    {
                        delegate
                        {
                            Game1.playSound("boulderCrack");
                            Game1.warpFarmer("Custom_Ridgeside_RSVCableCar", 25, 18, false);
                        },
                        delegate
                        {
                            Game1.playSound("boulderCrack");
                            Game1.warpFarmer("Custom_Ridgeside_RidgesideVillage", 69, 16, false);
                        },
                        delegate { }
                    };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("RSV.Minecart.Question"), choices, selectionAction);
            }
        }
    }
}
