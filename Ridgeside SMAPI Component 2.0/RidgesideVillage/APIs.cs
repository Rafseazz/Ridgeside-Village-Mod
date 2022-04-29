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

        public static ICustomCompanionsApi CC;
        public static IContentPatcherApi CP;
        public static IJsonAssetsApi JA;
        public static IWearMoreRingsApi MR;
        public static ISpaceCoreApi SC;
        public static IQuestFrameworkApi QF;

        private static IMonitor Monitor { get; set; }
        private static IModHelper Helper { get; set; }

        internal static void Initialize(IModHelper helper)
        {
            Helper = helper;

            CC = Helper.ModRegistry.GetApi<ICustomCompanionsApi>("PeacefulEnd.CustomCompanions");
            if (CC == null)
            {
                Log.Warn("Custom Companions API not found. This could lead to issues.");
            }

            CP = Helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");
            if (CP is null)
            {
                Log.Alert("Content Patcher is not installed - RSV requires CP to run. Please install CP and restart your game.");
                return;
            }

            JA = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if(JA == null)
            {
                Log.Warn("Json Assets API not found. This could lead to issues.");
            }

            MR = Helper.ModRegistry.GetApi<IWearMoreRingsApi>("bcmpinc.WearMoreRings");
            if (MR == null)
            {
                Log.Trace("Wear More Rings API not found. Using base game ring slots only.");
            }

            SC = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            if (SC == null)
            {
                Log.Warn("SpaceCore API not found. This could lead to issues.");
            }

            QF = Helper.ModRegistry.GetApi<IQuestFrameworkApi>("purrplingcat.QuestFramework");
            if (QF == null)
            {
                Log.Warn("Quest Framework API not found. This could lead to issues.");
            }
        }
    }
}
