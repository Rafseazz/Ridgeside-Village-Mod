using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using System.Reflection;

namespace RidgesideVillage
{
    //Harmony patches for the Summit Farm to make it behave like the player's farm
    internal static class SummitFarm
    {
        private static IModHelper Helper { get; set; }

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;
            Log.Trace($"Applying Harmony Patch from \"{nameof(SummitFarm)}\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CanPlantTreesHere)),
                postfix: new HarmonyMethod(typeof(SummitFarm), nameof(GameLocation_CanPlanTreesHere_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.spawnWeedsAndStones)),
                prefix: new HarmonyMethod(typeof(SummitFarm), nameof(GameLocation_SpawnWeedsAndStones_Prefix))
            );
            /*
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.loadLights)),
               prefix: new HarmonyMethod(typeof(SummitFarm), nameof(SummitHouse_NoAmbientLight_Prefix))
           );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.setUpLocationSpecificFlair)),
               prefix: new HarmonyMethod(typeof(SummitFarm), nameof(SummitHouse_NoAmbientLight_Prefix))
           );
            */
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CanPlaceThisFurnitureHere)),
               prefix: new HarmonyMethod(typeof(SummitFarm), nameof(GameLocation_CanPlaceThisFurnitureHere_Prefix))
           );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), "resetSharedState"),
                postfix: new HarmonyMethod(typeof(SummitFarm), nameof(SummitHouse_SetUpKitchen_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.loadMap)),
                postfix: new HarmonyMethod(typeof(SummitFarm), nameof(SummitHouse_SetUpKitchen_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.draw)),
                postfix: new HarmonyMethod(typeof(SummitFarm), nameof(GameLocation_draw_Postfix))
            );

            Helper.Events.Player.Warped += OnWarped;
            Helper.Events.GameLoop.DayEnding += OnDayEnded;
        }

        private static void OnDayEnded(object sender, DayEndingEventArgs e)
        {
            if (Game1.getFarm().isBuildingConstructed("Gold Clock") && !Game1.MasterPlayer.mailReceived.Contains(RSVConstants.M_GOLDCLOCK))
            {
                Game1.MasterPlayer.mailReceived.Add(RSVConstants.M_GOLDCLOCK);
            }
        }

        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation.Name.Equals(RSVConstants.L_SUMMITFARM)){
                if(Game1.random.NextDouble() > 0.01)
                {
                    return;
                }

                Random r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
                GameLocation summitFarm = e.NewLocation;
                for (int i = 0; i < 1000; i++)
                {
                    //below y = 32
                    int x = r.Next(90);
                    int y = r.Next(50) + 30;
                    if (summitFarm.isTileLocationTotallyClearAndPlaceable(x, y) || summitFarm.isTileHoeDirt(new Vector2(x,y)))
                    {
                        summitFarm.addCritter(new StardewValley.BellsAndWhistles.Crow(x, y));
                    }
                }
            }
        }

        private static void GameLocation_CanPlanTreesHere_Postfix(ref GameLocation __instance, int sapling_index, int tile_x, int tile_y, ref bool __result)
        {
            try
            {
                if(__instance != null && __instance.Name.Equals(RSVConstants.L_SUMMITFARM))
                {
                    __result = true;
                }
                return;
            }
            catch(Exception e)
            {

                Log.Error($"Harmony patch \"{nameof(GameLocation_CanPlanTreesHere_Postfix)}\" has encountered an error. \n{e}");
                return;
            }
        }

        private static bool GameLocation_SpawnWeedsAndStones_Prefix(ref GameLocation __instance)
        {
            try
            {
                if (__instance != null && __instance.Name.Equals(RSVConstants.L_SUMMITFARM) && Game1.getFarm().isBuildingConstructed("Gold Clock"))
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {

                Log.Error($"Harmony patch \"{nameof(GameLocation_SpawnWeedsAndStones_Prefix)}\" has encountered an error. \n{e}");
                return true;
            }
        }

        private static bool SummitHouse_NoAmbientLight_Prefix(ref GameLocation __instance)
        {
            if (__instance.NameOrUniqueName == RSVConstants.L_SUMMITHOUSE.Split("New")[0]) // Old Summit House
            {
                if (Game1.isDarkOut())
                {
                    Game1.ambientLight = new Color(180, 180, 0, 255);
                }
                return false;
            }
            return true;
        }

        private static bool GameLocation_CanPlaceThisFurnitureHere_Prefix(ref GameLocation __instance, StardewValley.Objects.Furniture furniture, ref bool __result)
        {
            if (furniture.furniture_type.Value == 15 && __instance.NameOrUniqueName == RSVConstants.L_SUMMITHOUSE)
            {
                __result = true;
                return false;
            }
            return true;    
        }

        private static void SummitHouse_SetUpKitchen_Postfix(ref GameLocation __instance)
        {
            if (__instance.NameOrUniqueName == RSVConstants.L_SUMMITHOUSE && __instance.modData.ContainsKey("renovated"))
                SummitHouse.SetUpKitchen(__instance);
        }

        private static void GameLocation_draw_Postfix(ref GameLocation __instance, ref SpriteBatch b)
        {
            if (__instance.NameOrUniqueName != RSVConstants.L_SUMMITFARM)
                return;

            if (!Game1.MasterPlayer.mailReceived.Contains(RSVConstants.M_HOUSEUPGRADED))
                return;

            if (Game1.mailbox.Count > 0)
            {
                float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
                Point mailbox_position = new(23, 46);
                float draw_layer = (mailbox_position.X + 1) * 64 / 10000f + (mailbox_position.Y * 64) / 10000f;
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mailbox_position.X * 64, mailbox_position.Y * 64 - 96 - 48 + yOffset)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 1E-06f);
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mailbox_position.X * 64 + 32 + 4, mailbox_position.Y * 64 - 64 - 24 - 8 + yOffset)), new Rectangle(189, 423, 15, 13), Color.White, 0f, new Vector2(7f, 6f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
            }
        }
    }
}
