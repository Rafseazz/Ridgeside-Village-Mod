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
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using System.Reflection;

namespace RidgesideVillage
{
    //Handles the treasures for the spirit realm.
    //Mostly hardcoded for performance
    internal static class TreasureItems
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }
        
        //activate tracker for fox statue
        private static bool OnFoxStatueMap;
        //count consecutive 10min intervals where player was close to statue
        private readonly static PerScreen<int> FoxStatueCounter = new PerScreen<int>();


        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            //Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            Helper.Events.Player.Warped += OnWarped;
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.GameLoop.ReturnedToTitle += OnReturnToTitle;
            Helper.ConsoleCommands.Add("RSV_ResetArtifacts", "", (_,_) => { ResetArtifacts(); Log.Info("Reset Artifacts"); });

            Log.Trace($"Applying Harmony Patch from \"{nameof(TreasureItems)}\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(Axe), nameof(Axe.DoFunction)),
                postfix: new HarmonyMethod(typeof(TreasureItems), nameof(Axe_DoFunction_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Hoe), nameof(Hoe.DoFunction)),
                postfix: new HarmonyMethod(typeof(TreasureItems), nameof(Hoe_DoFunction_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(WateringCan), nameof(WateringCan.DoFunction)),
                postfix: new HarmonyMethod(typeof(TreasureItems), nameof(WateringCan_DoFunction_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.DoFunction)),
                postfix: new HarmonyMethod(typeof(TreasureItems), nameof(FishingRod_DoFunction_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.pullFishFromWater)),
                postfix: new HarmonyMethod(typeof(TreasureItems), nameof(FishingRod_PullFishFromWater_PostFix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getFish)),
                postfix: new HarmonyMethod(typeof(TreasureItems), nameof(GameLocation_GetFish_Postifx))
            );

        }

        private static void OnReturnToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            FoxStatueCounter.Value = 0;
            OnFoxStatueMap = false;
            Helper.Events.GameLoop.TimeChanged -= OnTimeChanged;
        }

        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            //reset flags every season
            if(Game1.dayOfMonth == 1)
            {
                ResetArtifacts();   
            }
        }

        private static void ResetArtifacts()
        {
            var flags = new List<string>() { RSVConstants.M_MOOSE, RSVConstants.M_FOXMASK, RSVConstants.M_SAPPHIRE, RSVConstants.M_MUSICBOX, RSVConstants.M_EVERFROST, RSVConstants.M_HEROSTATUE, RSVConstants.M_CANDELABRUM, RSVConstants.M_ELVENCOMB, RSVConstants.M_OPALHALO };
            foreach (var flag in flags)
            {
                Game1.player.mailReceived.Remove(flag);
            }
        }

        //setup tracker for fox statue if needed
        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!Game1.player.eventsSeen.Contains(RSVConstants.E_RAEUNSEAL))
                return;

            if (!OnFoxStatueMap && e.NewLocation.Name.Equals(RSVConstants.L_RIDGE) && !Game1.player.mailReceived.Contains(RSVConstants.M_FOXMASK)){
                FoxStatueCounter.Value = 0;
                Helper.Events.GameLoop.TimeChanged += OnTimeChanged;
                OnFoxStatueMap = true;
            }
            else if(OnFoxStatueMap && e.OldLocation.Name.Equals(RSVConstants.L_RIDGE) && !e.NewLocation.Name.Equals(RSVConstants.L_RIDGE)){

                FoxStatueCounter.Value = 0;
                Helper.Events.GameLoop.TimeChanged -= OnTimeChanged;
                OnFoxStatueMap = false;
            }
        }

        //track player position for fox statue
        private static void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (!Game1.player.eventsSeen.Contains(RSVConstants.E_RAEUNSEAL))
                return;

            int distance = (int) (Math.Abs(Game1.player.Tile.X - 18) + Math.Abs(Game1.player.Tile.Y - 10));
            if(distance < 3)
            {
                FoxStatueCounter.Value += 1;
            }
            else
            {
                FoxStatueCounter.Value = 0;
            }

            if(FoxStatueCounter.Value >= 12)
            {
                SpawnJAItemAsDebris(RSVConstants.IRELICFOXMASK, 18, 10, Game1.getLocationFromName(RSVConstants.L_RIDGE));
                Game1.player.mailReceived.Add(RSVConstants.M_FOXMASK);
                FoxStatueCounter.Value = 0;
                Helper.Events.GameLoop.TimeChanged -= OnTimeChanged;
                OnFoxStatueMap = false;
            }
        }

        private static void GameLocation_GetFish_Postifx(GameLocation __instance, ref Item __result, float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName)
        {
            if (!Game1.player.eventsSeen.Contains(RSVConstants.E_RAEUNSEAL))
                return;

            if ((int)bobberTile.X == 60 && (int)bobberTile.Y == 54 && Game1.currentLocation.Name.Equals(RSVConstants.L_VILLAGE) && !Game1.player.mailReceived.Contains(RSVConstants.M_SAPPHIRE))
            {
                __result = new StardewValley.Object(RSVConstants.ISAPPHIREPEARL, 1);
            }
        }

        internal static void Axe_DoFunction_Postfix(ref Axe __instance, int x, int y, int power, Farmer who)
        {
            if (!Game1.player.eventsSeen.Contains(RSVConstants.E_RAEUNSEAL))
                return;

            try
            {
                int tileX = x / 64;
                int tileY = y / 64;
                if(tileX == 14 && tileY == 3 && Game1.currentLocation.Name.Equals(RSVConstants.L_HOTEL3) && !Game1.player.mailReceived.Contains(RSVConstants.M_MUSICBOX))
                {
                    SpawnJAItemAsDebris(RSVConstants.IMUSICBOX, tileX, tileY, Game1.currentLocation);
                    Game1.player.mailReceived.Add(RSVConstants.M_MUSICBOX);
                }
                else if(tileX == 5 && tileY == 3 && Game1.currentLocation.Name.Equals(RSVConstants.L_ALISSASHED) && !Game1.player.mailReceived.Contains(RSVConstants.M_EVERFROST))
                {
                    Game1.player.mailReceived.Add(RSVConstants.M_EVERFROST);
                    SpawnJAItemAsDebris(RSVConstants.IEVERFROSTSTONE, tileX, tileY, Game1.currentLocation);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(Axe_DoFunction_Postfix)}\" has encountered an error. \n{e.ToString()}");
            }
        }

        internal static void Hoe_DoFunction_Postfix(ref Hoe __instance, int x, int y, int power, Farmer who)
        {
            if (!Game1.player.eventsSeen.Contains(RSVConstants.E_RAEUNSEAL))
                return;

            try
            {
                int tileX = x / 64;
                int tileY = y / 64;
                if (tileX == 80 && tileY == 22 && Game1.currentLocation.Name.Equals(RSVConstants.L_CLIFF) && !Game1.player.mailReceived.Contains(RSVConstants.M_MOOSE))
                {
                    SpawnJAItemAsDebris(RSVConstants.IMOOSESTATUE, tileX, tileY, Game1.currentLocation);
                    Game1.player.mailReceived.Add(RSVConstants.M_MOOSE);

                }
                else if (tileX == 63 && tileY == 40 && Game1.currentLocation.Name.Equals(RSVConstants.L_HIKE) && !Game1.player.mailReceived.Contains(RSVConstants.M_ELVENCOMB))
                {
                    SpawnJAItemAsDebris(RSVConstants.IELVENCOMB, tileX, tileY, Game1.currentLocation);
                    Game1.player.mailReceived.Add(RSVConstants.M_ELVENCOMB);
                }
                else if (tileX == 23 && tileY == 6 && Game1.currentLocation.Name.Equals(RSVConstants.L_CABLECAR) && !Game1.player.mailReceived.Contains(RSVConstants.M_OPALHALO))
                {
                    SpawnJAItemAsDebris(RSVConstants.IOPALHALO, tileX, tileY, Game1.currentLocation);
                    Game1.player.mailReceived.Add(RSVConstants.M_OPALHALO);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(Hoe_DoFunction_Postfix)}\" has encountered an error. \n{e.ToString()}");
            }
        }

        internal static void WateringCan_DoFunction_Postfix(ref WateringCan __instance, int x, int y, int power, Farmer who)
        {
            if (!Game1.player.eventsSeen.Contains(RSVConstants.E_RAEUNSEAL))
                return;

            try
            {
                int tileX = x / 64;
                int tileY = y / 64;
                if (tileX == 11 && tileY == 7 && Game1.currentLocation.Name.Equals(RSVConstants.L_HAUNTEDGH) && !Game1.player.mailReceived.Contains(RSVConstants.M_CANDELABRUM))
                {
                    SpawnJAItemAsDebris(RSVConstants.ICANDELABRUM, tileX, tileY, Game1.currentLocation);
                    Game1.player.mailReceived.Add(RSVConstants.M_CANDELABRUM);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(WateringCan_DoFunction_Postfix)}\" has encountered an error. \n{e.ToString()}");
            }
        }

        //change result to sapphire if appropriate
        internal static void FishingRod_DoFunction_Postfix(ref WateringCan __instance, int x, int y, int power, Farmer who)
        {
            if (!Game1.player.eventsSeen.Contains(RSVConstants.E_RAEUNSEAL))
                return;

            try
            {
                if(who.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
                {
                    return;
                }
                int tileX = x / 64;
                int tileY = y / 64;

                if (tileX == 145 && tileY == 69 && Game1.currentLocation.Name.Equals(RSVConstants.L_VILLAGE) && !Game1.player.mailReceived.Contains(RSVConstants.M_HEROSTATUE))
                {
                    SpawnJAItemAsDebris(RSVConstants.IHEROSTATUE, tileX, tileY, Game1.currentLocation);
                    Game1.player.mailReceived.Add(RSVConstants.M_HEROSTATUE);
                }
                
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(FishingRod_DoFunction_Postfix)}\" has encountered an error. \n{e.ToString()}");
            }
        }

        //if player pulled sapphire, add flag. not done in getFish() cus mod compatibility
        private static void FishingRod_PullFishFromWater_PostFix(ref FishingRod __instance, string  fishId, int fishSize, int fishQuality, int fishDifficulty, bool treasureCaught, bool wasPerfect, bool fromFishPond)
        {
            if (fishId.Equals(RSVConstants.ISAPPHIREPEARL))
            {
                Game1.player.mailReceived.Add(RSVConstants.M_SAPPHIRE);
            }
        }

        internal static void SpawnJAItemAsDebris(string itemID, int tileX, int tileY, GameLocation location)
        {
            if (itemID != null)
            {
                Game1.createObjectDebris(itemID, tileX, tileY, location: location);
            }

        }


    }
}
