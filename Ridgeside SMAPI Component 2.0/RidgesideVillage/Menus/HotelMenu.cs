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
using xTile.Dimensions;

namespace RidgesideVillage
{
    internal static class HotelMenu
    {
        const string REWARDLETTER = "RichardSOLetter";

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

            GameLocation.RegisterTileAction("HotelCounter", HandleHotelCounterMenu);
            GameLocation.RegisterTileAction("EventHallCounter", HandleEventHallMenu);
            GameLocation.RegisterTileAction("RatesCounter", HandleRatesMenu);
            GameLocation.RegisterTileAction("RSVHistoryScroll", HandleHistoryScroll);
            GameLocation.RegisterTileAction("BlissBook", HandleBlissBook);
        }

        //Informs player where there room is upon entering the 2nd floor
        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            if (Game1.currentLocation.Name.Contains(RSVConstants.L_HOTEL2) && Game1.player.mailReceived.Contains(RSVConstants.M_ROOMBOOKEDFLAG))
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("HotelRoom.DirectionsAlert"));
            }
        }

        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.player.mailReceived.Contains(RSVConstants.M_ROOMBOOKEDFLAG))
            {
                Game1.player.mailReceived.Remove(RSVConstants.M_ROOMBOOKEDFLAG);
            }

            //Removes wedding reception ID from being seen AFTER seeing it.
            if (Game1.player.eventsSeen.Contains(RSVConstants.E_WEDDINGRECEPTION))
            {
                Game1.player.eventsSeen.Remove(RSVConstants.E_WEDDINGRECEPTION);
                if (Game1.player.activeDialogueEvents.ContainsKey(RSVConstants.M_RECEPTIONBOOKEDFLAG))
                    Game1.player.activeDialogueEvents.Remove(RSVConstants.M_RECEPTIONBOOKEDFLAG);
            }

            //Adds flag if player is engaged so the mail can be sent to the player
            if (Game1.player.isEngaged())
            {
                Game1.player.mailReceived.Add(RSVConstants.M_ENGAGEDFLAG);
            }

            //Removes flags if player isn't engaged anymore
            //Removes the flags that identify the player has reserved an event and has seen the mail about the reception
            //Removes the wedding reception event so player can see new one after remarry
            if (!Game1.player.isEngaged())
            {
                Game1.player.mailReceived.Remove(RSVConstants.M_ENGAGEDFLAG);
                Game1.player.mailReceived.Remove(RSVConstants.M_RECEIVEDMAILWR);
            }

            //If it's after wedding day and the player didn't attend their booked Wedding Reception
            if (!(Game1.player.eventsSeen.Contains(RSVConstants.E_WEDDINGRECEPTION))
                && !Game1.weddingToday && Game1.player.activeDialogueEvents.ContainsKey(RSVConstants.M_RECEPTIONBOOKEDFLAG) &&Game1.player.activeDialogueEvents[RSVConstants.M_RECEPTIONBOOKEDFLAG] <= 0 && Game1.player.isMarriedOrRoommates())
            {
                Game1.player.eventsSeen.Remove(RSVConstants.E_WEDDINGRECEPTION);
                Game1.player.activeDialogueEvents.Remove(RSVConstants.M_RECEPTIONBOOKEDFLAG);
            }

            //removes booked flag next day
            if (!Game1.player.mailReceived.Contains(RSVConstants.M_BIRTHDAYBOOKEDFLAG))
            {
                foreach (var entry in Game1.player.mailReceived)
                {
                    if (entry.StartsWith(RSVConstants.M_BIRTHDAYBOOKED))
                    {
                        Game1.player.mailReceived.Remove(entry);
                    }
                }
            }

            if (Game1.player.eventsSeen.Contains(RSVConstants.E_BIRTHDAY) && Game1.player.mailReceived.Contains(RSVConstants.M_BIRTHDAYBOOKEDFLAG))
            {
                Game1.player.eventsSeen.Remove(RSVConstants.E_BIRTHDAY);
                Game1.player.mailReceived.Remove(RSVConstants.M_BIRTHDAYBOOKEDFLAG);
            }

            
            //Removes seeing and booking anniversary event after event
            if (Game1.player.eventsSeen.Contains(RSVConstants.E_ANNIVERSARY) && Game1.player.mailReceived.Contains(RSVConstants.M_ANNIVERSARYBOOKEDFLAG))
            {
                Game1.player.mailReceived.Remove(RSVConstants.M_ANNIVERSARYBOOKEDFLAG);
            }

            //Removes anvtoday flag after next day
            if (Game1.player.mailReceived.Contains(RSVConstants.M_ANNIVERSARYTODAY) && Game1.player.eventsSeen.Contains(RSVConstants.E_ANNIVERSARY))
            {
                Game1.player.eventsSeen.Remove(RSVConstants.E_ANNIVERSARY);
                Game1.player.mailReceived.Remove(RSVConstants.M_ANNIVERSARYTODAY);
            }

            //Adds ANNIVERSARYTODAY flag if it's the next day after booking
            if (Game1.player.mailReceived.Contains(RSVConstants.M_ANNIVERSARYBOOKEDFLAG) && !Game1.player.mailReceived.Contains(RSVConstants.M_ANNIVERSARYTODAY))
            {
                Game1.player.mailReceived.Add(RSVConstants.M_ANNIVERSARYTODAY);
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

        private static bool HandleRatesMenu(GameLocation location, string[] arg2, Farmer farmer, Point point)
        {
            Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("LogCabinHotel.Rates.Expanded"));
            return true;
        }

        private static bool HandleHistoryScroll(GameLocation location, string[] arg2, Farmer farmer, Point point)
        {
            Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("LogCabinHotel.HistoryScroll"));
            return true;
        }

        private static bool HandleBlissBook(GameLocation location, string[] arg2, Farmer farmer, Point point)
        {
            var responses = new Response[]
            {
                new Response("Aniv1st", Helper.Translation.Get("Aniv.1st")),
                new Response("Aniv2nd", Helper.Translation.Get("Aniv.2nd")),
                new Response("Aniv3rd", Helper.Translation.Get("Aniv.3rd")),
                new Response("cancel", Helper.Translation.Get("Exit.Text"))
            };
            var responseActions = new List<Action>
            {
                delegate
                {
                    string translation;
                    if (Helper.ModRegistry.IsLoaded("ZoeDoll.NPCLeilani"))
                    {
                        translation = Helper.Translation.Get("Aniv.Airyn-Leilani");
                    }
                    else
                    {
                        translation = Helper.Translation.Get("Aniv.Airyn");
                    }
                    Game1.activeClickableMenu = new LetterViewerMenu(translation);
                },
                delegate
                {
                    Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Aniv.ChickenNuggets"));
                },
                delegate
                {
                    Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Aniv.Yri"));
                },
                delegate{}
            };
            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("BlissBook.Title"), responses, responseActions);
            return true;
        }

        private static bool HandleEventHallMenu(GameLocation location, string[] arg2, Farmer farmer, Point point)
        {
            HandleEventHallMenu();
            return true;
        }
        private static bool HandleHotelCounterMenu(GameLocation location, string[] arg2, Farmer farmer, Point point)
        {
            HandleHotelCounterMenu(arg2);
            return true;
        }

        private static void HandleRatesMenu(string tileActionString, Vector2 position)
        {
            HandleRatesMenu(tileActionString, position);
        }

        private static void HandleHistoryScroll(string tileActionString, Vector2 position)
        {
            HandleHistoryScroll(tileActionString, position);
        }
        private static void HandleHotelCounterMenu(string[] tileActionString)
        {
            if (Game1.player.mailReceived.Contains(REWARDLETTER))
            {
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("HotelCounter.Booking.Free")));
            }
            else if (Game1.player.Money >= ROOMPRICE && !Game1.player.mailReceived.Contains(RSVConstants.M_ROOMBOOKEDFLAG))
            {
                var responses = new Response[]
                    {
                        new Response("yes", Helper.Translation.Get("HotelCounter.Booking.Yes")),
                        new Response("no", Helper.Translation.Get("HotelCounter.Booking.No"))
                    };
                var responseActions = new List<Action>
                    {
                        delegate
                        {
                            Game1.player.Money -= ROOMPRICE;
                            // All players can go bc fuck multiplayer
                            foreach(Farmer player in Game1.getAllFarmers())
                            {
                                player.mailReceived.Add(RSVConstants.M_ROOMBOOKEDFLAG);
                            }
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("HotelCounter.Booking.AfterBooking"));
                        },
                        delegate { }
                    };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("HotelCounter.Booking.Question"), responses, responseActions);
            }
            else if (Game1.player.mailReceived.Contains(RSVConstants.M_ROOMBOOKEDFLAG))
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("HotelCounter.Booking.AlreadyBooked"));
            }
            else if (Game1.player.Money < ROOMPRICE)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("HotelCounter.Booking.NotEnoughMoney"));

            }
        }

        private static void HandleEventHallMenu()
        {
            //If player doesn't have enough money to book an event
            if (Game1.player.Money < 1500)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.NotEnoughMoney"));
            }
            else if (Game1.currentLocation.GetWeather().weather.Value == Game1.weather_green_rain)
            {
                Game1.activeClickableMenu = new DialogueBox("Uh oh! Green rain!");
            }
            //If player has booked an event
            else if (HotelMenu.IsThereUpcomingBirthdayBooked() || Game1.player.activeDialogueEvents.ContainsKey(RSVConstants.M_RECEPTIONBOOKEDFLAG) || Game1.player.mailReceived.Contains(RSVConstants.M_ANNIVERSARYBOOKEDFLAG))
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
                if(!Game1.player.activeDialogueEvents.ContainsKey(RSVConstants.M_RECEPTIONBOOKEDFLAG) && Game1.player.isEngaged())
                {
                    Response receptionesponse = new Response("weddingReception", Helper.Translation.Get("EventHallCounter.Booking.WeddingReception"));
                    responses.Add(receptionesponse);

                    Action receptionAction = delegate {
                        HandleReceptionEventMenu();
                    };
                    responseActions.Add(receptionAction);
                }

                if(Game1.player.isMarriedOrRoommates())
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
                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Question"), responses.ToArray(), responseActions);
            }
        }

        private static void HandleReceptionEventMenu()
        {
            var responses = new Response[]
                    {
                        new Response("yes", Helper.Translation.Get("EventHallCounter.Booking.WR.Yes")),
                        new Response("no", Helper.Translation.Get("EventHallCounter.Booking.No"))
                    };

            var responseActions = new List<Action>
                    {
                        delegate
                        {
                            Game1.player.Money -= WEDDINGPRICE;
                            Game1.player.activeDialogueEvents.Add(RSVConstants.M_RECEPTIONBOOKEDFLAG, Game1.player.friendshipData[UtilFunctions.GetFiance(Game1.player)].CountdownToWedding+1);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.WR.AfterBooking"));
                        },
                        delegate { HandleEventHallMenu(); }
                    };
            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Question"), responses, responseActions);
        }

        private static void HandleBirthdayEventMenu()
        {
            //Do you want to throw party for 2k?
            var responses = new Response[]
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
            var NPCList = NPCBirthdaysToday();

            if (NPCList.Count == 0)
            {
                Game1.activeClickableMenu = new DialogueBox("No NPCs with birthday today!");
                return;
            }

            foreach(var NPCtuple in NPCList)
            {
                string NPCName = NPCtuple.Item1;
                NPC npc = Game1.getCharacterFromName(NPCName);
                if(npc is null)
                {
                    Log.Trace($"Couldn't find npc {NPCName} in birthday code/hotel menu");
                    continue;
                }
                responses.Add(new Response(NPCName, npc.displayName));
                responseActions.Add(delegate
                {
                    Game1.player.Money -= BIRTHDAYPRICE;
                    //Game1.player.mailReceived.Add(RSVConstants.M_BIRTHDAYBOOKED + NPCName + "." + NPCtuple.Item2);
                    //Game1.player.mailReceived.Add(RSVConstants.M_BIRTHDAYBOOKEDFLAG);
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("EventHallCounter.Booking.Bday.AfterBooking"));

                    Game1.getLocationFromName(RSVConstants.L_HALL).TryGetLocationEvents(out var assetName, out var events);
                    var NPCListCheck = new List<string> { "Aguar", "Alissa", "Bert", "Corine", "Ezekiel", "Flor", "Freddie", "Ian", "Jeric", "Keahi", "Kenneth", "Lenny", "Lola", "Maddie", "Olga", "Philip", "Pika", "Richard", "Shiro", "Trinnie", "Ysabelle", "Yuuma", "Naomi", "Kimpoi", "Malaya", "Alex", "Elliott", "Harvey", "Sam", "Sebastian", "Shane", "Abigail", "Emily", "Haley", "Leah", "Maru", "Penny", "Caroline", "Clint", "Demetrius", "Evelyn", "George", "Gus", "Jas", "Jodi", "Kenneth", "Lewis", "Linus", "Marnie", "Pam", "Pierre", "Robin", "Vincent", "Willy" };
                    Event BirthdayEvent = null;
                    if (NPCName == "Torts")
                    {
                        BirthdayEvent = new Event(events[$"{RSVConstants.E_BIRTHDAY}/n TortsParty"].Replace("{NPC}", NPCName), assetName, RSVConstants.E_BIRTHDAY);
                    }
                    else if (NPCListCheck.Contains(NPCName))
                    {
                        BirthdayEvent = new Event(events[$"{RSVConstants.E_BIRTHDAY}/n KnownParty"].Replace("{NPC}", NPCName), assetName, RSVConstants.E_BIRTHDAY);
                    }
                    else
                    {
                        BirthdayEvent = new Event(events[$"{RSVConstants.E_BIRTHDAY}/n OtherParty"].Replace("{NPC}", NPCName), assetName, RSVConstants.E_BIRTHDAY);
                    }

                    if (BirthdayEvent != null)
                    {
                        UtilFunctions.StartEvent(BirthdayEvent, RSVConstants.L_HALL, 1000, 1000);
                    }
                });
            }
            responses.Add(new Response("", Helper.Translation.Get("Exit.Text")));
            responseActions.Add(delegate
            {
                HandleBirthdayEventMenu();
            });

            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("EventHallCounter.Booking.Bday.List"), responses.ToArray(), responseActions);
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
                //Log.Debug($"checking {k?.Name}, {k?.Birthday_Season}");
                if (k.IsVillager && k.Birthday_Season != null && validSeasons.Contains(k.Birthday_Season.ToLower()) && Game1.player.friendshipData.ContainsKey(k.Name) && k.Name != "Krobus")
                {

                    SDate birthday = new SDate(k.Birthday_Day, k.Birthday_Season.ToLower());
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
            Log.Debug($"Birthdays in next {n} days: ");
            foreach(var entry in birthdayNPCs)
            {
                Log.Debug($"{entry.Item1} {entry.Item2}");
            }

            return birthdayNPCs;
        }

        private static HashSet<Tuple<string, string>> NPCBirthdaysToday()
        {
            HashSet<Tuple<string, string>> birthdayNPCs = new HashSet<Tuple<string, string>>();
            SDate todaysDate = SDate.Now();
            List<string> validSeasons = new List<string> { "spring", "summer", "fall", "winter" };

            foreach (NPC k in Utility.getAllCharacters())
            {
                //Log.Debug($"checking {k?.Name}, {k?.Birthday_Season}");
                if (k.IsVillager && k.Birthday_Season != null && validSeasons.Contains(k.Birthday_Season.ToLower()) && Game1.player.friendshipData.ContainsKey(k.Name) && k.Name != "Krobus")
                {

                    SDate birthday = new SDate(k.Birthday_Day, k.Birthday_Season.ToLower());
                    if (birthday == todaysDate)
                    {
                        birthdayNPCs.Add(new Tuple<string, string>(k.Name, $"{birthday.Day}-{birthday.Season}-{birthday.Year}"));
                    }
                }
            }
            Log.Debug("Birthdays today: ");
            foreach (var entry in birthdayNPCs)
            {
                Log.Debug($"{entry.Item1} {entry.Item2}");
            }

            return birthdayNPCs;
        }

        private static void HandleAnniversaryMenu()
        {
            var responses = new Response[]
            {
                new Response("yes", Helper.Translation.Get("EventHallCounter.Anv.Yes")),
                new Response("no", Helper.Translation.Get("EventHallCounter.Booking.No"))
            };

            var responseActions = new List<Action>
            {
                delegate
                {
                    Game1.player.Money -= WEDDINGPRICE;
                    Game1.player.mailReceived.Add(RSVConstants.M_ANNIVERSARYBOOKEDFLAG);
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
                if (entry.StartsWith(RSVConstants.M_BIRTHDAYBOOKED))
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
                if (entry.StartsWith(RSVConstants.M_BIRTHDAYBOOKED) && entry.Contains(date))
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
