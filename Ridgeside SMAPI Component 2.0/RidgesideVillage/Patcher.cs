using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RidgesideVillage
{
    class Patcher
    {
        static IModHelper Helper;
        static IManifest Manifest;

        public Patcher(IMod mod)
        {
            Helper = mod.Helper;
            Manifest = mod.ModManifest;
        }

        public void PerformPatching()
        {
            var harmony = new Harmony(Manifest.UniqueID);

            HarmonyPatch_EventMessage.ApplyPatch(harmony, Helper);
            HarmonyPatch_Obelisk.ApplyPatch(harmony, Helper);
            HarmonyPatch_UntimedSO.ApplyPatch(harmony, Helper);
            HarmonyPatch_EventDetection.ApplyPatch(harmony, Helper);
            HarmonyPatch_Fish.ApplyPatch(harmony, Helper);
            HarmonyPatch_TreasureItems.ApplyPatch(harmony, Helper);
            HarmonyPatch_SummitFarm.ApplyPatch(harmony, Helper);
            HarmonyPatch_WeddingGuests.ApplyPatch(harmony, Helper);
            HarmonyPatch_Animations.ApplyPatch(harmony, Helper);
            HarmonyPatch_SecretSantaGift.ApplyPatch(harmony, Helper);
            HarmonyPatch_Rings.ApplyPatch(harmony, Helper);
            HarmonyPatch_Dateables.ApplyPatch(harmony, Helper);
            HarmonyPatch_WalletItem.ApplyPatch(harmony, Helper);

        }
    }        
}
