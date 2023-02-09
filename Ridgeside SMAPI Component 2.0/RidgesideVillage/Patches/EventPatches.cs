using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{
    //Corrects the location name in the "X has begun in Y" message
    internal static class EventPatches
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;

            Log.Trace($"Applying Harmony Patch \"{nameof(ShowGlobalMessage_Prefix)}.");
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.showGlobalMessage)),
                prefix: new HarmonyMethod(typeof(EventPatches), nameof(ShowGlobalMessage_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkEventPrecondition)),
                prefix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.checkEventPrecondition_Prefix))
            );
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;

        }

        private static void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            MethodInfo showImageCommands = typeof(EventPatches).GetMethod("command_RSVShowImage");
            ExternalAPIs.SC.AddEventCommand("RSVShowImage", showImageCommands);

            MethodInfo stopShowImageCommands = typeof(EventPatches).GetMethod(nameof(EventPatches.command_RSVStopShowImage));
            ExternalAPIs.SC.AddEventCommand("RSVStopShowImage", stopShowImageCommands);

            MethodInfo AddSOCommand = typeof(EventPatches).GetMethod("command_RSVAddSO");
            ExternalAPIs.SC.AddEventCommand("RSVAddSO", AddSOCommand);
        }


        public static void command_RSVAddSO(Event @event, GameLocation location, GameTime time, string[] split)
        {
            string specialOrderKey = split[2];
            try
            {
                if (!Game1.player.team.SpecialOrderActive(specialOrderKey))
                {
                    Game1.player.team.specialOrders.Add(SpecialOrder.GetSpecialOrder(specialOrderKey, null));
                }
                @event.CurrentCommand++;
                @event.checkForNextCommand(location, time);
            }
            catch
            {
                Log.Error($"Special order {specialOrderKey} not found and thus not added");
                @event.CurrentCommand++;
                @event.checkForNextCommand(location, time);
            }
        }


        static ImageMenu currImageMenu;

        public static void command_RSVShowImage(Event @event, GameLocation location, GameTime time, string[] split)
        {
            try
            {
                Texture2D image = Helper.GameContent.Load<Texture2D>(PathUtilities.NormalizeAssetName(split[1]));
                if (!float.TryParse(split[2], out float scale))
                {
                    scale = 1f;
                }

                Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen((int)(image.Width * scale), (int)(image.Height * scale));
                //dialog is 500px 
                topLeft.Y = Math.Min(topLeft.Y, Game1.viewport.Height - image.Height*scale - 510);
                topLeft.Y = Math.Max(topLeft.Y, 0);
                EventPatches.currImageMenu = new ImageMenu((int)topLeft.X, (int)topLeft.Y, scale, image, false);
                @event.CurrentCommand++;


                EventPatches.Helper.Events.Display.RenderedHud += DrawImageMenu;

                @event.CurrentCommand++;

            }
            catch
            {
                if(split.Length < 1)
                {
                    Log.Error("RSVShowImage has no path for file");
                }
                else
                {
                    Log.Error($"Image {split[1]} not found");
                }
                @event.CurrentCommand++;
                @event.checkForNextCommand(location, time);
            }
        }

        private static void DrawImageMenu(object sender, RenderedHudEventArgs e)
        {
            if(!Game1.eventUp || EventPatches.currImageMenu is null)
            {
                EventPatches.Helper.Events.Display.RenderedHud -= DrawImageMenu;
            }
            else
            {
                EventPatches.currImageMenu.draw(e.SpriteBatch);
            }
        }

        public static void command_RSVStopShowImage(Event @event, GameLocation location, GameTime time, string[] split)
        {
            try
            {
                EventPatches.Helper.Events.Display.RenderedHud -= DrawImageMenu;
                EventPatches.currImageMenu = null;

                @event.CurrentCommand++;
            }
            catch
            {
                @event.CurrentCommand++;
                @event.checkForNextCommand(location, time);
            }
        }


        internal static void ShowGlobalMessage_Prefix(ref string message)
        {
            try
            {
                if (message.Contains(RSVConstants.L_VILLAGE))
                {
                    message = message.Replace(RSVConstants.L_VILLAGE, Helper.Translation.Get("Gathering.location"));

                }
                else if (message.Contains(RSVConstants.L_RIDGE))
                {
                    message = message.Replace(RSVConstants.L_RIDGE, Helper.Translation.Get("EoR.location"));
                }
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(ShowGlobalMessage_Prefix)}\" has encountered an error. \n{e}");
            }
        }

        internal static bool checkEventPrecondition_Prefix(ref string precondition, ref int __result)
        {
            if (precondition.Contains("/rsvRidingHorse", StringComparison.OrdinalIgnoreCase))
            {
                if(Game1.player.mount is null)
                {
                    __result = -1;
                    return false;
                }
                precondition = precondition.Replace("/rsvRidingHorse", "", StringComparison.OrdinalIgnoreCase);
                return true;
            }
            else
            {
                return true;
            }
        }

    }
}
