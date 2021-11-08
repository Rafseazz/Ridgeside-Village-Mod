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
    internal static class HotelMenu
    {
        const string ROOMBOOKEDFLAG = "RSV.HotelRoomBooked";
        const string RECEPTIONBOOKEDFLAG = "RSV.ReservedReception";
        const string RECEIVEDMAILWR = "WedReceptionMail";
        const string BIRTHDAYBOOKEDFLAG = "RSV.BirthdayBooked";
        const string ENGAGEDFLAG = "RSV.IsEngagedFlag";
        const string BIRTHDAYBOOKED = "RSV.BirthdayBooked.";
        const string ANNIVERSARYBOOKEDFLAG = "RSV.ReservedAnv";
        const string ANNIVERSARYTODAY = "RSV.AnvToday";

        const int ROOMPRICE = 500;
        const int WEDDINGPRICE = 2000;
        const int BIRTHDAYPRICE = 1500;

        static IModHelper Helper;
        static IMonitor Monitor;
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            //Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.Player.Warped += OnWarped;

            TileActionHandler.RegisterTileAction("HotelCounter", HandleHotelCounterMenu);
            TileActionHandler.RegisterTileAction("EventHallCounter", HandleEventHallMenu);
            TileActionHandler.RegisterTileAction("RatesCounter", HandleRatesMenu);
        }

        //Informs player where there room is upon entering the 2nd floor
        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            if (Game1.currentLocation.Name.Contains("Custom_Ridgeside_LogCabinHotel2ndFloor") && Game1.player.mailReceived.Contains(ROOMBOOKEDFLAG))
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("HotelRoom.DirectionsAlert"));
            }
        }

        private static void OnDayStarted(object sender, DayStartedEventArgs e)
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
            if (!Game1.player.eventsSeen.Contains(75160245) && !Game1.weddingToday && Game1.player.mailReceived.Contains(RECEPTIONBOOKEDFLAG) && Game1.player.isMarried())
            {
                Game1.player.mailReceived.Remove(RECEPTIONBOOKEDFLAG);
            }

            //removes booked flag next day
            if (!Game1.player.mailReceived.Contains(BIRTHDAYBOOKEDFLAG))
            {
                foreach (var entry in Game1.player.mailReceived)
                {
                    if (entry.StartsWith(BIRTHDAYBOOKED))
                    {
                        Game1.player.mailReceived.Remove(entry);
                    }
                }
            }

            if (Game1.player.eventsSeen.Contains(75160247) && Game1.player.mailReceived.Contains(BIRTHDAYBOOKEDFLAG))
            {
                Game1.player.eventsSeen.Remove(75160247);
                Game1.player.mailReceived.Remove(BIRTHDAYBOOKEDFLAG);
            }

            
            //Removes seeing and booking anniversary event after event
            if (Game1.player.eventsSeen.Contains(75160248) && Game1.player.mailReceived.Contains(ANNIVERSARYBOOKEDFLAG))
            {
                Game1.player.mailReceived.Remove(ANNIVERSARYBOOKEDFLAG);
            }

            //Removes anvtoday flag after next day
            if (Game1.player.mailReceived.Contains(ANNIVERSARYTODAY) && Game1.player.eventsSeen.Contains(75160248))
            {
                Game1.player.mailReceived.Remove(ANNIVERSARYTODAY);
                Game1.player.eventsSeen.Remove(75160248);
            }

            //Adds ANNIVERSARYTODAY flag if it's the next day after booking
            if (Game1.player.mailReceived.Contains(ANNIVERSARYBOOKEDFLAG))
            {
                Game1.player.mailReceived.Add(ANNIVERSARYTODAY);
            }

            //Alerts player on wake up about birthday party
            string npcName = GetTodaysBirthdayNPC();
            if ( npcName != null)
            {
                NPC npc = Game1.getCharacterFromName(npcName);
                if (npc != null)
                {
                    string alertText = Helper.Translation.Get("EventHall.TodayBirthday", new { name = npc.displayName });
                    Game1.activeClickableMenu = new DialogueBox(alertText);
                }
            }
        }

        private static void HandleRatesMenu(string tileActionString = "")
        {
            Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("LogCabinHotel.Rates.Expanded"));
        }

        private static void HandleEventHallMenu(string tileActionString, Vector2 position)
        {
            HandleEventHallMenu(tileActionString);
        }
        private static void HandleHotelCounterMenu(string tileActionString, Vector2 position)
        {
            HandleHotelCounterMenu(tileActionString);
        }

        private static void HandleRatesMenu(string tileActionString, Vector2 position)
        {
            HandleRatesMenu(tileActionString);
        }
        private static void HandleHotelCounterMenu(string tileActionString = "")
        {
            if (Game1.player.Money >= ROOMPRICE && !Game1.player.mailReceived.Contains(ROOMBOOKEDFLAG))
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
                            Game1.player.Money -= ROOMPRICE;
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
            else if (Game1.player.Money < ROOMPRICE)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("HotelCounter.Booking.NotEnoughMoney"));

            }
        }

        private static void HandleEventHallMenu(string tileActionString = "")
        {
            //If player doesn't have enough money to book an event
            if (Game1.player.Money < 1500)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.NotEnoughMoney"));
            }            
            //If player has booked an event
            else if (HotelMenu.IsThereUpcomingBirthdayBooked() || Game1.player.mailReceived.Contains(RECEPTIONBOOKEDFLAG) || Game1.player.mailReceived.Contains(ANNIVERSARYBOOKEDFLAG))
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.AlreadyBooked"));
            }
            //player has enough money and not any event
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
                    Response receptionesponse = new Response("weddingReception", Helper.Translation.Get("EventHallCounter.Booking.WeddingReception"));
                    responses.Add(receptionesponse);

                    Action receptionAction = delegate {
                        HandleReceptionEventMenu();
                    };
                    responseActions.Add(receptionAction);
                }

                if(Game1.player.isMarried())
                {
                    Response anvresponse = new Response("anvParty", Helper.Translation.Get("EventHallCounter.Anv.Title"));
                    responses.Add(anvresponse);

                    Action anvAction = delegate
                    {
                        HandleAnniversaryMenu();
                    };
                    responseActions.Add(anvAction);
                }

                responses.Add(new Response("no", Helper.Translation.Get("HotelCounter.Booking.No")));
                responseActions.Add(delegate { });
                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Question"), responses, responseActions);
            }
        }

        private static void HandleReceptionEventMenu()
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

        private static void HandleBirthdayEventMenu()
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
                        delegate { HandleEventHallMenu(); }
                    };

            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Question"), responses, responseActions);
        }
        private static void HandleBirthDayNPCSelectionMenu()
        {

            var responses = new List<Response>();
            var responseActions = new List<Action>();
            var NPCList = NPCBirthdaysInNextNDays(3);

            foreach(var NPCtuple in NPCList)
            {
                string NPCName = NPCtuple.Item1;
                responses.Add(new Response(NPCName, Game1.getCharacterFromName(NPCName).displayName));
                responseActions.Add(delegate
                {
                    Game1.player.Money -= BIRTHDAYPRICE;
                    Game1.player.mailReceived.Add(BIRTHDAYBOOKED + NPCName + "." + NPCtuple.Item2);
                    Game1.player.mailReceived.Add(BIRTHDAYBOOKEDFLAG);
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.Bday.AfterBooking"));
                });
            }
            responses.Add(new Response("", Helper.Translation.Get("Exit.Text")));
            responseActions.Add(delegate
            {
                HandleBirthdayEventMenu();
            });

            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Bday.List"), responses, responseActions);
        }

        private static HashSet<Tuple<string, string>> NPCBirthdaysInNextNDays(int n)
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
                    if(startDate <= birthday && birthday <= endDate)
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

        private static void HandleAnniversaryMenu()
        {
            var responses = new List<Response>
            {
                new Response("yes", Helper.Translation.Get("EventHallCounter.Anv.Yes")),
                new Response("no", Helper.Translation.Get("EventHallCounter.Booking.No"))
            };

            var responseActions = new List<Action>
            {
                delegate
                {
                    Game1.player.Money -= WEDDINGPRICE;
                    Game1.player.mailReceived.Add(ANNIVERSARYBOOKEDFLAG);
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Anv.AfterBooking"));
                },
                delegate
                {
                    HandleEventHallMenu();
                }
            };
            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Anv.Question"), responses, responseActions);
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

        //returns name of NPC if it has birthday today AND there is a birthdayevent booked
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
