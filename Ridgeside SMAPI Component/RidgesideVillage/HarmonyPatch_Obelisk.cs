using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.Buildings;
using StardewModdingAPI.Events;

namespace RidgesideVillage
{
    //Corrects the location name in the "X has begun in Y" message
    internal static class HarmonyPatch_Obelisk
    {
        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }

        private static string obeliskName = "RSV Obelisk";
        private static string obeliskReplacement = "Earth Obelisk";
        private static string obeliskKey = "RSVObelisk";

        internal static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            Helper = helper;



            Helper.Events.GameLoop.Saving += OnSaving;
            Helper.Events.GameLoop.Saved += OnSaved;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.Display.MenuChanged += OnMenuChanged;

            Log.Trace($"Applying Harmony Patch \"{nameof(obeliskWarpForReal_Prefix)}\" prefixing SDV method \"obeliskWarpForReal_Prefix\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(Building), "obeliskWarpForReal"),
                prefix: new HarmonyMethod(typeof(HarmonyPatch_Obelisk), nameof(obeliskWarpForReal_Prefix))
            );
        }



        internal static bool obeliskWarpForReal_Prefix(ref Building __instance)
        {
            try
            {
                switch ((string)__instance.buildingType.Value)
                {
                    case "RSV Obelisk":
                        Game1.warpFarmer("Custom_Ridgeside_RidgesideVillage", 114, 45, flip: false);
                        break;
                }
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(obeliskWarpForReal_Prefix)}\" has encountered an error. \n{e.ToString()}");
                return true;
            }
        }
        private static void OnSaved(object sender, SavedEventArgs e)
        {
            HarmonyPatch_Obelisk.RestoreObelisks();
        }

        private static void OnSaving(object sender, SavingEventArgs e)
        {
            HarmonyPatch_Obelisk.SanitizeObelisks();
        }

        private static void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            HarmonyPatch_Obelisk.RestoreObelisks();
        }

        //add the obelisk to the wizardMenu
        private static void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is StardewValley.Menus.CarpenterMenu Menu)
            {
                if (Helper.Reflection.GetField<bool>(Menu, "magicalConstruction").GetValue())
                {
                    var Blueprints = Helper.Reflection.GetField<List<BluePrint>>(Menu, "blueprints").GetValue();
                    Blueprints.Add(new BluePrint("RSV Obelisk"));
                }
            }
        }

        static internal void SanitizeObelisks()
        {
            if (!Game1.player.IsMainPlayer)
            {
                return;
            }

            //replace RSV Obelisk with Earth Obelisk
            var buildings = Game1.getFarm().buildings;
            foreach (var building in buildings)
            {
                if (building != null && building.buildingType.Value == obeliskName)
                {
                    building.buildingType.Value = obeliskReplacement;
                    //store in modData original type
                    if (!building.modData.ContainsKey(obeliskKey))
                    {
                        building.modData.Add(obeliskKey, obeliskName);
                    }
                }
            }
        }
        static internal void RestoreObelisks()
        {
            if (!Game1.player.IsMainPlayer)
            {
                return;
            }

            //replace RSV Obelisk with Earth Obelisk
            var buildings = Game1.getFarm().buildings;
            foreach(var building in buildings)
            {
                if(building != null && building.buildingType.Value == obeliskReplacement && building.modData.ContainsKey(obeliskKey))
                {
                    building.buildingType.Value = building.modData[obeliskKey];
                }
            }
        }

    }
}
