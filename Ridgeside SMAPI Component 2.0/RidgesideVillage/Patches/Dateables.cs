using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using SpaceCore.Events;

namespace RidgesideVillage
{

    internal static class Dateables
    {
        private static IModHelper Helper { get; set; }
        private static IManifest ModManifest;

        private static string[] travelers = { "Bryle", "Irene", "June", "Zayne" };
        private static Dictionary<string, string> unlockables = new Dictionary<string, string>(){
         // Character, deciding heart event ID/response ID or mail flag
            { "Anton", "75160304/75163042" },
            { "Paula", "75160352/75163521" },
            { "Irene", "75160324/7516325" },
            { "Zayne", "75160440/7516439" },
            { "Faye", "75160319/FayeBryleLoveStory" },
        // Character, follow-up event ID (if different from event ID above)
            //{ "PaulaPt2", "75160389" },
            { "IrenePt2", "75160431" },
            { "Bryle", "75160453" }, // Not a follow-up per se, but datability decided in Faye's event
            { "Faye8", "75160320" }, // Lead-up to the actual fashion show but should only be shown once anyway
            { "Faye8Pt2", "75160449" },
        };

        internal static void ApplyPatch(Harmony harmony, IModHelper helper, IManifest manifest)
        {
            Helper = helper;
            ModManifest = manifest;

            Log.Trace($"Applying Harmony Patch \"{nameof(Dateables)}.");
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), "engagementResponse"),
                prefix: new HarmonyMethod(typeof(Dateables), nameof(NPC_engagementResponse_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(SocialPage), "drawNPCSlot"),
                postfix: new HarmonyMethod(typeof(Dateables), nameof(SocialPage_drawNPCSlot_Postfix))
            );

