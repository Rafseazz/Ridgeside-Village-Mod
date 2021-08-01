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
        const string RECEPTIONMAILFLAG = "RSV.ReservedReception";
        const string RECEIVEDMAILWR = "WedReceptionMail";
        const string BIRTHDAYMAILFLAG = "RSV.BirthdayBooked";
        const string ENGAGEDFLAG = "RSV.IsEngagedFlag";

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

            //Removes wedding reception ID from being seen AFTER seeing it.
            if (Game1.player.eventsSeen.Contains(75160245))
            {
                Game1.player.eventsSeen.Remove(75160245);
                Game1.player.mailReceived.Remove(RECEPTIONMAILFLAG);
            }

            //Adds flag if player is engaged so the mail can be sent to the player
            if (Game1.player.isEngaged())
            {
                Game1.player.mailReceived.Add(ENGAGEDFLAG);
            }

            //Removes flags if player isn't engaged anymore
            //Removes the flags that identify the player has reserved an event and has seen the mail about the reception
            //Removes the wedding reception event so player can see new one after remarry
            if (!Game1.player.isEngaged())
            {
                Game1.player.mailReceived.Remove(ENGAGEDFLAG);
                Game1.player.mailReceived.Remove(RECEIVEDMAILWR);
                Game1.player.eventsSeen.Remove(75160246);
            }

            //If it's after wedding day and the player didn't attend their booked Wedding Reception
            if (!Game1.player.eventsSeen.Contains(75160245) && !Game1.weddingToday && Game1.player.mailReceived.Contains(RECEPTIONMAILFLAG))
            {
                Game1.player.mailReceived.Remove(RECEPTIONMAILFLAG);
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
            //Booking a room
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
                }
                else if (Game1.player.mailReceived.Contains(ROOMMAILFLAG))
                {
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("HotelCounter.Booking.AlreadyBooked"));
                }
                else if (Game1.player.Money < 500)
                {
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("HotelCounter.Booking.NotEnoughMoney"));

                }

            }

            //Reserving an event in event hall
            if (str != null && str.Contains("EventHallCounter"))
            {
                //If player isn't engaged and hasn't booked birthday event
                if (Game1.player.Money >= 2000 && !Game1.player.mailReceived.Contains(BIRTHDAYMAILFLAG) && !Game1.player.mailReceived.Contains(ENGAGEDFLAG))
                {
                    var responses = new List<Response>
                    {
                        new Response("yes", Helper.Translation.Get("EventHallCounter.Booking.Bday.Yes")),
                        new Response("no", Helper.Translation.Get("EventHallCounter.Booking.No"))
                    };

                    var responseActions = new List<Action>
                    {
                        delegate
                        {
                            Game1.player.Money -= 2000;
                            Game1.player.mailReceived.Add(BIRTHDAYMAILFLAG);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.Bday.AfterBooking"));
                        },
                        delegate { }
                    };

                    var eventChoices = new List<Response>
                    {
                        new Response("bday", Helper.Translation.Get("EventHallCounter.Booking.BirthdayParty")),
                        new Response("no", Helper.Translation.Get("EventHallCounter.Booking.No"))
                    };
                    var eventActions = new List<Action>
                    {
                        delegate
                        {
                            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Bday.Question"), responses, responseActions);
                        },
                        delegate { }
                    };

                    Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Question"), eventChoices, eventActions);
                }

                //If player is engaged and hasn't booked both events
                else if (Game1.player.Money >= 2000 && !Game1.player.mailReceived.Contains(BIRTHDAYMAILFLAG) && !Game1.player.mailReceived.Contains(RECEPTIONMAILFLAG) && Game1.player.mailReceived.Contains(ENGAGEDFLAG))
                {
                    var responsesBDAY = new List<Response>
                    {
                        new Response("yes", Helper.Translation.Get("EventHallCounter.Booking.Bday.Yes")),
                        new Response("no", Helper.Translation.Get("EventHallCounter.Booking.No"))
                    };

                    var responseActionsBDAY = new List<Action>
                    {
                        delegate
                        {
                            Game1.player.Money -= 2000;
                            Game1.player.mailReceived.Add(BIRTHDAYMAILFLAG);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.Bday.AfterBooking"));
                        },
                        delegate { }
                    };

                    var responsesWR = new List<Response>
                    {
                        new Response("yes", Helper.Translation.Get("EventHallCounter.Booking.WR.Yes")),
                        new Response("no", Helper.Translation.Get("EventHallCounter.Booking.No"))
                    };

                    var responseActionsWR = new List<Action>
                    {
                        delegate
                        {
                            Game1.player.Money -= 2000;
                            Game1.player.mailReceived.Add(RECEPTIONMAILFLAG);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.WR.AfterBooking"));
                        },
                        delegate { }
                    };

                    var eventChoices = new List<Response>
                    {
                        new Response("bday", Helper.Translation.Get("EventHallCounter.Booking.BirthdayParty")),
                        new Response("weddingreception", Helper.Translation.Get("EventHallCounter.Booking.WeddingReception")),
                        new Response("no", Helper.Translation.Get("EventHallCounter.Booking.No")),
                    };
                    var eventActions = new List<Action>
                    {
                        delegate
                        {
                            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Bday.Question"), responsesBDAY, responseActionsBDAY);
                        },
                        delegate
                        {
                            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Bday.Question"), responsesWR, responseActionsWR);
                        },
                        delegate { }
                    };

                    Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Question"), eventChoices, eventActions);
                }

                //If player is engaged and already reserved birthday event
                else if (Game1.player.Money >= 2000 && Game1.player.mailReceived.Contains(BIRTHDAYMAILFLAG) && !Game1.player.mailReceived.Contains(RECEPTIONMAILFLAG) && Game1.player.mailReceived.Contains(ENGAGEDFLAG))
                {
                    var responses = new List<Response>
                    {
                        new Response("yes", Helper.Translation.Get("EventHallCounter.Booking.WR.Yes")),
                        new Response("no", Helper.Translation.Get("EventHallCounter.Booking.No"))
                    };

                    var responseActions = new List<Action>
                    {
                        delegate
                        {
                            Game1.player.Money -= 2000;
                            Game1.player.mailReceived.Add(RECEPTIONMAILFLAG);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.WR.AfterBooking"));
                        },
                        delegate { }
                    };

                    var eventChoices = new List<Response>
                    {
                        new Response("bday", Helper.Translation.Get("EventHallCounter.Booking.BirthdayParty")),
                        new Response("weddingreception", Helper.Translation.Get("EventHallCounter.Booking.WeddingReception")),
                        new Response("no", Helper.Translation.Get("EventHallCounter.Booking.No"))
                    };
                    var eventActions = new List<Action>
                    {
                        delegate
                        {
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.AlreadyBooked"));
                        },
                        delegate
                        {
                            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.WR.Question"), responses, responseActions);
                        },
                        delegate { }
                    };

                    Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Question"), eventChoices, eventActions);
                }

                //If player has reserved wedding reception but not birthday
                else if (Game1.player.Money >= 2000 && !Game1.player.mailReceived.Contains(BIRTHDAYMAILFLAG) && Game1.player.mailReceived.Contains(RECEPTIONMAILFLAG))
                {
                    var responses = new List<Response>
                    {
                        new Response("yes", Helper.Translation.Get("EventHallCounter.Booking.Bday.Yes")),
                        new Response("no", Helper.Translation.Get("EventHallCounter.Booking.No"))
                    };

                    var responseActions = new List<Action>
                    {
                        delegate
                        {
                            Game1.player.Money -= 2000;
                            Game1.player.mailReceived.Add(BIRTHDAYMAILFLAG);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.Bday.AfterBooking"));
                        },
                        delegate { }
                    };

                    var eventChoices = new List<Response>
                    {
                        new Response("bday", Helper.Translation.Get("EventHallCounter.Booking.BirthdayParty")),
                        new Response("weddingreception", Helper.Translation.Get("EventHallCounter.Booking.WeddingReception")),
                        new Response("no", Helper.Translation.Get("EventHallCounter.Booking.No"))
                    };
                    var eventActions = new List<Action>
                    {
                        delegate
                        {
                            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Bday.Question"), responses, responseActions);
                        },
                        delegate
                        {
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.AlreadyBooked"));
                        },
                        delegate { }
                    };

                    Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Question"), eventChoices, eventActions);
                }

                //If player isn't and has booked birthday event
                else if (Game1.player.Money >= 2000 && Game1.player.mailReceived.Contains(BIRTHDAYMAILFLAG) && !Game1.player.mailReceived.Contains(ENGAGEDFLAG))
                {
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.AlreadyBooked"));
                }

                //If player has booked both events
                else if (Game1.player.Money >= 2000 && Game1.player.mailReceived.Contains(BIRTHDAYMAILFLAG) && Game1.player.mailReceived.Contains(RECEPTIONMAILFLAG))
                {
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.AlreadyBooked"));
                }

                //If player doesn't have enough money to book an event
                else if (Game1.player.Money < 2000)
                {
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.NotEnoughMoney"));
                }
            }
        }
    }
}
