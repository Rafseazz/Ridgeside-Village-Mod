using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;

namespace RidgesideVillage
{
    internal static class HarmonyPatch_UntimedSO
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }

        public static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            Helper.Events.GameLoop.DayEnding += OnDayEnd;
        }

        private static void OnDayEnd(object sender, DayEndingEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            foreach(SpecialOrder o in Game1.player.team.specialOrders)
            {
                if (o.questKey.Value.StartsWith("RSV.UntimedSpecialOrder"))
                {
                    o.dueDate.Value = Game1.Date.TotalDays + 100;
                }
            }
        }

    }
}
