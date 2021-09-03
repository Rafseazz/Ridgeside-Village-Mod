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
    public static class TileActionHandler
    {

        static Dictionary<string, Action<string>> tileActions = new Dictionary<string, Action<string>>();

        static Mod ModRef;
        internal static void Initialize(Mod Mod)
        {
            ModRef = Mod;
            ModRef.Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        internal static void RegisterTileAction(string name, Action<string> actionFunction)
        {
            Log.Debug($"Registered {name}");
            tileActions.Add(name, actionFunction);
        }

        private static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //Log.Debug("Button pressed");
            var Helper = ModRef.Helper;
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
            || Game1.IsFading();

            //Will only trigger if player can move
            if (probablyDontCheck)
            {
                return;
            }

            if (!e.Button.IsActionButton())
                return;
            Vector2 clickedTile = Helper.Input.GetCursorPosition().GrabTile;
            string actionString = Game1.currentLocation.doesTileHaveProperty(((int)clickedTile.X), ((int)clickedTile.Y), "Action", "Buildings");

            if (actionString != null && actionString != "")
            {

                //Log.Debug($"FOUND ACTION {actionString}");
                foreach (var key in tileActions.Keys)
                {
                    if (actionString.StartsWith(key))
                    {
                        tileActions[key](actionString);
                        break;
                    }
                }
            }
        }
    }
}
