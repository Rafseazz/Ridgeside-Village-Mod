using System.Collections.Generic;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{
    internal static class AssetManager
    {
        private static readonly string CUSTOM_EVENTS = PathUtilities.NormalizeAssetName("Data/Events/Custom_Ridgeside");
        internal static void LoadEmptyJsons(AssetRequestedEventArgs e)
        {
            // Load in files for events
#warning - fix in Stardew 1.6 for string event IDs.
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/AdventureGuild")
                || e.NameWithoutLocale.BaseName.StartsWith(CUSTOM_EVENTS))
            {
                e.LoadFrom(() => new Dictionary<int, string>(), AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/CustomWeddingGuestPositions"))
            { // Zero clue what this is, is this fully implemented?
                e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Low);
            }
        }
    }
}
