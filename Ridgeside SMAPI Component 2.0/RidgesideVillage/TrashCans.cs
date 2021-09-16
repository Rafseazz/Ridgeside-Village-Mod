using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RidgesideVillage
{
    internal class TrashCans
    {

        static IModHelper Helper;

        static List<Tuple<int, float>> spawnProbabilities;
        static HashSet<Vector2> TrashCansTriggeredToday = new HashSet<Vector2>();
        internal static void Setup(IModHelper Helper)
        {
            TrashCans.Helper = Helper;
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            TileActionHandler.RegisterTileAction("RSV.TrashCan", Trigger);
        }

        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            TrashCansTriggeredToday.Clear();
            
        }

        internal static void Trigger(string tileAction, Vector2 position)
        {
            if (TrashCansTriggeredToday.Contains(position))
            {
                return;
            }
            TrashCansTriggeredToday.Add(position);
            GameLocation location = Game1.currentLocation;
            Vector2 itemSpawnPosition = position * 64f;
            itemSpawnPosition.Y -= 64f;
            itemSpawnPosition.X += 32f;
            Game1.stats.incrementStat("trashCansChecked", 1);

            //vanilla code. draws the animation
            List<TemporaryAnimatedSprite> trashCanSprites = new List<TemporaryAnimatedSprite>();
            bool doubleMega = false;
            bool mega = false;
            int xSourceOffset = Utility.getSeasonNumber(Game1.currentSeason) * 17;

            xTile.Dimensions.Location tileLocation = new xTile.Dimensions.Location((int) position.X, (int) position.Y);
            trashCanSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(22 + xSourceOffset, 0, 16, 10), new Vector2(tileLocation.X, tileLocation.Y) * 64f + new Vector2(0f, -6f) * 4f, flipped: false, 0f, Color.White)
            {
                interval = (doubleMega ? 4000 : 1000),
                motion = (doubleMega ? new Vector2(4f, -20f) : new Vector2(0f, -8f + (mega ? (-7f) : ((float)(Game1.random.Next(-1, 3) + ((Game1.random.NextDouble() < 0.1) ? (-2) : 0)))))),
                rotationChange = (doubleMega ? 0.4f : 0f),
                acceleration = new Vector2(0f, 0.7f),
                yStopCoordinate = tileLocation.Y * 64 + -24,
                layerDepth = (doubleMega ? 1f : ((float)((tileLocation.Y + 1) * 64 + 2) / 10000f)),
                scale = 4f,
                Parent = location,
                shakeIntensity = (doubleMega ? 0f : 1f),
                reachedStopCoordinate = delegate
                {
                    location.removeTemporarySpritesWithID(97654);
                    location.playSound("thudStep");
                    for (int l = 0; l < 3; l++)
                    {
                        location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(tileLocation.X, tileLocation.Y) * 64f + new Vector2(l * 6, -3 + Game1.random.Next(3)) * 4f, flipped: false, 0.02f, Color.DimGray)
                        {
                            alpha = 0.85f,
                            motion = new Vector2(-0.6f + (float)l * 0.3f, -1f),
                            acceleration = new Vector2(0.002f, 0f),
                            interval = 99999f,
                            layerDepth = (float)((tileLocation.Y + 1) * 64 + 3) / 10000f,
                            scale = 3f,
                            scaleChange = 0.02f,
                            rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
                            delayBeforeAnimationStart = 50
                        });
                    }
                },
                id = 97654f
            });
            trashCanSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(22 + xSourceOffset, 11, 16, 16), new Vector2(tileLocation.X, tileLocation.Y) * 64f + new Vector2(0f, -5f) * 4f, flipped: false, 0f, Color.White)
            {
                interval = (doubleMega ? 999999 : 1000),
                layerDepth = (float)((tileLocation.Y + 1) * 64 + 1) / 10000f,
                scale = 4f,
                id = 97654f
            });
            for (int i = 0; i < 5; i++)
            {
                trashCanSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(22 + Game1.random.Next(4) * 4, 32, 4, 4), new Vector2(tileLocation.X, tileLocation.Y) * 64f + new Vector2(Game1.random.Next(13), -3 + Game1.random.Next(3)) * 4f, flipped: false, 0f, Color.White)
                {
                    interval = 500f,
                    motion = new Vector2(Game1.random.Next(-2, 3), -5f),
                    acceleration = new Vector2(0f, 0.4f),
                    layerDepth = (float)((tileLocation.Y + 1) * 64 + 3) / 10000f,
                    scale = 4f,
                    color = Utility.getRandomRainbowColor(),
                    delayBeforeAnimationStart = Game1.random.Next(100)
                });
            }
            var multiplayer = Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            multiplayer.broadcastSprites(location, trashCanSprites);
            location.playSound("trashcan");
            //copied code done 

            var tileActionSplit = tileAction.Split(' ');
            string lootTableID = "-1";
            if (tileActionSplit.Length >= 2)
            {
                lootTableID = tileActionSplit[1].Trim();
            }
            var trashData = Game1.content.Load<Dictionary<string, string>>(PathUtilities.NormalizeAssetName("RSV/TrashCanData"));
            //check if key or fallback key exist
            if (!trashData.ContainsKey(lootTableID))
            {
                lootTableID = "¨-1";
                if (!trashData.ContainsKey("-1"))
                {
                    return;
                }
            }
            var data = trashData[lootTableID];
            var dataSplit = data.Split('/');
            List<TrashDataItem> trashItems = new List<TrashDataItem>();
            int totalWeight = 0;
            foreach(var entry in dataSplit)
            {
                var entrySplit = entry.Split(' ');
                if(entrySplit.Length != 3)
                {
                    continue;
                }
                int weight = int.Parse(entrySplit[0]);
                totalWeight += weight;
                trashItems.Add(new TrashDataItem(weight, int.Parse(entrySplit[1]), int.Parse(entrySplit[2])));
            }

            if(trashItems.Count == 0)
            {
                Log.Debug("No TrashItems found");
                return;
            }
            int weightChosen = Game1.random.Next(0, totalWeight);
            TrashDataItem chosenItem = new TrashDataItem(0, -1, 0);
            foreach(var trashItem in trashItems)
            {
                weightChosen -= trashItem.Weight;
                if(weightChosen < 0)
                {
                    chosenItem = trashItem;
                    break;
                }
            }
            if(chosenItem.ObjectID != -1)
            {
                for (int i = 0; i < chosenItem.Amount; i++)
                {
                    Game1.createItemDebris(new StardewValley.Object(chosenItem.ObjectID, 1), itemSpawnPosition, 2, location, (int)itemSpawnPosition.Y + 64);
                }
            }
            
        }
    }
    struct TrashDataItem
    {
        internal int Weight;
        internal int ObjectID;
        internal int Amount;
        public TrashDataItem(int weight, int objectID, int amount)
        {
            this.Weight = weight;
            this.ObjectID = objectID;
            this.Amount = amount;
        }
    }
}
