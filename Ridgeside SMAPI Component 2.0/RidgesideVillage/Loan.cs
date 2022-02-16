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

namespace RidgesideVillage
{
    internal static class Loan
    {
        static IModHelper Helper;
        static IMonitor Monitor;
        const string STARTEDLOAN = "RSV.TakenLoan";
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;
            TileActionHandler.RegisterTileAction("RSVMaiveLoan", RSVMaiveLoan);
        }

        private static void RSVMaiveLoan(string tileActionString, Vector2 position)
        {
            if (!Game1.player.IsMainPlayer)
            {
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("RSV.MaiveError"), HUDMessage.error_type));
            }
            else
            {
                if (!Game1.player.mailReceived.Contains(STARTEDLOAN))
                {
                        var responses = new List<Response>
                    {
                        new Response("yes", Helper.Translation.Get("Offer.Yes")),
                        new Response("no", Helper.Translation.Get("Offer.No")),
                    };
                        var responseActions = new List<Action>
                    {
                        delegate
                        {
                            Game1.player.Money += 500000;
                            Game1.player.mailReceived.Add(STARTEDLOAN);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("RSV.LoanBegun"));
                        },
                        delegate{}
                    };
                        Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("RSV.BeginLoan"), responses, responseActions);
                }
                else if (Game1.player.mailReceived.Contains(STARTEDLOAN))
                {
                    var responses = new List<Response>
                    {
                        new Response("yes", Helper.Translation.Get("Offer.Yes")),
                        new Response("no", Helper.Translation.Get("Offer.No")),
                    };
                    var responseActions = new List<Action>
                    {
                        delegate
                        {
                            Game1.player.Money -= 500000;
                            Game1.player.mailReceived.Remove(STARTEDLOAN);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("RSV.LoanPaid"));
                        },
                        delegate{}
                    };
                    Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("RSV.PayLoan"), responses, responseActions);
                }
            }
        }


    }
}

