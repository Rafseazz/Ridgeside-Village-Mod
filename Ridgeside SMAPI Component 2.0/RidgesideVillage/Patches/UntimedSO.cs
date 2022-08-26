using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Quests;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using SpaceCore.Events;

namespace RidgesideVillage
{
    internal static class UntimedSO
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }

        const int MEETBELINDA = 75160255;
        const int PREUNSEAL = 75160257;
        const int BLISSVISIT = 75160258;
        const int RAEUNSEAL = 75160259;
        const int OPENPORTAL = 75160256;
        const int BLISSGH1 = 75160266;
        const int SPIRITGH1 = 75160267;
        const int BLISSGH2 = 75160268;
        const int SPIRITGH2 = 75160269;

        private readonly static List<string> NPCNames = new List<string> { 
            "Acorn", "Aguar", "Alissa", "Anton", "Ariah", "Belinda", "Bert", "Blair", "Bliss", "Bryle", "Carmen",
            "Corine", "Daia", "Ezekiel", "Faye", "Flor", "Freddie", "Helen", "Ian", "Irene", "Jeric", "Jio", "Keahi",
            "Kenneth", "Kiarra", "Kimpoi", "Kiwi", "Lenny", "Lola", "Lorenzo", "Louie", "Maddie", "Maive", "Malaya",
            "Naomi", "Olga", "Paula", "Philip", "Pika", "Pipo", "Raeriyala", "Richard", "Sari", "Shanice", "Shiro",
            "Sonny", "Trinnie", "Undreya", "Ysabelle", "Yuuma", "Zayne"};
        private static Texture2D RSVemojis;

        public static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            Helper.Events.GameLoop.DayEnding += OnDayEnd;
            SpaceEvents.OnEventFinished += OnEventFinished;
            Log.Trace($"Applying Harmony Patch \"{nameof(UntimedSO)}\" prefixing SDV method.");
            
            harmony.Patch(
                original: AccessTools.Method(typeof(SpecialOrder), nameof(SpecialOrder.IsTimedQuest)),
                postfix: new HarmonyMethod(typeof(UntimedSO), nameof(SpecialOrders_IsTimed_postfix))
            );

            //only method called once on quest end. Is called for *all* players, not just host.
            harmony.Patch(
                original: AccessTools.Method(typeof(SpecialOrder), nameof(SpecialOrder.HostHandleQuestEnd)),
                postfix: new HarmonyMethod(typeof(UntimedSO), nameof(SpecialOrders_HostHandleQuestEnd_postfix))
            );

            //causes issues on MAC apparently??
            if (Constants.TargetPlatform == GamePlatform.Windows) 
            {
                 harmony.Patch(
                    original: AccessTools.Method(typeof(SpecialOrdersBoard), nameof(SpecialOrdersBoard.GetPortraitForRequester)),
                    postfix: new HarmonyMethod(typeof(UntimedSO), nameof(SpecialOrdersBoard_GetPortrait_postfix))
                );
            }
            else
            {
                Log.Trace($"Not patching GetProtraitForRequester because platform is {Constants.TargetPlatform}");
            }
            
           
        }

        private static void SpecialOrders_IsTimed_postfix(ref SpecialOrder __instance, ref bool __result)
        {
            if (__instance.questKey.Value.StartsWith("RSV.UntimedSpecialOrder"))
            {
                __result = false;
            }
        }

        private static void SpecialOrdersBoard_GetPortrait_postfix(SpecialOrdersBoard __instance, string requester_name, ref KeyValuePair<Texture2D, Rectangle>?  __result)
        {
            try
            {
                if (RSVemojis == null)
                {
                    RSVemojis = Helper.GameContent.Load<Texture2D>(PathUtilities.NormalizeAssetName("LooseSprites\\RSVemojis"));
                    if (RSVemojis== null)
                    {
                        Log.Error($"Loading error: Couldn't load {PathUtilities.NormalizeAssetName("LooseSprites\\RSVemojis")}");
                        return;
                    }
                }

                if (__result == null)
                {
                    int index = NPCNames.FindIndex(name => name.Equals(requester_name, StringComparison.OrdinalIgnoreCase));
                    if (index != -1)
                    {
                        __result = new KeyValuePair<Texture2D, Rectangle>(UntimedSO.RSVemojis, new Rectangle(index % 14 * 9, index / 14 * 9, 9, 9));
                        return;
                    }
                }
                return;
            }
            catch(Exception e)
            {
                Log.Error("Error in SpecialOrdersBoard_GetPortrait_postifx");
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                return;
            }
            
        }

        private static void SpecialOrders_HostHandleQuestEnd_postfix(ref SpecialOrder __instance)
        {
            try
            {
                if (__instance.questKey.Value == "RSV.UntimedSpecialOrder.DaiaQuest")
                {
                    Game1.player.addQuest(RSVConstants.PreparationsCompleteQuestID);
                }
            }
            catch (Exception e)
            {

                Log.Error("Error in SpecialOrders_HostHandleQuestEnd_postfix");
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
            }
           
        }

        public static void OnDayEnd(object sender, DayEndingEventArgs e)
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

        static void OnEventFinished(object sender, EventArgs e)
        {
            if (!Game1.player.IsMainPlayer)
                return;

            switch (Game1.CurrentEvent.id)
            {
                case MEETBELINDA:
                    TryRemoveQuest(RSVConstants.PreparationsCompleteQuestID); // Added in line 137, might also have been completed upon reading ninja note 
                    TryCompleteQuest(RSVConstants.NinjaNoteQuestID); // Added in QF hooks currently
                    TryAddQuest(RSVConstants.PreUnsealQuestID);
                    break;

                case PREUNSEAL:
                    TryCompleteQuest(RSVConstants.PreUnsealQuestID);
                    // Crystal quests are then added
                    break;

                case BLISSVISIT:
                    // Comes after crystal quests are complete
                    TryAddQuest(RSVConstants.RaeUnsealQuestID);
                    break;

                case RAEUNSEAL:
                    TryCompleteQuest(RSVConstants.RaeUnsealQuestID);
                    TryAddQuest(RSVConstants.OpenSpiritPortalQuestID);
                    break;

                case OPENPORTAL:
                    TryCompleteQuest(RSVConstants.OpenSpiritPortalQuestID);
                    if (Game1.player.IsMainPlayer)
                    {
                        Game1.player.team.specialOrders.Add(SpecialOrder.GetSpecialOrder("RSV.UntimedSpecialOrder.SpiritRealmFlames", null));
                    }
                    break;

                case BLISSGH1:
                    TryAddQuest(RSVConstants.PhantomGreenHouse1QuestID);
                    break;

                case SPIRITGH1:
                    TryCompleteQuest(RSVConstants.PhantomGreenHouse1QuestID);
                    break;

                case BLISSGH2:
                    TryAddQuest(RSVConstants.PhantomGreenHouse2QuestID);
                    break;

                case SPIRITGH2:
                    TryCompleteQuest(RSVConstants.PhantomGreenHouse2QuestID);
                    // Greenhouse quest then added
                    break;
            }
        }

        static void TryAddQuest(int id)
        {
            foreach(Farmer farmer in Game1.getAllFarmers())
            {
                farmer.addQuest(id);
            }
        }

        static void TryRemoveQuest(int id)
        {
            if (!Game1.player.hasQuest(id))
                return;

            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                if (farmer.hasQuest(id))
                    farmer.removeQuest(id);
            }

        }

        static void TryCompleteQuest(int id)
        {
            if (!Game1.player.hasQuest(id))
                return;

            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                if (farmer.hasQuest(id))
                    farmer.completeQuest(id);
            }
        }

    }
}
