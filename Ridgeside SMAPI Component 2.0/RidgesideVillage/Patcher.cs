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

            EventMessage.ApplyPatch(harmony, Helper);
            Obelisk.ApplyPatch(harmony, Helper);
            UntimedSO.ApplyPatch(harmony, Helper);
            EventDetection.ApplyPatch(harmony, Helper);
            Fish.ApplyPatch(harmony, Helper);
            TreasureItems.ApplyPatch(harmony, Helper);
            SummitFarm.ApplyPatch(harmony, Helper);
            WeddingGuests.ApplyPatch(harmony, Helper);
            Animations.ApplyPatch(harmony, Helper);
            SecretSantaGift.ApplyPatch(harmony, Helper);
            Rings.ApplyPatch(harmony, Helper);
            Dateables.ApplyPatch(harmony, Helper);
            WalletItem.ApplyPatch(harmony, Helper);
            SODialogue.ApplyPatch(harmony, Helper);
            Backgrounds.ApplyPatch(harmony, Helper);
            Projectiles.ApplyPatch(harmony, Helper);

        }
    }        
}
