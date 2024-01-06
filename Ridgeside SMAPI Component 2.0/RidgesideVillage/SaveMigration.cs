using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RidgesideVillage
{
    internal class SaveMigration
    {
        Dictionary<String, String> lookup;
        private const String RSV_MIGRATED = "RSV_Migrated";
        public SaveMigration(IModHelper helper) {
          
        lookup = helper.ModContent.Load<Dictionary<String, String>>("assets/migration.json");
        helper.Events.GameLoop.SaveLoaded += run;
            
        }

        public void run(object sender, SaveLoadedEventArgs e)
        {
            if (!Game1.player.IsMainPlayer)
            {
                return;
            }

            Farm farm = Game1.getFarm();
            if(farm.modData.ContainsKey(RSV_MIGRATED))
            {
                return;
            }

            Utility.ForEachItem((item, remove, replaceWith) =>
            {
                if (lookup.TryGetValue(item.Name, out String newID))
                {
                    Item newItem = ItemRegistry.Create(newID, item.Stack, item.Quality);
                    replaceWith(newItem);
                }
                else
                {
                    String alternativeID = item.QualifiedItemId.Substring(3).Replace("_s", "'s").Replace("_", " ");
                    if(lookup.TryGetValue(alternativeID, out String newID2))
                    {
                        Item newItem = ItemRegistry.Create(newID2, item.Stack, item.Quality);
                        replaceWith(newItem);
                    }
                    else if (item.Category == -100)
                    {
                        String genderedID = alternativeID + (Game1.player.GetGender() == NPC.male ? " M" : " F");
                        if (lookup.TryGetValue(genderedID, out String newID3))
                        {
                            Item newItem = ItemRegistry.Create(newID3, item.Stack, item.Quality);
                            replaceWith(newItem);
                        }
                    }
                }
                return true;
            });


            for(int i = 0; i<farm.buildings.Count; i++)
            {
                var building = farm.buildings[i];
                if (building.modData.ContainsKey("RSVObelisk"))
                {
                    var newObelisk = new Building("RSV_Obelisk", new Microsoft.Xna.Framework.Vector2(building.tileX.Value, building.tileY.Value));
                    farm.buildings[i] = newObelisk;
                }
            }

            farm.modData.Add(RSV_MIGRATED, RSV_MIGRATED);
        }

    }
}
