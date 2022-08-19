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
        private static string[] names = { "Anton", "Paula" };

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
            Farmer who = Game1.player;
            if ((__instance.Name == "Shiro") && !who.eventsSeen.Contains(75160249))
            {
                __instance.CurrentDialogue.Clear();
                __instance.CurrentDialogue.Push(new Dialogue(Helper.Translation.Get("Shiro.RejectProposal"), __instance));
                Game1.drawDialogue(__instance);
                return false;
            }
            else if ((__instance.Name == "Kiarra") && who.eventsSeen.Contains(502261))
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
            if (names.Contains(name) && !SocialPage.isDatable(name))
            {
                int width = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
                string text = Game1.parseText(Helper.Translation.Get("RelationshipStatus.Locked"), Game1.smallFont, width);
                Vector2 textSize = Game1.smallFont.MeasureString(text);
                var sprites = Helper.Reflection.GetField<List<ClickableTextureComponent>>(__instance, "sprites").GetValue();
                float lineHeight = Game1.smallFont.MeasureString("W").Y;
                b.DrawString(Game1.smallFont, text, new Vector2((__instance.xPositionOnScreen + 192 + 8) - textSize.X / 2f, sprites[i].bounds.Bottom - (textSize.Y - lineHeight)), Game1.textColor);
            }
        }

        /*
        public static class TextReplacementPatch
        {
            public static bool IsComplicated(bool datable, string name)
            {
                //Log.Debug("RSV: Entered IsComplicated");
                //Log.Debug($"RSV: NPC {name} is datable: {datable}");
                if (datable)
                    return true; 
                return names.Contains(name);
            }

            public static string ReplaceText(string text, string name)
            {
                Log.Debug("RSV: Entered ReplaceText");
                Log.Debug($"RSV: NPC {name} is currently {text}");
                if (names.Contains(name) && !SocialPage.isDatable(name))
                {
                    text = "(complicated)";
                }
                Log.Debug($"RSV: NPC {name} is NOW {text}");
                return text;
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, ILGenerator ilgen)
            {
                List<CodeInstruction> l = insns.ToList();

                for (int i = 0; i < l.Count; i++)
                {
                    CodeInstruction current = l[i];
                    CodeInstruction next = l[i + 1];
                    if (current.Calls(AccessTools.Method(typeof(SocialPage), nameof(SocialPage.isDatable))))
                    {
                        //Log.Debug($"RSV: Found isDatable call");
                        l.InsertRange(i + 1, new CodeInstruction[] {
                            new CodeInstruction(OpCodes.Ldloc_0), // this should load the second argument string name
                            new CodeInstruction(OpCodes.Call, typeof(TextReplacementPatch).GetMethod("IsComplicated"))
                        });
                    }
                    else if (current.opcode == OpCodes.Stloc_S && next.opcode == OpCodes.Ldsfld && next.operand.ToString().Contains("borderWidth"))
                    {
                        Log.Debug($"RSV: Found end of if block");
                        l.InsertRange(i - 1, new CodeInstruction[] {
                            //new CodeInstruction(OpCodes.Stloc_S, l[i+1].operand), // this should load the first argument string text
                            new CodeInstruction(OpCodes.Ldloc_0), // this should load the second argument string name
                            new CodeInstruction(OpCodes.Call, typeof(TextReplacementPatch).GetMethod("ReplaceText"))
                        });
                        break;
                    };
                    Log.Debug($"RSV: {current}");
                }
                return l.AsEnumerable();
            }
        }
        */
                
    }
}
