using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{
    internal static class EventDetection
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }

        private static Rectangle ButtonArea { get; set; }
        private static ClickableComponent RSVButton { get; set; }

        private static Texture2D RSVIcon;
        private static bool ShouldDraw { get; set; }

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            Log.Trace($"Applying Harmony Patch \"{nameof(GameMenu_ChangeTab_PostFix)}.");
            harmony.Patch(
                original: AccessTools.Method(typeof(GameMenu), nameof(GameMenu.changeTab)),
                prefix: new HarmonyMethod(typeof(EventDetection), nameof(GameMenu_ChangeTab_PostFix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(MapPage), nameof(MapPage.draw), new Type[]{ typeof(SpriteBatch)}),
                postfix: new HarmonyMethod(typeof(EventDetection), nameof(MapPage_draw_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(MapPage), nameof(MapPage.receiveLeftClick)),
                prefix: new HarmonyMethod(typeof(EventDetection), nameof(MapPage_receiveLeftClick_Prefix))
            );

            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(1200, 720);
            ButtonArea = new Rectangle((int)topLeft.X, (int)topLeft.Y + 180, 144, 104);
            
            RSVButton = new ClickableComponent(ButtonArea, "") {
                myID = 25555,
                rightNeighborID = 1001,
                downNeighborID = 1030,
                upNeighborID = 1001
            };

            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            Helper.Events.Display.WindowResized += OnWindowResized;
        }

        private static void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                RSVIcon = Helper.GameContent.Load<Texture2D>(PathUtilities.NormalizeAssetName("LooseSprites/RSVIcon"));
            }
            catch
            {
               Log.Debug("RSV: Failed to get RSVIcon on SaveLoaded event");
            }
        }

        private static void OnWindowResized(object sender, WindowResizedEventArgs e)
        {
            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(1200, 720);
            ButtonArea = new Rectangle((int)topLeft.X, (int)topLeft.Y + 180, 144, 104);
            RSVButton.bounds = ButtonArea;
        }

        internal static void GameMenu_ChangeTab_PostFix(ref GameMenu __instance, int whichTab, bool playSound = true)
        {
            try
            {
                //only draw on vanilla or RSV maps
                ShouldDraw = !Game1.currentLocation.Name.Contains('_') || Game1.currentLocation.Name.StartsWith("Custom_Ridgeside");
                if (whichTab == GameMenu.mapTab && Game1.currentLocation.Name.StartsWith("Custom_Ridgeside") && Constants.TargetPlatform != GamePlatform.Android)
                {
                    RSVWorldMap.Open(Game1.activeClickableMenu);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(GameMenu_ChangeTab_PostFix)}\" has encountered an error. \n{e.ToString()}");
            }
        }

        internal static void MapPage_draw_Postfix(ref MapPage __instance, SpriteBatch b) {
            if (!ShouldDraw)
            {
                return;
            }
            Game1.drawDialogueBox(ButtonArea.X - 92 + 60, ButtonArea.Y - 16 - 80, 250 - 42, 232, false, true);
            if (RSVIcon is null)
            {
                RSVIcon = Helper.GameContent.Load<Texture2D>(PathUtilities.NormalizeAssetName("LooseSprites/RSVIcon"));
            }
            b.Draw(RSVIcon, new Vector2(EventDetection.ButtonArea.X, EventDetection.ButtonArea.Y), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            Point mouseCoords = Game1.getMousePosition(true);
            if (ButtonArea.Contains(mouseCoords.X, mouseCoords.Y))
            {
                IClickableMenu.drawHoverText(b, Helper.Translation.Get("RSV.MapIconName"), Game1.smallFont);
            }
            __instance.drawMouse(b);
        }


        internal static bool MapPage_receiveLeftClick_Prefix(int x, int y, bool playSound) {
            if(ButtonArea.Contains(x, y))
            {
                RSVWorldMap.Open(Game1.activeClickableMenu);
                return false;
            }

            return true;

        }

    }
}
