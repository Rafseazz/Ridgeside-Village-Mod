using HarmonyLib;
using RidgesideVillage.Patches;
using StardewModdingAPI;

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

            Animations.ApplyPatch(harmony, Helper);
            Backgrounds.ApplyPatch(harmony, Helper);
            Dateables.ApplyPatch(harmony, Helper, Manifest);
            EventDetection.ApplyPatch(harmony, Helper);
            EventPatches.ApplyPatch(harmony, Helper);
            Projectiles.ApplyPatch(harmony, Helper);
            Rings.ApplyPatch(harmony, Helper);
            SecretSantaGift.ApplyPatch(harmony, Helper);
            SODialogue.ApplyPatch(harmony, Helper);
            SummitFarm.ApplyPatch(harmony, Helper);
            SwimPatch.ApplyPatch(harmony, Helper);
            TortsGifts.ApplyPatch(harmony, Helper);
            TreasureItems.ApplyPatch(harmony, Helper);
            UntimedSO.ApplyPatch(harmony, Helper);
            WalletItem.ApplyPatch(harmony, Helper);
            QuestPatches.ApplyPatch(harmony, Helper);
            Music.ApplyPatch(harmony, Helper);
            SummitHouse.ApplyPatch(harmony, Helper);

            //RidgeForest.ApplyPatch(harmony, Helper);
        }
    }
}
