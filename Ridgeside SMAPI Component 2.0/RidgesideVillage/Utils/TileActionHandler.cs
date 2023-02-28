using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace RidgesideVillage
{
    public class TileActionHandler
    {

        static Dictionary<string, Action<string, Vector2>> tileActions = new Dictionary<string, Action<string, Vector2>>();

        static IModHelper Helper;
        internal static void Initialize(IModHelper Helper)
        {
            TileActionHandler.Helper = Helper;
            TileActionHandler.Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        internal static void RegisterTileAction(string name, Action<string, Vector2> actionFunction)
        {
            Log.Trace($"Registered {name}");
            tileActions.Add(name, actionFunction);
        }

        private static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {


            if (!e.Button.IsActionButton())
                return;
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            //Checks if player can move
            bool probablyDontCheck =
            !StardewModdingAPI.Context.CanPlayerMove
            || Game1.player.isRidingHorse()
            || Game1.currentLocation == null
            || Game1.eventUp
            || Game1.isFestival()
            || Game1.IsFading()
            || Game1.menuUp;

            //Will only trigger if player can move
            if (probablyDontCheck)
            {
                return;
            }

            Vector2 clickedTile = Vector2.Zero;
            string actionString = "";
            bool usingGamepad = Game1.options.gamepadControls;
            if (usingGamepad)
            {
                clickedTile = Utility.clampToTile(Game1.player.GetToolLocation(Helper.Input.GetCursorPosition().ScreenPixels))/64f;
                actionString = Game1.currentLocation.doesTileHaveProperty(((int)clickedTile.X), ((int)clickedTile.Y), "Action", "Buildings");
            }
            if (!usingGamepad || String.IsNullOrWhiteSpace(actionString))
            {
                clickedTile = Helper.Input.GetCursorPosition().GrabTile;
                actionString = Game1.currentLocation.doesTileHaveProperty(((int)clickedTile.X), ((int)clickedTile.Y), "Action", "Buildings");

            }

            if (actionString != null && actionString != "")
            {
                Log.Trace($"Checking for {actionString}");
                foreach (var key in tileActions.Keys)
                {
                    if (actionString.StartsWith(key))
                    {
                        tileActions[key](actionString, clickedTile);
                        break;
                    }
                }
            }
        }
    }
}
