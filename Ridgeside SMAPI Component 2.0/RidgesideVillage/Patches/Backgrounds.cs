using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewModdingAPI;

namespace RidgesideVillage
{
    internal static class Backgrounds
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }

        private static AlphaTestEffect ate;
        const string TORTSREALM = "Custom_Ridgeside_TortsRealm";

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;
            Log.Trace($"Applying Harmony Patch \"{nameof(Backgrounds)}\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(Background), nameof(Background.draw)),
                postfix: new HarmonyMethod(typeof(Backgrounds), nameof(Background_draw_postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.Begin)),
                prefix: new HarmonyMethod(typeof(Backgrounds), nameof(SpriteBatch_Begin_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.End)),
                postfix: new HarmonyMethod(typeof(Backgrounds), nameof(SpriteBatch_End_postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.ShouldDrawOnBuffer)),
                postfix: new HarmonyMethod(typeof(Backgrounds), nameof(Game1_ShouldDrawOnBuffer_postfix))
            );
            harmony.Patch(
                original: AccessTools.Method("StardewModdingAPI.Framework.SGame:DrawImpl"),
                transpiler: new HarmonyMethod(typeof(Game1CatchLightingRenderPatch).GetMethod("Transpiler"))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), "resetLocalState"),
                postfix: new HarmonyMethod(typeof(Backgrounds), nameof(GameLocation_resetLocalState_postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.cleanupBeforePlayerExit)),
                postfix: new HarmonyMethod(typeof(Backgrounds), nameof(GameLocation_cleanUpBeforePlayerExit_postfix))
            );
        }

        public static void Background_draw_postfix(Background __instance, SpriteBatch b)
        {
            if (__instance is TortsBackground bg)
                bg.Draw(b);
            
        }

        public static void SpriteBatch_Begin_prefix(ref DepthStencilState depthStencilState, ref BlendState blendState, ref Effect effect)
        {
            if (BgUtils.DefaultStencilOverride != null && depthStencilState == null)
            {
                if (ate == null || true)
                {
                    ate = new AlphaTestEffect(Game1.graphics.GraphicsDevice)
                    {
                        Projection = Matrix.CreateOrthographicOffCenter(0, Game1.viewport.Width, Game1.viewport.Height, 0, 0, 1),
                        VertexColorEnabled = true
                    };
                }
                if (effect == null)
                    effect = ate;

                depthStencilState = BgUtils.DefaultStencilOverride;
            }

            if (Game1CatchLightingRenderPatch.IsDoingLighting)
            {
                effect = null;
                depthStencilState = BgUtils.StencilRenderOnDark;
            }
            
        }

        public static void SpriteBatch_End_postfix()
        {
            if (Game1CatchLightingRenderPatch.IsDoingLighting)
            {
                Game1CatchLightingRenderPatch.IsDoingLighting = false;
                BgUtils.DefaultStencilOverride = null;
            }
        }

        public static void Game1_ShouldDrawOnBuffer_postfix(ref bool __result)
        {
            if (Game1.background is TortsBackground)
                __result = true;
        }

        public static void GameLocation_resetLocalState_postfix(ref GameLocation __instance)
        {
            if (__instance.NameOrUniqueName == TORTSREALM)
                Game1.background = new TortsBackground();
        }

        public static void GameLocation_cleanUpBeforePlayerExit_postfix(ref GameLocation __instance)
        {
            if (__instance.NameOrUniqueName == TORTSREALM)
                Game1.background = null;
        }

        // Can't [HarmonyPatch] SGame since it is internal
        public static class Game1CatchLightingRenderPatch
        {
            public static bool IsDoingLighting = false;

            public static void DoStuff()
            {
                IsDoingLighting = true;
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, ILGenerator ilgen)
            {
                List<CodeInstruction> ret = new();

                int countdown = 0;
                foreach (var insn in insns)
                {
                    if (insn.opcode == OpCodes.Ldsfld && insn.operand == typeof(Game1).GetField("drawLighting"))
                    {
                        countdown = 4;
                    }
                    else if (countdown > 0 && --countdown == 0)
                    {
                        ret.Add(new CodeInstruction(OpCodes.Call, typeof(Game1CatchLightingRenderPatch).GetMethod("DoStuff")));
                    }

                    ret.Add(insn);
                }

                return ret;
            }
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1.SetWindowSize))]
        public static class Game1AddStencilToScreenPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, ILGenerator ilgen)
            {
                List<CodeInstruction> ret = new();

                foreach (var insn in insns)
                {
                    if (insn.opcode == OpCodes.Ldstr && insn.operand is string str && str == "Screen")
                    {
                        ret[ret.Count - 7].opcode = OpCodes.Ldc_I4_3;
                    }

                    ret.Add(insn);
                }

                return ret;
            }
        }

    }
}