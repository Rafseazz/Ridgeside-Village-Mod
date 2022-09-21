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
