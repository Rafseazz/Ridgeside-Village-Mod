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
    internal static class SpecialOrders
    {
        const string FIXMINECART = "RSV.UntimedSpecialOrder.FixMinecart";
        const string DAIAQUEST = "RSV.UntimedSpecialOrder.DaiaQuest";
        const string RedCRYSTALQUEST = "RSV.UntimedSpecialOrder.ColorCrystalRed";
        const string OrangeCRYSTALQUEST = "RSV.UntimedSpecialOrder.ColorCrystalOrange";
        const string YellowCRYSTALQUEST = "RSV.UntimedSpecialOrder.ColorCrystalYellow";
        const string GreenCRYSTALQUEST = "RSV.UntimedSpecialOrder.ColorCrystalGreen";
        const string BlueCRYSTALQUEST = "RSV.UntimedSpecialOrder.ColorCrystalBlue";
        const string PurpleCRYSTALQUEST = "RSV.UntimedSpecialOrder.ColorCrystalPurple";
        const string GrayCRYSTALQUEST = "RSV.UntimedSpecialOrder.ColorCrystalGray";

        static IModHelper Helper;
        static IMonitor Monitor;
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            //Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.GameLoop.OneSecondUpdateTicked += OnUpdate;
        }

        private static void OnUpdate(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (Game1.player.eventsSeen.Contains(75160190) && !Game1.player.team.SpecialOrderActive(FIXMINECART) && !Game1.player.team.completedSpecialOrders.ContainsKey(FIXMINECART))
            {
                Game1.player.team.specialOrders.Add(SpecialOrder.GetSpecialOrder(FIXMINECART, null));
            }

            if (Game1.player.eventsSeen.Contains(75160254) && !Game1.player.team.SpecialOrderActive(DAIAQUEST) && !Game1.player.team.completedSpecialOrders.ContainsKey(DAIAQUEST))
            {
                Game1.player.team.specialOrders.Add(SpecialOrder.GetSpecialOrder(DAIAQUEST, null));
            }

            if (Game1.player.eventsSeen.Contains(75160257) && !Game1.player.team.SpecialOrderActive(RedCRYSTALQUEST) && !Game1.player.team.completedSpecialOrders.ContainsKey(RedCRYSTALQUEST))
            {
                Game1.player.team.specialOrders.Add(SpecialOrder.GetSpecialOrder(RedCRYSTALQUEST, null));
                Game1.player.mailReceived.Add("CrystalsFlagOngoing");
            }

            if (Game1.player.eventsSeen.Contains(75160257) && !Game1.player.team.SpecialOrderActive(OrangeCRYSTALQUEST) && !Game1.player.team.completedSpecialOrders.ContainsKey(OrangeCRYSTALQUEST))
            {
                Game1.player.team.specialOrders.Add(SpecialOrder.GetSpecialOrder(OrangeCRYSTALQUEST, null));
            }

            if (Game1.player.eventsSeen.Contains(75160257) && !Game1.player.team.SpecialOrderActive(YellowCRYSTALQUEST) && !Game1.player.team.completedSpecialOrders.ContainsKey(YellowCRYSTALQUEST))
            {
                Game1.player.team.specialOrders.Add(SpecialOrder.GetSpecialOrder(YellowCRYSTALQUEST, null));
            }

            if (Game1.player.eventsSeen.Contains(75160257) && !Game1.player.team.SpecialOrderActive(GreenCRYSTALQUEST) && !Game1.player.team.completedSpecialOrders.ContainsKey(GreenCRYSTALQUEST))
            {
                Game1.player.team.specialOrders.Add(SpecialOrder.GetSpecialOrder(GreenCRYSTALQUEST, null));
            }

            if (Game1.player.eventsSeen.Contains(75160257) && !Game1.player.team.SpecialOrderActive(BlueCRYSTALQUEST) && !Game1.player.team.completedSpecialOrders.ContainsKey(BlueCRYSTALQUEST))
            {
                Game1.player.team.specialOrders.Add(SpecialOrder.GetSpecialOrder(BlueCRYSTALQUEST, null));
            }

            if (Game1.player.eventsSeen.Contains(75160257) && !Game1.player.team.SpecialOrderActive(PurpleCRYSTALQUEST) && !Game1.player.team.completedSpecialOrders.ContainsKey(PurpleCRYSTALQUEST))
            {
                Game1.player.team.specialOrders.Add(SpecialOrder.GetSpecialOrder(PurpleCRYSTALQUEST, null));
            }

            if (Game1.player.eventsSeen.Contains(75160257) && !Game1.player.team.SpecialOrderActive(GrayCRYSTALQUEST) && !Game1.player.team.completedSpecialOrders.ContainsKey(GrayCRYSTALQUEST))
            {
                Game1.player.team.specialOrders.Add(SpecialOrder.GetSpecialOrder(GrayCRYSTALQUEST, null));
            }

            //Specific code to give message to player upon finishing crystals quest
            if (Game1.player.team.completedSpecialOrders.ContainsKey(RedCRYSTALQUEST) && Game1.player.team.completedSpecialOrders.ContainsKey(OrangeCRYSTALQUEST) && Game1.player.team.completedSpecialOrders.ContainsKey(YellowCRYSTALQUEST) && Game1.player.team.completedSpecialOrders.ContainsKey(GreenCRYSTALQUEST) && Game1.player.team.completedSpecialOrders.ContainsKey(BlueCRYSTALQUEST) && Game1.player.team.completedSpecialOrders.ContainsKey(PurpleCRYSTALQUEST) && Game1.player.team.completedSpecialOrders.ContainsKey(GrayCRYSTALQUEST) && Game1.player.mailReceived.Contains("CrystalsFlagOngoing"))
            {
                Game1.player.mailReceived.Remove("CrystalsFlagOngoing");
                Game1.playSound("healSound");
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("CompleteCrystalsNotif"));
            }
        }
    }
}
