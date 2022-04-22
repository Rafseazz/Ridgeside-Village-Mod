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

        const int HasUnsealedRae = 75160259;
        const int RealmCleansed = 75160265;
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            TileActionHandler.RegisterTileAction("RSVOpenDaiaBook", RSVOpenDaiaBook);
            TileActionHandler.RegisterTileAction("MyLetter", MyLetter);
            TileActionHandler.RegisterTileAction("RSVFoxbloomHint", GetFoxbloomHint);
        }

        private static void GetFoxbloomHint(string tileActionString, Vector2 position)
        {
            if (!Game1.player.eventsSeen.Contains(RealmCleansed))
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("FoxbloomHint.Uncleansed"));
                return;
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
        }

        private static void OnFirstExit()
        {
            LetterViewerMenu letter = new(Helper.Translation.Get("FoxbloomHint.Text"));
            letter.whichBG = 1;
            Game1.activeClickableMenu = letter;
            Game1.activeClickableMenu.exitFunction = OnSecondExit;
        }

        private static void OnSecondExit()
        {
            LetterViewerMenu letter2 = new($"Solve for x:^^   x = {CustomCPTokens.FoxbloomDay}");
            letter2.whichBG = 1;
            Game1.activeClickableMenu = letter2;
        }

        private static void MyLetter(string tileActionString, Vector2 position)
        {
            Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("RSV.MyLetter"));
        }

        private static void RSVOpenDaiaBook(string tileActionString, Vector2 position)
        {
            Game1.playSound("shadowpeep");
            OpenDaiaBook();
        }

        private static void OpenDaiaBook()
        {
            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Daia.BookOpen"));
            if (!Game1.player.eventsSeen.Contains(HasUnsealedRae))
            {
                var responses = new List<Response>
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
                        ImageMenu.Open("ShowImage \"LooseSprites/RSVDaiaPage4\" 4f", Vector2.Zero);
                    },
                    delegate{}
                };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("Daia.BookPages"), responses, responseActions);
            }
            else if (Game1.player.eventsSeen.Contains(HasUnsealedRae) && !Game1.player.eventsSeen.Contains(75160265))
            {
                var responses = new List<Response>
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
                        ImageMenu.Open("ShowImage \"LooseSprites/RSVDaiaPage4\" 4f", Vector2.Zero);
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
                        ImageMenu.Open("ShowImage \"LooseSprites/RSVDaiaPage7\" 4f", Vector2.Zero);
                    },
                    delegate{}
                };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("Daia.BookPages"), responses, responseActions);
            }
            else if (Game1.player.eventsSeen.Contains(75160265))
            {
                var responses = new List<Response>
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
                        ImageMenu.Open("ShowImage \"LooseSprites/RSVDaiaPage4\" 4f", Vector2.Zero);
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
                        ImageMenu.Open("ShowImage \"LooseSprites/RSVDaiaPage7\" 4f", Vector2.Zero);
                    },
                    delegate
                    {
                        ImageMenu.Open("ShowImage \"LooseSprites/RSVDaiaPage8\" 4f", Vector2.Zero);
                    },
                    delegate{}
                };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("Daia.BookPages"), responses, responseActions);
            }
        }

    }
}