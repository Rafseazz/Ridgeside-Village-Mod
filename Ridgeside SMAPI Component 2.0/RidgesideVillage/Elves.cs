using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{
    internal static class Elves
    {
        static IModHelper Helper;
        static IMonitor Monitor;
        
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;
            TileActionHandler.RegisterTileAction("RSVUndreyaSched", RSVUndreyaSched);
        }

        private static void RSVUndreyaSched(string tileActionString, Vector2 position)
        {
            if (!Game1.player.IsMainPlayer)
            {
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("RSV.UndreyaError"), HUDMessage.error_type));
            }
            else
            {
                if (!Game1.player.mailReceived.Contains(RSVConstants.M_UNDREYAHOME))
                {
                        var responses = new List<Response>
                    {
                        new Response("yes", Helper.Translation.Get("Offer.Yes")),
                        new Response("no", Helper.Translation.Get("Offer.No")),
                    };
                        var responseActions = new List<Action>
                    {
                        delegate
                        {
                            Game1.player.mailReceived.Add(RSVConstants.M_UNDREYAHOME);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("RSV.UndreyaWontPlay"));
                        },
                        delegate{}
                    };
                        Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("RSV.UndreyaSchedStop"), responses, responseActions);
                }
                else if (Game1.player.mailReceived.Contains(RSVConstants.M_UNDREYAHOME))
                {
                    var responses = new List<Response>
                    {
                        new Response("yes", Helper.Translation.Get("Offer.Yes")),
                        new Response("no", Helper.Translation.Get("Offer.No")),
                    };
                    var responseActions = new List<Action>
                    {
                        delegate
                        {
                            Game1.player.mailReceived.Remove(RSVConstants.M_UNDREYAHOME);
                            Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("RSV.UndreyaWillPlay"));
                        },
                        delegate{}
                    };
                    Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("RSV.UndreyaSchedStart"), responses, responseActions);
                }
            }
        }


    }
}

