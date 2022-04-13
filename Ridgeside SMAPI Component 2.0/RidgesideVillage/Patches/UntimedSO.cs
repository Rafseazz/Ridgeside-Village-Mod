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
                try
                {
                    Type QFSpecialBoardClass = Type.GetType("QuestFramework.Framework.Menus.CustomOrderBoard, QuestFramework");
                    harmony.Patch(
                        original: AccessTools.Method(QFSpecialBoardClass, "GetPortraitForRequester"),
                        postfix: new HarmonyMethod(typeof(UntimedSO), nameof(SpecialOrdersBoard_GetPortrait_postfix))
                    );
                }
                catch
                {
                    Log.Info("Couldnt patch Quest Framework. Emojis in the SO board might not show up");
                }
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
                    RSVemojis = Helper.Content.Load<Texture2D>(PathUtilities.NormalizeAssetName("LooseSprites\\RSVemojis"), ContentSource.GameContent);
                    if(RSVemojis== null)
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
                    int questID = ExternalAPIs.QF.ResolveQuestId("preparations_complete@Rafseazz.RSVQF");
                    Game1.player.addQuest((questID));
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
            switch (Game1.CurrentEvent.id)
            {
                case MEETBELINDA:
                    try { Game1.player.removeQuest(ExternalAPIs.QF.ResolveQuestId("preparations_complete@Rafseazz.RSVQF")); } // Added in line 137
                    catch { } // Might also have been completed upon reading ninja note 
                    try { Game1.player.completeQuest(ExternalAPIs.QF.ResolveQuestId("follow_ninja_note@Rafseazz.RSVQF")); } // Added in QF hooks currently
                    catch { }
                    try { Game1.player.addQuest(ExternalAPIs.QF.ResolveQuestId("rae_pre_unseal@Rafseazz.RSVQF")); }
                    catch { }
                    break;

                case PREUNSEAL:
                    try { Game1.player.completeQuest(ExternalAPIs.QF.ResolveQuestId("rae_pre_unseal@Rafseazz.RSVQF")); }
                    catch { }
                    // Crystal quests are then added
                    break;

                case BLISSVISIT:
                    // Comes after crystal quests are complete
                    try { Game1.player.addQuest(ExternalAPIs.QF.ResolveQuestId("rae_unseal@Rafseazz.RSVQF")); }
                    catch { }
                    break;

                case RAEUNSEAL:
                    try { Game1.player.completeQuest(ExternalAPIs.QF.ResolveQuestId("rae_unseal@Rafseazz.RSVQF")); }
                    catch { }
                    try { Game1.player.addQuest(ExternalAPIs.QF.ResolveQuestId("open_spirit_portal@Rafseazz.RSVQF")); }
                    catch { }
                    break;

                case OPENPORTAL:
                    try { Game1.player.completeQuest(ExternalAPIs.QF.ResolveQuestId("open_spirit_portal@Rafseazz.RSVQF")); }
                    catch { }
                    // No try catch bc I want to see the error if this fails
                    Game1.player.team.specialOrders.Add(SpecialOrder.GetSpecialOrder("RSV.UntimedSpecialOrder.SpiritRealmFlames", null));
                    break;

                case BLISSGH1:
                    try { Game1.player.addQuest(ExternalAPIs.QF.ResolveQuestId("phantom_greenhouse_1@Rafseazz.RSVQF")); }
                    catch { }
                    break;

                case SPIRITGH1:
                    try { Game1.player.completeQuest(ExternalAPIs.QF.ResolveQuestId("phantom_greenhouse_1@Rafseazz.RSVQF")); }
                    catch { }
                    break;

                case BLISSGH2:
                    try { Game1.player.addQuest(ExternalAPIs.QF.ResolveQuestId("phantom_greenhouse_2@Rafseazz.RSVQF")); }
                    catch { }
                    break;

                case SPIRITGH2:
                    try { Game1.player.completeQuest(ExternalAPIs.QF.ResolveQuestId("phantom_greenhouse_2@Rafseazz.RSVQF")); }
                    catch { }
                    // Greenhouse quest then added
                    break;
            }
        }

    }
}
