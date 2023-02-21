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

namespace RidgesideVillage
{

    internal static class Dateables
    {
        private static IModHelper Helper { get; set; }
        private static Dictionary<string, string> unlockables = new Dictionary<string, string>(){
         // Character, 8 heart event ID/response ID for becoming datable
            { "Anton", "75160304/75163042" },
            { "Paula", "75160352/75163521" },
            { "Irene", "75160324/7516325" },
            { "Zayne", "75160440/7516439" },
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

            Helper.Events.GameLoop.DayEnding += OnDayEnding;
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
        }

        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            NPC irene = Game1.getCharacterFromName("Irene");
            if (irene is not null && Game1.player.friendshipData.TryGetValue("Irene", out var friendship) 
                && friendship.Status == FriendshipStatus.Married)
            { 
                if (Game1.currentSeason.Equals("spring") || Game1.currentSeason.Equals("summer"))
                {
                    if (Game1.dayOfMonth >= 15 && Game1.dayOfMonth <= 21)
                    {
                        Game1.warpCharacter(irene, RSVConstants.L_HIDDENWARP, Vector2.One);
                    }
                }
                else if (Game1.currentSeason.Equals("fall") || Game1.currentSeason.Equals("winter"))
                {
                    if (Game1.dayOfMonth >= 2 && Game1.dayOfMonth <= 7)
                    {
                        Game1.warpCharacter(irene, RSVConstants.L_HIDDENWARP, Vector2.One);
                    }
                }
            }
        }

        private static void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            //Has crucial events be marked as seen for every player when one player triggers it.
            //Made so that it doesn't require the host to trigger the crucial event to apply changes
            if (!Game1.IsMultiplayer)
            {
                return;
            }

            foreach (Farmer o in Game1.getAllFarmers())
            {
                //Paula's 8 heart event
                if (o.eventsSeen.Contains(75160389)) //seen Paula's 8 heart part 2
                {
                    int EventID = 75160352;
                    int EventID2 = 75160389;
                    int ResponseID = 75163521;
                    foreach (Farmer p in Game1.getAllFarmers())
                    {
                        if (!p.eventsSeen.Contains(EventID) && !p.eventsSeen.Contains(EventID2))
                        {
                            p.eventsSeen.Add(EventID);
                            p.eventsSeen.Add(EventID2);
                        }
                    }

                    if (o.dialogueQuestionsAnswered.Contains(ResponseID)) //Romance Paula route; Makes Paula dateable for everyone
                    {
                        foreach (Farmer p in Game1.getAllFarmers())
                        {
                            if (!p.dialogueQuestionsAnswered.Contains(ResponseID))
                            {
                                p.dialogueQuestionsAnswered.Add(ResponseID);
                            }
                        }
                    }
                }

                //Anton's 8 heart event
                if (o.eventsSeen.Contains(75160304))
                {
                    int EventID = 75160304;
                    int ResponseID = 75163042;
                    foreach (Farmer p in Game1.getAllFarmers())
                    {
                        if (!p.eventsSeen.Contains(EventID))
                        {
                            p.eventsSeen.Add(EventID);
                        }
                    }

                    if (o.dialogueQuestionsAnswered.Contains(ResponseID)) //Romance Paula route; Makes Anton dateable for everyone
                    {
                        foreach (Farmer p in Game1.getAllFarmers())
                        {
                            if (!p.dialogueQuestionsAnswered.Contains(ResponseID))
                            {
                                p.dialogueQuestionsAnswered.Add(ResponseID);
                            }
                        }
                    }
                }

                //Irene's 8 heart event
                if (o.eventsSeen.Contains(75160431)) //seen Irene's 8 heart part 2
                {
                    int EventID = 75160324;
                    int EventID2 = 75160431;
                    int ResponseID = 7516325;
                    foreach (Farmer p in Game1.getAllFarmers())
                    {
                        if (!p.eventsSeen.Contains(EventID) && !p.eventsSeen.Contains(EventID2))
                        {
                            p.eventsSeen.Add(EventID);
                            p.eventsSeen.Add(EventID2);
                        }
                    }

                    if (o.dialogueQuestionsAnswered.Contains(ResponseID)) //Romance Paula route; Makes Irene dateable for everyone
                    {
                        foreach (Farmer p in Game1.getAllFarmers())
                        {
                            if (!p.dialogueQuestionsAnswered.Contains(ResponseID))
                            {
                                p.dialogueQuestionsAnswered.Add(ResponseID);
                            }
                        }
                    }
                }

                //Zayne's 8 heart event
                if (o.eventsSeen.Contains(75160440))
                {
                    int EventID = 75160440;
                    int ResponseID = 7516439;
                    foreach (Farmer p in Game1.getAllFarmers())
                    {
                        if (!p.eventsSeen.Contains(EventID))
                        {
                            p.eventsSeen.Add(EventID);
                        }
                    }

                    if (o.dialogueQuestionsAnswered.Contains(ResponseID)) //Romance Paula route; Makes Zayne dateable for everyone
                    {
                        foreach (Farmer p in Game1.getAllFarmers())
                        {
                            if (!p.dialogueQuestionsAnswered.Contains(ResponseID))
                            {
                                p.dialogueQuestionsAnswered.Add(ResponseID);
                            }
                        }
                    }
                }
            }
        }

        private static void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            //If host has seen the respective event that prevent friendship decay, remove the decay by marking them as talked to
            if(Game1.MasterPlayer.eventsSeen.Contains(RSVConstants.E_IRENE_NODECAY) && Game1.player.friendshipData.ContainsKey("Irene"))
            {
                Game1.player.friendshipData["Irene"].TalkedToToday = true;
            }
            if (Game1.MasterPlayer.eventsSeen.Contains(RSVConstants.E_ZAYNE_NODECAY) && Game1.player.friendshipData.ContainsKey("Zayne"))
            {
                Game1.player.friendshipData["Zayne"].TalkedToToday = true;
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
