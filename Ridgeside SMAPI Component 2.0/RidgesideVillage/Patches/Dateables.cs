using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Events;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RidgesideVillage
{

    internal static class Dateables
    {
        private static IModHelper Helper { get; set; }
        private static IManifest ModManifest;

        private static string[] travelers = { "Bryle", "Irene", "June", "Zayne" };
        private static Dictionary<string, string> to_be_broadcast = new Dictionary<string, string>(){
         // Important event, extra info type/extra info ID
         // e: event only, r: response ID, m: mail flag
            { "75160304", "r/75163042" }, // Anton
            { "75160352", "r/75163521" }, // Paula
            { "75160324", "r/7516325" }, // Irene
            { "75160440", "r/7516439" }, //Zayne
            { "75160319", "m/FayeBryleLoveStory" }, //Faye + Bryle
            { "75160431", "e/" }, // Irene follow-up
            { "75160320", "e/" }, // Faye fashion show notice
            { "75160449", "e/" }, // Faye fashion show
        };
        public static Dictionary<string, string> unlock_rules = new Dictionary<string, string>(){
         // Character, deciding heart event ID/unlock cond type/cond ID
         // r: response, m: mail, !: does not have
            { "anton", "75160304/r/75163042" },
            { "paula", "75160352/r/75163521" },
            { "irene", "75160324/r/7516325" },
            { "zayne", "75160440/r/7516439" },
            { "faye", "75160449/!m/FayeBryleLoveStory" },
            { "bryle", "75160453/!m/FayeBryleLoveStory" },
        };

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            Log.Trace($"Applying Harmony Patch \"{nameof(Dateables)}.");
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), "engagementResponse"),
                prefix: new HarmonyMethod(typeof(Dateables), nameof(NPC_engagementResponse_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(SocialPage), "drawNPCSlot"),
                postfix: new HarmonyMethod(typeof(Dateables), nameof(SocialPage_drawNPCSlot_Postfix))
            );

            Helper.Events.GameLoop.DayStarted += WarpIrene_OnDayStarted;
            Helper.Events.GameLoop.DayEnding += TalkToTravelers_OnDayEnding;
            Helper.Events.GameLoop.SaveLoaded += SyncMulti_OnSaveLoaded;
            SpaceEvents.OnEventFinished += WarpBryle_OnEventFinished;
        }

        // sync_direction
        // Negative means this player is any player who has seen a relevant event and we want to send a broadcast message
        // Positive means this player is a farmhand and we want them to be in sync with the host if they've seen a relevant event
        private static void CheckUnlockables(int sync_direction)
        {
            if (sync_direction > 0 && Game1.IsMasterGame)
                return;
            //Log.Trace($"RSV: Syncing datable info. Direction = " + sync_direction);
            Farmer currentPlayer = Game1.player;
            foreach (string key in to_be_broadcast.Keys)
            {
                string eventID = key;
                string[] info = to_be_broadcast[key].Split('/');
                // For all entries, make sure all players have seen the event in the first bucket if anyone has
                if (sync_direction < 0 && Game1.CurrentEvent.id == eventID)
                {
                    UtilFunctions.sendBroadcastMsg(Helper, "event", "add", eventID.ToString());
                }
                else if (sync_direction > 0 && Game1.MasterPlayer.eventsSeen.Contains(eventID))
                {
                    if (!currentPlayer.eventsSeen.Contains(eventID))
                    {
                        currentPlayer.eventsSeen.Add(eventID);
                        Log.Trace($"RSV: {currentPlayer.Name} has now seen event {eventID}.");
                    }
                }

                // For each listing with a second item in the list, use that as condition and make sure it's universally met or unmet
                var id_type = info[0];
                string responseID;
                string mailID;
                bool decision_made;
                switch (id_type)
                {
                    case "e":
                        break;
                    // Unlocked by dialogue ID
                    case "r":
                        if (sync_direction < 0 && Game1.CurrentEvent.id == eventID)
                        {
                            responseID = info[1];
                            decision_made = sync_direction < 0 ? Game1.player.dialogueQuestionsAnswered.Contains(responseID) : Game1.MasterPlayer.dialogueQuestionsAnswered.Contains(responseID);
                            if (decision_made)
                            {
                                UtilFunctions.sendBroadcastMsg(Helper, "response", "add", responseID.ToString());
                            }
                        }
                        else if (sync_direction > 0 && Game1.MasterPlayer.eventsSeen.Contains(eventID))
                        {
                            responseID = info[1];
                            decision_made = sync_direction < 0 ? Game1.player.dialogueQuestionsAnswered.Contains(responseID) : Game1.MasterPlayer.dialogueQuestionsAnswered.Contains(responseID);
                            if (!currentPlayer.dialogueQuestionsAnswered.Contains(responseID) && decision_made)
                            {
                                currentPlayer.dialogueQuestionsAnswered.Add(responseID);
                                Log.Trace($"RSV: {currentPlayer.Name} now has response ID {responseID}.");
                            }  
                        }
                        break;
                    // Unlocked by mail flag
                    case "m":
                        if (sync_direction < 0 && Game1.CurrentEvent.id == eventID)
                        {
                            mailID = info[1];
                            decision_made = sync_direction < 0 ? Game1.player.mailReceived.Contains(mailID) : Game1.MasterPlayer.mailReceived.Contains(mailID);
                            if (decision_made)
                            {
                                UtilFunctions.sendBroadcastMsg(Helper, "mail", "add", mailID);
                            }
                        }
                        else if (sync_direction > 0 && Game1.MasterPlayer.eventsSeen.Contains(eventID))
                        {
                            mailID = info[1];
                            decision_made = sync_direction < 0 ? Game1.player.mailReceived.Contains(mailID) : Game1.MasterPlayer.mailReceived.Contains(mailID);
                            if (!currentPlayer.mailReceived.Contains(mailID) && decision_made)
                            {
                                currentPlayer.mailReceived.Add(mailID);
                                Log.Trace($"RSV: {currentPlayer.Name} now has mail flag {mailID}.");
                            }   
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private static void OnMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != ModManifest.UniqueID)
                return;
            string message = e.ReadAs<string>();
            //Log.Trace($"RSV: {Game1.player.Name} received message {e.Type} {message} received.");
            switch (e.Type)
            {
                case "EventSeen":
                    Game1.player.eventsSeen.Add(message);
                    Log.Trace($"RSV: Marked event {message} as seen.");
                    break;
                case "QuestionAnswered":
                    Game1.player.dialogueQuestionsAnswered.Add(message);
                    Log.Trace($"RSV: Marked response {message} as chosen.");
                    break;
                case "MailReceived":
                    Game1.player.mailReceived.Add(message);
                    Log.Trace($"RSV: Marked mail {message} as received.");
                    break;
                default:
                    break;
            }
        }

        private static void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Game1.IsMultiplayer)
            {
                Log.Trace($"RSV: Checking unlockables at load time.");
                CheckUnlockables(1);
            }
        }

        private static void WarpBryle_OnEventFinished(object sender, EventArgs e)
        {
            if (Game1.IsMultiplayer)
            {
                Log.Trace($"RSV: Checking unlockables at event end.");
                CheckUnlockables(-1);
            }

            //Teleport Bryle
            if (Game1.CurrentEvent.id.Equals("75160460"))
            {
                NPC bryle = Game1.getCharacterFromName("Bryle");
                if (bryle is not null && Game1.player.friendshipData.TryGetValue("Bryle", out var f1)
                    && f1.Status == FriendshipStatus.Married)
                {
                    Game1.warpCharacter(bryle, RSVConstants.L_HIDDENWARP, Vector2.One);
                }
            }
        }

        private static void TalkToTravelers_OnDayEnding(object sender, DayEndingEventArgs e)
        {
            foreach(string name in travelers)
                if (Game1.player.friendshipData.ContainsKey(name) && Game1.getCharacterFromName(name).currentLocation.Name.Contains("HiddenWarp"))
                    Game1.player.friendshipData[name].TalkedToToday = true;
        }

        private static void WarpIrene_OnDayStarted(object sender, DayStartedEventArgs e)
        {
            NPC irene = Game1.getCharacterFromName("Irene");
            if (irene is not null && Game1.player.friendshipData.TryGetValue("Irene", out var friendship)
                && friendship.Status == FriendshipStatus.Married)
            {
                if (Game1.currentSeason.Equals("spring") && Game1.dayOfMonth >= 15 && Game1.dayOfMonth <= 21)
                {
                    Game1.warpCharacter(irene, RSVConstants.L_HIDDENWARP, Vector2.One);
                }
                else if (Game1.currentSeason.Equals("fall") && Game1.dayOfMonth >= 2 && Game1.dayOfMonth <= 7)
                {
                    Game1.warpCharacter(irene, RSVConstants.L_HIDDENWARP, Vector2.One);
                }
            }
        }

        private static bool NPC_engagementResponse_Prefix(NPC __instance)
        {
            if ((__instance.Name == "Shiro") && !Game1.MasterPlayer.eventsSeen.Contains("75160249"))
            {
                __instance.CurrentDialogue.Clear();
                __instance.CurrentDialogue.Push(new Dialogue(__instance, "",Helper.Translation.Get("Shiro.RejectProposal")));
                Game1.drawDialogue(__instance);
                return false;
            }
            else if ((__instance.Name == "Kiarra") && Game1.MasterPlayer.eventsSeen.Contains("502261"))
            {
                __instance.CurrentDialogue.Clear();
                __instance.CurrentDialogue.Push(new Dialogue(__instance, "",Helper.Translation.Get("Kiarra.RejectProposal")));
                Game1.drawDialogue(__instance);
                return false;
            }
            return true;
        }

        private static void SocialPage_drawNPCSlot_Postfix(SocialPage __instance, SpriteBatch b, int i)
        {
            var socialEntry = __instance.SocialEntries[i];
            if (unlock_rules.Keys.Contains(socialEntry.InternalName.ToLower()) && !socialEntry.IsDatable)
            {
                if (Game1.player.eventsSeen.Contains(unlock_rules[socialEntry.InternalName.ToLower()].Split("/")[0]))
                    return;
                int width = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
                string text = Game1.parseText(Helper.Translation.Get("RelationshipStatus.Locked"), Game1.smallFont, width);
                Vector2 textSize = Game1.smallFont.MeasureString(text);
                var sprites = Helper.Reflection.GetField<List<ClickableTextureComponent>>(__instance, "sprites").GetValue();
                float lineHeight = Game1.smallFont.MeasureString("W").Y;
                b.DrawString(Game1.smallFont, text, new Vector2((__instance.xPositionOnScreen + 192 + 8) - textSize.X / 2f, sprites[i].bounds.Bottom - (textSize.Y - lineHeight)), Game1.textColor);
            }
        }
    }
}
