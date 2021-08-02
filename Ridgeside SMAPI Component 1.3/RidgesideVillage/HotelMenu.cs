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
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{
    internal class HotelMenu
    {
        const string ROOMBOOKEDFLAG = "RSV.HotelRoomBooked";
        const string RECEPTIONBOOKEDFLAG = "RSV.ReservedReception";
        const string RECEIVEDMAILWR = "WedReceptionMail";
        //const string BIRTHDAYBOOKEDFLAG = "RSV.BirthdayBooked";
        const string ENGAGEDFLAG = "RSV.IsEngagedFlag";
        const string BIRTHDAYBOOKED = "RSV.BirthdayBooked.";

        const int ROOMPRICE = 500;
        const int WEDDINGPRICE = 2000;
        const int BIRTHDAYPRICE = 2000;

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
            if (Game1.player.mailReceived.Contains(ROOMBOOKEDFLAG))
            {
                Game1.player.mailReceived.Remove(ROOMBOOKEDFLAG);
            }

            //Removes wedding reception ID from being seen AFTER seeing it.
            if (Game1.player.eventsSeen.Contains(75160245))
            {
                Game1.player.eventsSeen.Remove(75160245);
                Game1.player.mailReceived.Remove(RECEPTIONBOOKEDFLAG);
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
            if (!Game1.player.eventsSeen.Contains(75160245) && !Game1.weddingToday && Game1.player.mailReceived.Contains(RECEPTIONBOOKEDFLAG))
            {
                Game1.player.mailReceived.Remove(RECEPTIONBOOKEDFLAG);
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
                HandleHotelCounterMenu();
            }

            //Reserving an event in event hall
            if (str != null && str.Contains("EventHallCounter"))
            {
                HandleEventHallMenu();
            }
        }

        private void HandleHotelCounterMenu()
        {
            if (Game1.player.Money >= 500 && !Game1.player.mailReceived.Contains(ROOMBOOKEDFLAG))
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
                            Game1.player.mailReceived.Add(ROOMBOOKEDFLAG);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("HotelCounter.Booking.AfterBooking"));
                        },
                        delegate { }
                    };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("HotelCounter.Booking.Question"), responses, responseActions);
            }
            else if (Game1.player.mailReceived.Contains(ROOMBOOKEDFLAG))
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("HotelCounter.Booking.AlreadyBooked"));
            }
            else if (Game1.player.Money < 500)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("HotelCounter.Booking.NotEnoughMoney"));

            }
        }

        private void HandleEventHallMenu()
        {
            //If player doesn't have enough money to book an event
            if (Game1.player.Money < 2000)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.NotEnoughMoney"));
            }            
            //If player has booked both events
            else if (HotelMenu.IsThereUpcomingBirthdayBooked() || (Game1.player.mailReceived.Contains(RECEPTIONBOOKEDFLAG)))
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.AlreadyBooked"));
            }
            //player has enough money and not booked both events
            else
            {
                var responses = new List<Response>();
                var responseActions = new List<Action>();

                //handle birthday booking
                if (!HotelMenu.IsThereUpcomingBirthdayBooked())
                {
                    //can throw birthday party
                    Response birthdayResponse = new Response("bday", Helper.Translation.Get("EventHallCounter.Booking.BirthdayParty"));
                    responses.Add(birthdayResponse);

                    Action birthdayAction = delegate {
                        HandleBirthdayEventMenu();
                        };
                    responseActions.Add(birthdayAction);

                }
                if(!Game1.player.mailReceived.Contains(RECEPTIONBOOKEDFLAG) && Game1.player.isEngaged())
                {
                    Response receptionesponse = new Response("bday", Helper.Translation.Get("EventHallCounter.Booking.BirthdayParty"));
                    responses.Add(receptionesponse);

                    Action receptionAction = delegate {
                        HandleReceptionEventMenu();
                    };
                    responseActions.Add(receptionAction);
                }

                responses.Add(new Response("no", Helper.Translation.Get("HotelCounter.Booking.No")));
                responseActions.Add(delegate { });
                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Question"), responses, responseActions);
            }
        }

        private void HandleReceptionEventMenu()
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
                            Game1.player.Money -= WEDDINGPRICE;
                            Game1.player.mailReceived.Add(RECEPTIONBOOKEDFLAG);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.WR.AfterBooking"));
                        },
                        delegate { HandleEventHallMenu(); }
                    };
            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Question"), responses, responseActions);
        }

        private void HandleBirthdayEventMenu()
        {
            //Do you want to throw party for 2k?
            var responses = new List<Response>
                    {
                        new Response("yes", Helper.Translation.Get("EventHallCounter.Booking.Bday.Yes")),
                        new Response("no", Helper.Translation.Get("EventHallCounter.Booking.No"))
                    };

            var responseActions = new List<Action>
                    {
                        delegate
                        {
                            HandleBirthDayNPCSelectionMenu();
                        },
                        delegate { HandleHotelCounterMenu(); }
                    };

            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Question"), responses, responseActions);
        }
        private void HandleBirthDayNPCSelectionMenu()
        {

            var responses = new List<Response>();
            var responseActions = new List<Action>();
            var NPCList = NPCBirthdaysInNextNDays(10);

            foreach(var NPCtuple in NPCList)
            {
                string NPCName = NPCtuple.Item1;
                responses.Add(new Response(NPCName, Game1.getCharacterFromName(NPCName).displayName));
                responseActions.Add(delegate
                {
                    Game1.player.Money -= BIRTHDAYPRICE;
                    Game1.player.mailReceived.Add(BIRTHDAYBOOKED + NPCName + "." + NPCtuple.Item2);
                });
            }
            responses.Add(new Response("", Helper.Translation.Get("Exit.Text")));
            responseActions.Add(delegate
            {
                HandleBirthdayEventMenu();
            });

            Game1.activeClickableMenu = new DialogueBoxWithActions("Imagine a dialogue to chose a birthday NPC here.", responses, responseActions);
        }

        private HashSet<Tuple<string, string>> NPCBirthdaysInNextNDays(int n)
        {
            HashSet<Tuple<string, string>> birthdayNPCs = new HashSet<Tuple<string, string>>();
            SDate startDate = SDate.Now().AddDays(1);
            SDate endDate = startDate.AddDays(n);
            SDate todaysDate = SDate.Now();
            List<string> validSeasons = new List<string>{ "spring", "summer", "fall", "winter" };

            List<SDate> datesToCheck = new List<SDate>();


            foreach (NPC k in Utility.getAllCharacters())
            {
                Log.Debug($"checking {k?.Name}, {k?.Birthday_Season}");
                if (k.isVillager() && k.Birthday_Season != null && validSeasons.Contains(k.Birthday_Season.ToLower()) && (Game1.player.friendshipData.ContainsKey(k.Name)))
                {

                    SDate birthday = new SDate(k.Birthday_Day, k.Birthday_Season);
                    if(birthday < todaysDate)
                    {
                        //if birthday in the past, add a year
                        birthday = birthday.AddDays(112);
                    }
                    if(startDate < birthday && birthday <= endDate)
                    {
                        birthdayNPCs.Add(new Tuple<string, string>(k.Name, $"{birthday.Day}-{birthday.Season}-{birthday.Year}"));
                    }
                }
            }
            Log.Debug("Birthdays: ");
            foreach(var entry in birthdayNPCs)
            {
                Log.Debug($"{entry.Item1} {entry.Item2}");
            }

            return birthdayNPCs;
        }

        internal static bool IsThereUpcomingBirthdayBooked()
        {
            SDate today = SDate.Now();
            foreach (var entry in Game1.player.mailReceived)
            {
                if (entry.StartsWith(BIRTHDAYBOOKED))
                {
                    Log.Debug($"found birthday {entry}");
                    var split = entry.Split('.');
                    if (split.Length == 4)
                    {
                        var splitDate = split[3].Split('-');
                        if(splitDate.Length == 3)
                        {
                            SDate date = new SDate(int.Parse(splitDate[0]), splitDate[1], int.Parse(splitDate[2]));
                            Log.Debug($"today is {today}, party is  {date}");
                            if(date >= today)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        internal static string GetTodaysBirthdayNPC()
        {
            SDate today = SDate.Now();
            string date = $"{today.Day}-{today.Season}-{today.Year}";
            foreach (var entry in Game1.player.mailReceived)
            {
                if (entry.StartsWith(BIRTHDAYBOOKED) && entry.Contains(date))
                {
                    var split = entry.Split('.');
                    if(split.Length == 4)
                    {
                        return split[2];
                    }
                }
            }

            return null;
        }
    }

  
}
