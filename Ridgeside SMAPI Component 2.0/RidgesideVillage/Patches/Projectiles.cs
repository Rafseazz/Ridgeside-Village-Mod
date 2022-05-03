using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netcode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Network;
using StardewValley.Projectiles;
using StardewValley.BellsAndWhistles;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using HarmonyLib;

namespace RidgesideVillage
{

    internal static class Projectiles
    {
        private static IModHelper Helper { get; set; }
        public static Texture2D sprite;
        public static float rotation = (float)Math.PI / 16f;

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;
            sprite = Helper.Content.Load<Texture2D>("assets/Poof.png");

            harmony.Patch(
                original: AccessTools.Method(typeof(DinoMonster), nameof(DinoMonster.behaviorAtGameTick)),
                transpiler: new HarmonyMethod(typeof(MonsterProjectilePatch).GetMethod("Transpiler"))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Ghost), nameof(Ghost.behaviorAtGameTick)),
                transpiler: new HarmonyMethod(typeof(MonsterProjectilePatch).GetMethod("Transpiler"))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(LavaLurk), nameof(LavaLurk.behaviorAtGameTick)),
                transpiler: new HarmonyMethod(typeof(MonsterProjectilePatch).GetMethod("Transpiler"))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(SquidKid), nameof(SquidKid.behaviorAtGameTick)),
                transpiler: new HarmonyMethod(typeof(MonsterProjectilePatch).GetMethod("Transpiler"))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(BlueSquid), nameof(BlueSquid.behaviorAtGameTick)),
                transpiler: new HarmonyMethod(typeof(MonsterProjectilePatch).GetMethod("Transpiler"))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(ShadowShaman), nameof(ShadowShaman.behaviorAtGameTick)),
                transpiler: new HarmonyMethod(typeof(MonsterProjectilePatch).GetMethod("Transpiler"))
            );
        }

        public static class MonsterProjectilePatch
        {
            public static void ReplaceProjectile(NetCollection<Projectile> projectiles, Projectile original)
            {
                //Log.Debug($"RSV: Entered transpiler, woohoo!");
                NetCharacterRef firerInfo = Helper.Reflection.GetField<NetCharacterRef>(original, "theOneWhoFiredMe").GetValue();
                Monster monster = (Monster)firerInfo.Get(Game1.currentLocation);
                try
                {
                    if (monster.modData["RSV_bloomDebuff"] != "true")
                    {
                        projectiles.Add(original);
                        return;
                    }
                }
                catch
                {
                    projectiles.Add(original);
                    return;
                }
                float xVelocity = Helper.Reflection.GetField<NetFloat>(original, "xVelocity").GetValue().Value;
                float yVelocity = Helper.Reflection.GetField<NetFloat>(original, "yVelocity").GetValue().Value;
                monster.currentLocation.projectiles.Add(new MistProjectile(rotation, xVelocity, yVelocity, monster.Position + new Vector2(0f, -32f), monster.currentLocation, monster));
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, ILGenerator ilgen)
            {
                //Log.Debug($"RSV: Hopefully transpiling...");
                List<CodeInstruction> ret = new();

                foreach (var insn in insns)
                {
                    if (insn.opcode == OpCodes.Callvirt && (insn.operand?.Equals(AccessTools.Method(typeof(NetCollection<Projectile>), nameof(NetCollection<Projectile>.Add)))) == true)
                    {
                        ret.Add(new CodeInstruction(OpCodes.Call, typeof(MonsterProjectilePatch).GetMethod("ReplaceProjectile")));
                    }
                    else
                    {
                        ret.Add(insn);
                    }
                    //Log.Debug($"RSV: {ret.Last()}");
                }
                return ret;
            }
        }
    }

}