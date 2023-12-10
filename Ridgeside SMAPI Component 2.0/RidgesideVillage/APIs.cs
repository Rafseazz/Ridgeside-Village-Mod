using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace RidgesideVillage
{
    internal class ExternalAPIs
    {
        public static IContentPatcherApi CP;
        //public static IJsonAssetsApi JA;
        public static ISpaceCoreApi SC;

        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }

        internal static void Initialize(IModHelper helper)
        {
            Helper = helper;

            CP = Helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");
            if (CP is null)
            {
                Log.Alert("Content Patcher is not installed - RSV requires CP to run. Please install CP and restart your game.");
                return;
            }

            SC = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            if (SC == null)
            {
                Log.Warn("SpaceCore API not found. This could lead to issues.");
            }

        }
    }
}