            Helper.Events.Multiplayer.ModMessageReceived += OnMessageReceived;
            Helper.Events.GameLoop.DayEnding += OnDayEnding;
            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            SpaceEvents.OnEventFinished += OnEventFinished;
        }

        // sync_direction
        // Negative means this player is any player who has seen a relevant event and we want the others to be in sync
        // Positive means this player is a farmhand and we want them to be in sync with the host if they've seen a relevant event
        private static void CheckUnlockables(int sync_direction)
        {
            if (sync_direction > 0 && Game1.IsMasterGame)
                return;
            //Log.Trace($"RSV: Syncing datable info. Direction = " + sync_direction);
            Farmer currentPlayer = Game1.player;
            foreach (string name in unlockables.Keys)
            {
                string[] info = unlockables[name].Split('/');
                //Log.Trace($"RSV: Checking {name} event {info[0]}.");
                int eventID = int.Parse(info[0]);
                //Log.Trace($"RSV: Checking event ID {eventID}...");
                // For all entries, make sure all players have seen the event in the first bucket if anyone has
                if (sync_direction < 0 && Game1.CurrentEvent.id == eventID)
                {
                    Helper.Multiplayer.SendMessage(
                                message: $"{eventID}",
                                messageType: "EventSeen",
                                modIDs: new[] { ModManifest.UniqueID },
                                playerIDs: null);
                    Log.Trace($"RSV: EventSeen message for {eventID} sent to all players.");
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
                int responseID;
                string mailID;
                bool decision_made;
                switch (name)
                {
                    // Unlocked by dialogue ID
                    case "Anton":
                    case "Paula":
                    case "Irene":
                    case "Zayne":
                        if (sync_direction < 0 && Game1.CurrentEvent.id == eventID)
                        {
                            responseID = int.Parse(info[1]);
                            decision_made = sync_direction < 0 ? Game1.player.dialogueQuestionsAnswered.Contains(responseID) : Game1.MasterPlayer.dialogueQuestionsAnswered.Contains(responseID);
                            if (decision_made)
                            {
                                Helper.Multiplayer.SendMessage(
                                message: $"{responseID}",
                                messageType: "QuestionAnswered",
                                modIDs: new[] { ModManifest.UniqueID },
                                playerIDs: null);
                                Log.Trace($"RSV: QuestionAnswered message for {responseID} sent to all players.");
                            }
                        }
                        else if (sync_direction > 0 && Game1.MasterPlayer.eventsSeen.Contains(eventID))
                        {
                            responseID = int.Parse(info[1]);
                            decision_made = sync_direction < 0 ? Game1.player.dialogueQuestionsAnswered.Contains(responseID) : Game1.MasterPlayer.dialogueQuestionsAnswered.Contains(responseID);
                            if (!currentPlayer.dialogueQuestionsAnswered.Contains(responseID) && decision_made)
                            {
                                currentPlayer.dialogueQuestionsAnswered.Add(responseID);
                                Log.Trace($"RSV: {currentPlayer.Name} now has response ID {responseID}.");
                            }  
                        }
                        break;
                    // Unlocked by mail flag
                    case "Faye":
                        if (sync_direction < 0 && Game1.CurrentEvent.id == eventID)
                        {
                            mailID = info[1];
                            decision_made = sync_direction < 0 ? Game1.player.mailReceived.Contains(mailID) : Game1.MasterPlayer.mailReceived.Contains(mailID);
                            if (decision_made)
                            {
                                Helper.Multiplayer.SendMessage(
                                message: $"{mailID}",
                                messageType: "MailReceived",
                                modIDs: new[] { ModManifest.UniqueID },
                                playerIDs: null);
                                Log.Trace($"RSV: MailReceived message for {mailID} sent to all players.");
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
                        return;
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
                    Game1.player.eventsSeen.Add(int.Parse(message));
                    Log.Trace($"RSV: Marked event {message} as seen.");
                    break;
                case "QuestionAnswered":
                    Game1.player.dialogueQuestionsAnswered.Add(int.Parse(message));
                    Log.Trace($"RSV: Marked response {message} as chosen.");
                    break;
                case "MailReceived":
                    Game1.player.mailReceived.Add(message);
                    Log.Trace($"RSV: Marked mail {message} as received.");
                    break;
                default:
                    return;
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

        private static void OnEventFinished(object sender, EventArgs e)
        {
            if (Game1.IsMultiplayer)
            {
                Log.Trace($"RSV: Checking unlockables at event end.");
                CheckUnlockables(-1);
            }

            //Teleport Bryle
            if (!(Game1.CurrentEvent.id == 75160460))
                return;
            NPC bryle = Game1.getCharacterFromName("Bryle");
            if (bryle is not null && Game1.player.friendshipData.TryGetValue("Bryle", out var friendship)
                && friendship.Status == FriendshipStatus.Married)
            {
                if (Game1.CurrentEvent.id == 75160460)
                {
                    Game1.warpCharacter(bryle, RSVConstants.L_HIDDENWARP, Vector2.One);
                }
            }
        }

        private static void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            foreach(string name in travelers)
                if (Game1.player.friendshipData.ContainsKey(name) && Game1.getCharacterFromName(name).currentLocation.Name.Contains("HiddenWarp"))
                    Game1.player.friendshipData[name].TalkedToToday = true;
        }

        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            NPC irene = Game1.getCharacterFromName("Irene");
            if (irene is not null && Game1.player.friendshipData.TryGetValue("Irene", out var friendship)
                && friendship.Status == FriendshipStatus.Married)
            {
                if (Game1.currentSeason.Equals("spring"))
                {
                    if (Game1.dayOfMonth >= 15 && Game1.dayOfMonth <= 21)
                    {
                        Game1.warpCharacter(irene, RSVConstants.L_HIDDENWARP, Vector2.One);
                    }
                }
                else if (Game1.currentSeason.Equals("fall"))
                {
                    if (Game1.dayOfMonth >= 2 && Game1.dayOfMonth <= 7)
                    {
                        Game1.warpCharacter(irene, RSVConstants.L_HIDDENWARP, Vector2.One);
                    }
                }
            }
        }

        private static bool NPC_engagementResponse_Prefix(NPC __instance)
        {
            if ((__instance.Name == "Shiro") && !Game1.MasterPlayer.eventsSeen.Contains(75160249))
            {
                __instance.CurrentDialogue.Clear();
                __instance.CurrentDialogue.Push(new Dialogue(Helper.Translation.Get("Shiro.RejectProposal"), __instance));
                Game1.drawDialogue(__instance);
                return false;
            }
            else if ((__instance.Name == "Kiarra") && Game1.MasterPlayer.eventsSeen.Contains(502261))
            {
                __instance.CurrentDialogue.Clear();
                __instance.CurrentDialogue.Push(new Dialogue(Helper.Translation.Get("Kiarra.RejectProposal"), __instance));
                Game1.drawDialogue(__instance);
                return false;
            }
            return true;
        }

        private static void SocialPage_drawNPCSlot_Postfix(SocialPage __instance, SpriteBatch b, int i)
        {
            string name = __instance.names[i] as string;
            if (unlockables.Keys.Contains(name) && !SocialPage.isDatable(name))
            {
                int event_id = int.Parse(unlockables[name].Split("/")[0]);
                int romance_id = int.Parse(unlockables[name].Split("/")[1]);
                if (Game1.player.eventsSeen.Contains(event_id) && !Game1.player.dialogueQuestionsAnswered.Contains(romance_id))
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
