using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RidgesideVillage.Offering
{
    internal class OfferingTileAction
    {
        static IModHelper Helper;
        static IMonitor Monitor;

        static OfferingData Data;

        static bool PerformedOfferingToday = false;
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;

            GameLocation.RegisterTileAction("RSVOffering", DoOffering);

            Helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private static bool DoOffering(GameLocation location, string[] arg2, Farmer farmer, Point point)
        {
            if (PerformedOfferingToday || Game1.player.CurrentItem == null)
            {
                return false;
            }
            var responses = new List<Response>
                    {
                        new Response("yes", Helper.Translation.Get("Offer.yes")),
                        new Response("no", Helper.Translation.Get("Offer.No"))
                    };
            var responseActions = new List<Action>
                    {
                        delegate
                        {
                            performOffering();
                        },
                        delegate { }
                    };

            Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("Offer.Question", new { itemName = Game1.player.CurrentItem.DisplayName }), responses.ToArray(), responseActions);
            return true;
        }

        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            Data = new OfferingData();
            PerformedOfferingToday = false;
        }

        static void performOffering()
        {
            Farmer player = Game1.player;
            Item currentHeldItem = player.CurrentItem;
            if (currentHeldItem == null || !currentHeldItem.canBeTrashed() || !currentHeldItem.canBeDropped())
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Offer.IllegalItem"));
                return;
            }
            DropItem(player);
            var Offer = FindCorrespondingOffer(currentHeldItem);

            if(Offer == null)
            {
                //do standard thing or so?
                return;
            }
            bool found = Game1.currentLocation.TryGetLocationEvents(out String assetName, out Dictionary<String, String> events);
            if(!found)
            {
                return;
            }
            if (events.TryGetValue(Offer.ScriptKey, out string EventScript))
            {
                PerformedOfferingToday = true;
                Game1.delayedActions.Add(new DelayedAction(1500, delegate {
                    Game1.fadeScreenToBlack();
                }));
                Game1.delayedActions.Add(new DelayedAction(2000, delegate {
                    Game1.currentLocation.startEvent(new Event(EventScript));
                }));

                Game1.delayedActions.Add(new DelayedAction(5000, delegate {
                    Offer.Apply();
                }));
            }
        }

        static OfferEntry FindCorrespondingOffer(Item item)
        {
            var data = new OfferingData();
            if (data.lookup.TryGetValue(item.Name, out OfferEntry Offer))
            {
                return Offer;
            }
            foreach (var tag in item.GetContextTags())
            {
                if (data.lookup.TryGetValue(tag, out Offer))
                {
                    return Offer;
                }
            }
            return null;
        }

        static void DropItem(Farmer player)
        {
            if (player.CurrentItem != null && player.CurrentItem.canBeDropped())
            {
                GameLocation location = Game1.currentLocation;
                Debris thrownItem = Game1.createItemDebris(player.CurrentItem.getOne(), player.getStandingPosition() + new Vector2(0, -32), 0);
                player.reduceActiveItemByOne();

                //sink the item after 500ms
                Game1.delayedActions.Add(new DelayedAction(500, () => {
                    Chunk thrownItemChunk = thrownItem.Chunks[0];
                    Vector2 chunkTile = thrownItemChunk.position.Value / 64f;
                    location.sinkDebris(thrownItem, chunkTile, thrownItem.Chunks[0].position.Value);
                    location.debris.Remove(thrownItem);
                }));
            }                           
        }
    }
}
