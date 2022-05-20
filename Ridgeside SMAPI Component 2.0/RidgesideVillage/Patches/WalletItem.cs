using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using StardewValley;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using Netcode;
using SpaceCore.Events;
using SpaceCore.Interface;
using HarmonyLib;

namespace RidgesideVillage
{
    //This section is heavily inspired by spacechase0's Moon Misadventures code, which can be found here:
    //https://github.com/spacechase0/StardewValleyMods/tree/develop/MoonMisadventures
    internal static class WalletItem
    {
        const int UNLOCKEVENT = 75160380;

        private static IModHelper Helper { get; set; }
        public static Texture2D image;

        internal class Holder { public readonly NetBool Value = new(); }
        internal static ConditionalWeakTable<FarmerTeam, Holder> values = new();

        public static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            Helper.ConsoleCommands.Add("RSV_rivera_secret", "Gives you the Rivera Family Secret item until you exit the save.", GetItemCommand);
            image = Helper.Content.Load<Texture2D>("assets/RiveraSecret.png");

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            SpaceEvents.AddWalletItems += AddWalletItems;
            SpaceEvents.OnEventFinished += OnEventFinished;
            try
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Object), "getPriceAfterMultipliers"),
                    postfix: new HarmonyMethod(typeof(WalletItem), nameof(WalletItem.Object_getPriceAfterMultipliers_Postfix))
                    );
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(WalletItem)}\" has encountered an error. \n{e.ToString()}");
                return;
            }

        }

        public static void Object_getPriceAfterMultipliers_Postfix(StardewValley.Object __instance, float startPrice, ref float __result, long specificPlayerID = -1L)
        {
            if (__instance.Category != -4)
            {
                return;
            }
            if (!__instance.GetContextTagList().Contains("rsv_fish"))
            {
                return;
            }
            float saleMultiplier = 1f;
            foreach (Farmer player in Game1.getAllFarmers())
            {
                if (Game1.player.useSeparateWallets)
                {
                    if (specificPlayerID == -1)
                    {
                        if (player.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID || !player.isActive())
                        {
                            continue;
                        }
                    }
                    else if (player.UniqueMultiplayerID != specificPlayerID)
                    {
                        continue;
                    }
                }
                else if (!player.isActive())
                {
                    continue;
                }
                if (player.team.get_hasRiveraSecret().Value)
                {
                    saleMultiplier = 2f;
                    break;
                }
            }
            __result = __result * saleMultiplier;
            return;
        }

        private static void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ExternalAPIs.SC.RegisterCustomProperty(typeof(FarmerTeam), "hasRiveraSecret", typeof(NetBool), AccessTools.Method(typeof(WalletItem), nameof(WalletItem.get_hasRiveraSecret)), AccessTools.Method(typeof(WalletItem), nameof(WalletItem.set_hasRiveraSecret)));
        }

        private static void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Game1.player.eventsSeen.Contains(UNLOCKEVENT))
            {
                Game1.player.team.get_hasRiveraSecret().Value = true;
            }
        }

        private static void AddWalletItems(object sender, EventArgs e)
        {
            var page = sender as NewSkillsPage;
            if (Game1.player.team.get_hasRiveraSecret().Value)
                page.specialItems.Add(new ClickableTextureComponent(
                    name: "", bounds: new Rectangle(-1, -1, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom),
                    label: null, hoverText: Helper.Translation.Get("RSV.RiveraSecret"),
                    texture: image, sourceRect: new Rectangle(0, 0, 16, 16), scale: 4f, drawShadow: true));
        }

        private static void OnEventFinished(object sender, EventArgs e)
        {
            if (Game1.CurrentEvent.id == UNLOCKEVENT)
            {
                Game1.player.team.get_hasRiveraSecret().Value = true;
            }
        }

        private static void GetItemCommand(string cmd, string[] args)
        {
            Game1.player.team.get_hasRiveraSecret().Value = true;
            Log.Trace($"{Game1.player.Name} has temporarily received the Rivera Family Secret.");
        }
        
        public static void set_hasRiveraSecret(this FarmerTeam farmer, NetBool newVal)
        {
            farmer.get_hasRiveraSecret().Value = newVal.Value;
        }

        public static NetBool get_hasRiveraSecret(this FarmerTeam farmer)
        {
            var holder = values.GetOrCreateValue(farmer);
            return holder.Value;
        }

    }
  
}
