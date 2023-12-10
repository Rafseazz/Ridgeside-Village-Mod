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
using StardewValley.SpecialOrders;

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

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;

        }

        private static void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {

            Event.RegisterCustomCommand("RSVShowImage", command_RSVShowImage);
            Event.RegisterCustomCommand("RSVStopShowImage", command_RSVStopShowImage);
            Event.RegisterCustomPrecondition("rsvRidingHorse", precondition_RSVRidingHorse);
        }

        private static bool precondition_RSVRidingHorse(GameLocation location, string eventId, string[] args)
        {
            return Game1.player.isRidingHorse();
        }

        static ImageMenu currImageMenu;

        public static void command_RSVShowImage(Event @event, string[] args, EventContext context)
        {
            try
            {
                Texture2D image = Helper.GameContent.Load<Texture2D>(PathUtilities.NormalizeAssetName(args[1]));
                if (!float.TryParse(args[2], out float scale))
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
                if(args.Length < 1)
                {
                    Log.Error("RSVShowImage has no path for file");
                }
                else
                {
                    Log.Error($"Image {args[1]} not found");
                }
                //@event.CurrentCommand++;
                //@event.checkForNextCommand(location, time);
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

        public static void command_RSVStopShowImage(Event @event, string[] args, EventContext context)
        {
            try
            {
                EventPatches.Helper.Events.Display.RenderedHud -= DrawImageMenu;
                EventPatches.currImageMenu = null;

                //@event.CurrentCommand++;
            }
            catch
            {
                //@event.CurrentCommand++;
                //@event.checkForNextCommand(location, time);
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
