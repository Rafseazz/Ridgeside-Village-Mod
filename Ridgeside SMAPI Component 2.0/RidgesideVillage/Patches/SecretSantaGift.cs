using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{

    internal static class SecretSantaGift
    {
        private static IModHelper Helper { get; set; }
        private static readonly string AssetPath = PathUtilities.NormalizeAssetName("RSV/RSVSecretSantaGifts");


        public static void ApplyPatch(Harmony harmony, IModHelper helper)
        {
            SecretSantaGift.Helper = helper;

            Helper.ConsoleCommands.Add("RSV_GiftTest", "", SecretSantaGift.GiveGift);
            Log.Trace($"Applying Harmony Patch \"{nameof(SecretSantaGift)}.");
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getGiftFromNPC)),
                prefix: new HarmonyMethod(typeof(SecretSantaGift), nameof(Utility_getGiftFromNPC_Prefix))
            );

        }

        public static void GiveGift(string arg1, string[] arg2)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }
            var npc = Game1.getCharacterFromName(arg2[0]);
            if (npc == null){
                Log.Error($"Character {arg2[0]} not found");
                return;
            }
            Item item = Utility.getGiftFromNPC(npc);
            Game1.player.addItemByMenuIfNecessary(item);
        }

        public static bool Utility_getGiftFromNPC_Prefix(NPC who, ref Item __result)
        {
            try
            {
                Log.Trace($"Choosing gift from {who.Name}");
                Random r = new Random((int)Game1.uniqueIDForThisGame / 2 + Game1.year + Game1.dayOfMonth + Utility.getSeasonNumber(Game1.currentSeason) + (int)who.TilePoint.X);

                var data = Helper.GameContent.Load<Dictionary<string, Dictionary<string, ItemEntry>>>(AssetPath);
                if (data.TryGetValue(who.Name, out Dictionary<string, ItemEntry> possibleGifts) && possibleGifts.Count > 0)
                {

                    Log.Trace($"Found gifts from {who.Name}");
                    string itemName = possibleGifts.Keys.ElementAt(r.Next(possibleGifts.Count));
                    ItemEntry itemData = possibleGifts[itemName];
                    Item giftItem = Utility.fuzzyItemSearch(itemName, stack_count:itemData.amount);
                    if(giftItem != null && giftItem.ParentSheetIndex >= 0)
                    {
                        __result = giftItem;

                        Log.Trace($"Found gift from {who.Name}: {giftItem.Name}");
                        return false;
                    }
                    //something went wrong
                }
                
                return true;
                
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading secret santa gift for RSV NPC {who.Name}: {ex}\n{ex.Message}\n{ex.StackTrace}");
                return true;
            }
        }

    }

    public class ItemEntry
    {
        public int ID { get; set; }
        public int amount { get; set; }
        public ItemEntry(int id, int amount)
        {
            this.ID = id;
            this.amount = amount;
        }
    }
}
