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
    internal class HotelMenu
    {
        const string ROOMMAILFLAG = "RSV.HotelRoomBooked";

        IModHelper Helper;
        IMonitor Monitor;
        internal void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.player.mailReceived.Contains(ROOMMAILFLAG))
            {
                Game1.player.mailReceived.Remove(ROOMMAILFLAG);
            }
        }

        internal void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;
            
            if (!Game1.currentLocation.Name.Equals("Custom_Ridgeside_LogCabinHotelLobby"))
            {
                return;
            }
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
            if (str != null && str.Contains("HotelCounter"))
            {
                if (Game1.player.Money >= 500 && !Game1.player.mailReceived.Contains(ROOMMAILFLAG))
                {
                    var responses = new List<Response>
                {
                    new Response("yes", Helper.Translation.Get("HotelCounter.Booking.Yes")),
                    new Response("no", Helper.Translation.Get("HotelCounter.Booking.No"))
                };
                    var responseActions = new List<Action>
                {
                    delegate
                    {
                        Game1.player.Money -= 500;
                        Game1.player.mailReceived.Add(ROOMMAILFLAG);
                        Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("HotelCounter.Booking.AfterBooking"));
                    },
                    delegate { }
                };

                    Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("HotelCounter.Booking.Question"), responses, responseActions);
                }else if (Game1.player.mailReceived.Contains(ROOMMAILFLAG))
                {
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("HotelCounter.Booking.AlreadyBooked"));
                }else if (Game1.player.Money < 500)
                {
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("HotelCounter.Booking.NotEnoughMoney"));

                }

            }
        }
    }
}
