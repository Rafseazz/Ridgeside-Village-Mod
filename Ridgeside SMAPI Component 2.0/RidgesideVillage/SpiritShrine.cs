using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SObject = StardewValley.Object;

namespace RidgesideVillage
{
    public class SpiritShrine
    {
        IModHelper Helper;

        const string PORTALFLAG = "RSV.Opened_Portal";
        //whether we have to check for portal finish
        internal bool StartedOpeningEvent = false;
        static List<PedestalTemplate> PedestalTemplates;

        public SpiritShrine(IMod Mod)
        {
            this.Helper = Mod.Helper;
            this.Helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            


            if (Game1.player.IsMainPlayer)
            {

                PedestalTemplates = Helper.Data.ReadJsonFile<List<PedestalTemplate>>(PathUtilities.NormalizePath("assets/PedestalInfo.json"));
                if(PedestalTemplates == null)
                {
                    Log.Error("Couldnt load PedestalData. Please write a comment on the ridgeside discord or nexusmods page :)");
                }
                AddMissingPedestals();
                StartedOpeningEvent = Game1.player.mailReceived.Contains(PORTALFLAG);
                if (!StartedOpeningEvent)
                {
                    Helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
                }
            }
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnOneSecondUpdateTicked;
        }

        internal void OpenPortal(string arg1, string[] arg2)
        {
            if (!Game1.IsMasterGame)
            {
                return;
            }
            GameLocation location = Game1.getLocationFromName("Custom_Ridgeside_RidgeFalls");
            foreach (var pedestalTemplate in PedestalTemplates)
            {

                ItemPedestal pedestal = (ItemPedestal)location.Objects[pedestalTemplate.tilePosition];
                pedestal.heldObject.Value = (SObject) pedestal.requiredItem.Value.getOne();
                pedestal.UpdateItemMatch();

            }
        }

        internal void ResetPedestals(string arg1, string[] arg2)
        {
            if (Game1.IsMasterGame)
            {
                GameLocation location = Game1.getLocationFromName("Custom_Ridgeside_RidgeFalls");
                Log.Trace($"Resetting pedestal in {location.Name}");
                foreach(var pedestal in PedestalTemplates)
                {
                    location.Objects.Remove(pedestal.tilePosition);
                    Log.Trace($"Removing pedestal at pedestal at {pedestal.tilePosition}");
                }
                this.AddMissingPedestals();
            }
        }

       
        internal void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !Game1.player.IsMainPlayer)
            {
                return;
            }else if (StartedOpeningEvent)
            {
                return;
            }
            try
            {
                GameLocation location = Game1.getLocationFromName("Custom_Ridgeside_RidgeFalls");
                bool all_done = true;
                foreach (var pedestal in PedestalTemplates)
                {
                    ItemPedestal itemPedestal = (ItemPedestal)location.Objects[pedestal.tilePosition];
                    if (!itemPedestal.match.Value)
                    {
                        all_done = false;
                        break;
                    }
                }
                if (all_done)
                {
                    //check if we can start the event
                    if(Game1.eventUp || Game1.currentLocation.currentEvent != null || Game1.farmEvent != null || !StardewModdingAPI.Context.CanPlayerMove || Game1.IsFading())
                    {
                        return;
                    }
                    foreach (var pedestal in PedestalTemplates)
                    {
                        ItemPedestal itemPedestal = (ItemPedestal)location.Objects[pedestal.tilePosition];
                        itemPedestal.locked.Value = true;
                    }
                    StartedOpeningEvent = true;
                    Helper.Events.GameLoop.OneSecondUpdateTicked -= OnOneSecondUpdateTicked;
                    var Events = location.GetLocationEvents();
                    
                    var PortalEvent = new Event(Events["75160256/y 2/n InexistentMailFlag"], eventID: 75160256);
                    // Moved add special order command to UntimedSO
                    UtilFunctions.StartEvent(PortalEvent, "Custom_Ridgeside_RidgeFalls", 15, 43);
                }
            }
            catch (Exception exception) { Log.Warn("Issue with pedestals detected in OneSecondUpdate. Check Trace for Details");
                Log.Trace(exception.Message);
                Log.Trace(exception.StackTrace);
                Helper.Events.GameLoop.OneSecondUpdateTicked -= OnOneSecondUpdateTicked;
            }
            
        }


        private void AddMissingPedestals()
        {
            Log.Trace("Adding pedestals");
            GameLocation location = Game1.getLocationFromName("Custom_Ridgeside_RidgeFalls");
            foreach (var pedestal in PedestalTemplates)
            {
                location.Objects.TryGetValue(pedestal.tilePosition, out SObject existing_object);
                if (existing_object != null && !(existing_object is ItemPedestal)){
                    location.Objects.Remove(pedestal.tilePosition);
                }else if(existing_object == null)
                {
                    location.Objects.Add(pedestal.tilePosition,
                                   new ItemPedestal(pedestal.tilePosition,
                                       new SObject(ExternalAPIs.JA.GetObjectId(pedestal.RequiredItemName), 1),
                                       false, Color.White));
                }
            }
            try
            {
                foreach (var pedestal in PedestalTemplates)
                {
                    ItemPedestal itemPedestal = (ItemPedestal)location.Objects[pedestal.tilePosition];
                    itemPedestal.requiredItem.Value = new SObject(ExternalAPIs.JA.GetObjectId(pedestal.RequiredItemName), 1);
                    itemPedestal.lockOnSuccess.Value = false;
                    itemPedestal.UpdateItemMatch();

                }
            }
            catch (Exception exception)
            {
                Log.Warn("Issue with pedestals detected in AddMissingPedestals. Check Trace for Details");
                Log.Trace(exception.Message);
                Log.Trace(exception.StackTrace);
            }
        }
    }

    public class PedestalTemplate
    {
        public Vector2 tilePosition;
        public string RequiredItemName;
        public PedestalTemplate(Vector2 tilePosition, string RequiredItemName)
        {
            this.tilePosition = tilePosition;
            this.RequiredItemName = RequiredItemName;
        }
    }
}
