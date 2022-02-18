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
        public static int deducted = 0;
        static IModHelper Helper;
        static IMonitor Monitor;
        const string REWARDLETTER = "MaiveSOLetter";
        const string LOANMAIL = "RSV.TakenLoan";
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.GameLoop.DayEnding += OnDayEnding;

            TileActionHandler.RegisterTileAction("RSVMaiveLoan", RSVMaiveLoan);
        }

        private static void RSVMaiveLoan(string tileActionString, Vector2 position)
        {
            if (!Game1.player.IsMainPlayer && Game1.player.mailReceived.Contains(REWARDLETTER))
            {
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("RSV.MaiveError"), HUDMessage.error_type));
            }
            else
            {
                if (!Game1.player.mailReceived.Contains(LOANMAIL) && Game1.player.mailReceived.Contains(REWARDLETTER))
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
                            Game1.player.mailReceived.Add(LOANMAIL);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("RSV.LoanBegun"));
                        },
                        delegate{}
                    };
                        Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("RSV.BeginLoan"), responses, responseActions);
                }
                else if (Game1.player.mailReceived.Contains(LOANMAIL))
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
                            Game1.player.mailReceived.Remove(LOANMAIL);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("RSV.LoanPaid"));
                        },
                        delegate{}
                    };
                    Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("RSV.PayLoan"), responses, responseActions);
                }
            }
        }

        public static void ApplyInterest()
        {
            Log.Trace($"MaiveLoan - begin interest calculations");
            int[] shippingCategoryTotals = new int[5];
            var ShippingBin = Game1.getFarm().getShippingBin(Game1.player);
            Log.Trace($"MaiveLoan - Got shipping bin");
            foreach (Item item in ShippingBin)
            {
                Log.Trace($"MaiveLoan - {item.Name} in shipping bin");
                if (item is StardewValley.Object obj)
                {
                    Log.Trace($"MaiveLoan - {obj.Name} is SDVObject");
                    int shippingCategory = GetShippingCategory(obj);
                    int sellPrice = obj.sellToStorePrice(-1L) * ((Item)obj).Stack;
                    shippingCategoryTotals[shippingCategory] += sellPrice;
                    Log.Trace($"MaiveLoan - {obj.Name} full price: {sellPrice}");
                }
            }
            int totalDeducted = 0;
            for (int i = 0; i < shippingCategoryTotals.Length; i++)
            {
                int amount = shippingCategoryTotals[i] * 10 / 100;
                Log.Trace($"MaiveLoan - deducting {amount} G for this category");
                if (amount > 0)
                {
                    Game1.player.Money -= amount;
                    totalDeducted += amount;
                }
            }
            if (totalDeducted > 0)
            {
                Log.Trace($"MaiveLoan - deducted {totalDeducted} G in total");
                Log.Trace($"MaiveLoan - player money is {Game1.player.Money}");
            }
            deducted = totalDeducted;
        }

        public static int GetShippingCategory(StardewValley.Object obj)
        {
            return GetShippingCategory(((Item)obj).ParentSheetIndex, ((Item)obj).Category);
        }

        public static int GetShippingCategory(int objectID, int objectCategory)
        {
            switch (objectID)
            {
                case 296:
                case 396:
                case 402:
                case 406:
                case 410:
                case 414:
                case 418:
                    return 1;
                default:
                    switch (objectCategory)
                    {
                        case -20:
                        case -4:
                            return 2;
                        case -15:
                        case -12:
                        case -2:
                            return 3;
                        case -80:
                        case -79:
                        case -75:
                        case -26:
                        case -14:
                        case -6:
                        case -5:
                            return 0;
                        case -81:
                        case -27:
                        case -23:
                            return 1;
                        default:
                            return 4;
                    }
            }
        }

        public static void SendReminder()
        {
            string content = Helper.Translation.Get("RSV.LoanInterest") + deducted.ToString() + " G";
            Game1.chatBox.addInfoMessage(content);
        }

        private static void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            if ((Game1.player.mailReceived.Contains(LOANMAIL)) & (Game1.player.IsMainPlayer))
            {
                ApplyInterest();
            }
        }
        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {

            if ((Game1.player.mailReceived.Contains(LOANMAIL)) & (Game1.player.IsMainPlayer))
            {
                SendReminder();
            }
        }


    }
}

