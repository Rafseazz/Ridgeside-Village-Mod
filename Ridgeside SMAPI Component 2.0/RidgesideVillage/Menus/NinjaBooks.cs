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
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;
using Netcode;

namespace RidgesideVillage
{
    internal static class NinjaBooks
    {
        static IModHelper Helper;
        static IMonitor Monitor;

        static bool dialogueShown = false;

        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            GameLocation.RegisterTileAction("RSVOpenDaiaBook", RSVOpenDaiaBook);
            GameLocation.RegisterTileAction("MyLetter", MyLetter);
            GameLocation.RegisterTileAction("RSVFoxbloomHint", GetFoxbloomHint);
        }

        private static bool GetFoxbloomHint(GameLocation location, string[] arg2, Farmer farmer, Point point)
        {
            if (!Game1.player.eventsSeen.Contains(RSVConstants.E_CLEANSED))
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("FoxbloomHint.Uncleansed"));
                return true;
            }

            if (!dialogueShown)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("FoxbloomHint.Cleansed"));
                Game1.afterDialogues = OnFirstExit;
                dialogueShown = true;
            }
            else
            {
                OnFirstExit();
            }

            return true;
        }

        private static void OnFirstExit()
        {
            LetterViewerMenu letter = new(Helper.Translation.Get("FoxbloomHint.Text"));
            Game1.activeClickableMenu = letter;
            Game1.activeClickableMenu.exitFunction = OnSecondExit;
        }

        private static void OnSecondExit()
        {
            //Quest available starting Year 2
            int year = Game1.year;
            string difficulty = "easy";
            switch(year)
            {
                case > 4:
                    int? randomseed = (int?)(Game1.stats?.DaysPlayed);
                    if (randomseed is not null)
                    {   //Seed the random with a seed that changes every 28 days (like Foxbloom)
                        Random random = new Random((int)Game1.uniqueIDForThisGame + ((randomseed.Value - 1) / 28));
                        double chance = random.NextDouble();
                        if (chance < 0.67)
                            difficulty = "hard";
                        else if (chance < 0.90)
                            difficulty = "medium";
                    }
                    break;
                case 4:
                    difficulty = "hard";
                    break;
                case 3:
                    difficulty = "medium";
                    break;
            }
            LetterViewerMenu letter = new(Helper.Translation.Get($"FoxbloomHint.{difficulty}.{CustomCPTokens.FoxbloomDay}"));
            Game1.activeClickableMenu = letter;
        }

        private static bool MyLetter(GameLocation location, string[] arg2, Farmer farmer, Point point)
        {
            Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("RSV.MyLetter"));
            return true;
        }

        private static bool RSVOpenDaiaBook(GameLocation location, string[] arg2, Farmer farmer, Point point)
        {
            Game1.playSound("shadowpeep");
            OpenDaiaBook(location, arg2, farmer, point);
            return true;
        }

        private static void OpenDaiaBook(GameLocation location, string[] arg2, Farmer farmer, Point point)
        {
            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Daia.BookOpen"));
            if (!Game1.player.eventsSeen.Contains(RSVConstants.E_RAEUNSEAL))
            {
                var responses = new Response[]
                {
                    new Response("page1", Helper.Translation.Get("Daia.Page1")),
                    new Response("page2", Helper.Translation.Get("Daia.Page2")),
                    new Response("page3", Helper.Translation.Get("Daia.Page3")),
                    new Response("page4", Helper.Translation.Get("Daia.Page4")),
                    new Response("cancel", Helper.Translation.Get("Daia.BookClose")),
                };
                var responseActions = new List<Action>
                {
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint1"));
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint2"));
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint3"));
                    },
                    delegate
                    {
                        ImageMenu.Open(location, "ShowImage \"LooseSprites/RSVDaiaPage4\" 4f".Split(' '), Game1.player, point);
                    },
                    delegate{}
                };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("Daia.BookPages"), responses, responseActions);
            }
            else if (Game1.player.eventsSeen.Contains(RSVConstants.E_RAEUNSEAL) && !Game1.player.eventsSeen.Contains(RSVConstants.E_CLEANSED))
            {
                var responses = new Response[]
                {
                    new Response("page1", Helper.Translation.Get("Daia.Page1")),
                    new Response("page2", Helper.Translation.Get("Daia.Page2")),
                    new Response("page3", Helper.Translation.Get("Daia.Page3")),
                    new Response("page4", Helper.Translation.Get("Daia.Page4")),
                    new Response("page5", Helper.Translation.Get("Daia.Page5")),
                    new Response("page6", Helper.Translation.Get("Daia.Page6")),
                    new Response("page7", Helper.Translation.Get("Daia.Page7")),
                    new Response("cancel", Helper.Translation.Get("Daia.BookClose")),
                };
                var responseActions = new List<Action>
                {
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint1"));
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint2"));
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint3"));
                    },
                    delegate
                    {
                        ImageMenu.Open(location, "ShowImage \"LooseSprites/RSVDaiaPage4\" 4f".Split(' '), Game1.player, point);
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint4"));
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint5"));
                    },
                    delegate
                    {
                        ImageMenu.Open(location, "ShowImage \"LooseSprites/RSVDaiaPage7\" 4f".Split(' '),  Game1.player, point);
            },
                    delegate{}
                };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("Daia.BookPages"), responses, responseActions);
            }
            else if (Game1.player.eventsSeen.Contains(RSVConstants.E_CLEANSED))
            {
                var responses = new Response[]
                {
                    new Response("page1", Helper.Translation.Get("Daia.Page1")),
                    new Response("page2", Helper.Translation.Get("Daia.Page2")),
                    new Response("page3", Helper.Translation.Get("Daia.Page3")),
                    new Response("page4", Helper.Translation.Get("Daia.Page4")),
                    new Response("page5", Helper.Translation.Get("Daia.Page5")),
                    new Response("page6", Helper.Translation.Get("Daia.Page6")),
                    new Response("page7", Helper.Translation.Get("Daia.Page7")),
                    new Response("page8", Helper.Translation.Get("Daia.LegendFishes")),
                    new Response("cancel", Helper.Translation.Get("Daia.BookClose")),
                };
                var responseActions = new List<Action>
                {
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint1"));
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint2"));
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint3"));
                    },
                    delegate
                    {
                        ImageMenu.Open(location, "ShowImage \"LooseSprites/RSVDaiaPage4\" 4f".Split(' '),  Game1.player, point);
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint4"));
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint5"));
                    },
                    delegate
                    {
                        ImageMenu.Open(location, "ShowImage \"LooseSprites/RSVDaiaPage7\" 4f".Split(' '),  Game1.player, point);
                    },
                    delegate
                    {
                        ImageMenu.Open(location, "ShowImage \"LooseSprites/RSVDaiaPage8\" 4f".Split(' '), Game1.player, point);
                    },
                    delegate{}
                };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("Daia.BookPages"), responses, responseActions);
            }
        }

    }
}
