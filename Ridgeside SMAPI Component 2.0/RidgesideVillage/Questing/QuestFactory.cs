using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;

using SObject = StardewValley.Object;
namespace RidgesideVillage.Questing
{

	static internal class QuestFactory
	{
		static internal Quest GetDailyQuest()
		{
			if (Game1.random.NextDouble() < 0.2)
			{
				return null;
			}
			if (Game1.random.NextDouble() > 0.2)
			{
				return GetRandomHandCraftedQuest();
            }
            else
            {
				return GetFishingQuest();
            }
		}

		static internal Quest GetDailyNinjaQuest()
        {
			//80% chance for no ninja questquest
			if(Game1.random.NextDouble() > 0.2)
            {
				return null;
            }
			var quests = ModEntry.Helper.GameContent.Load<Dictionary<int, string>>(StardewModdingAPI.Utilities.PathUtilities.NormalizeAssetName("data/quests"));
			var candidates = new List<int>();
			foreach (var key in quests.Keys)
			{
				// is ninjaquest and not completed yet
				if (key >= 72860500 && key <= 72860999 && !QuestController.FinishedQuests.Value.Contains(key))
				{
					if (!Game1.player.hasQuest(key))
					{
						candidates.Add(key);
					}
				}
			}
			Log.Trace($"NinjaQuest candidate count: {candidates.Count}");
			if(candidates.Count == 0)
            {
				return null;
            }
			int rand = Game1.random.Next(candidates.Count);
			Log.Trace($"Daily Ninjaquest: ID {candidates[rand]}");
			return QuestFactory.getQuestFromId(candidates[rand]);
		}

		static internal Quest GetRandomHandCraftedQuest()
        {
			var quests = ModEntry.Helper.GameContent.Load<Dictionary<int, string>>(StardewModdingAPI.Utilities.PathUtilities.NormalizeAssetName("data/quests"));
			var candidates = new List<int>();
			foreach(var key in quests.Keys)
            {
				if (key >= 72861000 && key <= 72862000)
                {
                    if (!Game1.player.hasQuest(key))
					{
						candidates.Add(key);
					}
                }
			}
			Log.Trace($"{candidates.Count} candidates for daily Quest");

			if(candidates.Count == 0)
            {
				return null;
            }

			int rand = Game1.random.Next(candidates.Count);

			Log.Trace($"chose {candidates[rand]}");
			return QuestFactory.getQuestFromId(candidates[rand]);
        }


		static internal Quest GetFishingQuest()
		{
			FishingQuest quest = new FishingQuest();
			quest.loadQuestInfo();

			string[] possibleFish;
			switch (Game1.currentSeason)
			{
				case "spring":
					{
						possibleFish = new string[] { "Cutthroat Trout", "Ridgeside Bass", "Ridge Bluegill", "Caped Tree Frog", "Pebble Back Crab", "Harvester Trout", "Mountain Redbelly Dace", "Mountain Whitefish" };
						int[] possiblefish2 = new int[8] { 129, 131, 136, 137, 142, 143, 145, 147 };
						quest.whichFish.Value = possiblefish2[Game1.random.Next(possiblefish2.Length)];
						break;
					}
				case "summer":
					{
						possibleFish = new string[] { "Cutthroat Trout", "Ridgeside Bass", "Caped Tree Frog", "Pebble Back Crab", "Skulpin Fish", "Mountain Redbelly Dace", "Mountain Whitefish" };
						break;
					}
				case "fall":
					{
						possibleFish = new string[] { "Cutthroat Trout", "Ridgeside Bass", "Ridge Bluegill", "Caped Tree Frog", "Pebble Back Crab", "Skulpin Fish", "Harvester Trout", "Mountain Redbelly Dace", "Mountain Whitefish" };
						break;
					}
				case "winter":
					{
						possibleFish = new string[] { "Ridgeside Bass", "Ridge Bluegill", "Skulpin Fish", "Harvester Trout", "Mountain Redbelly Dace", "Mountain Whitefish" };
						break;
					}
				default:
					{
						possibleFish = new string[] { "Bream" };
						break;
					}
			}
			string chosenFish = possibleFish[Game1.random.Next(possibleFish.Length)];
			quest.whichFish.Value = ExternalAPIs.JA.GetObjectId(chosenFish);
			if(quest.whichFish.Value == -1)
            {
				quest.whichFish.Value = 132; //Bream as fallback

			}

			quest.fish.Value = new SObject(Vector2.Zero, quest.whichFish.Value, 1);
			quest.numberToFish.Value = (int)Math.Ceiling(200.0 / (double)Math.Max(1, quest.fish.Value.Price)) + Game1.player.FishingLevel / 5;
			quest.reward.Value = (int)(quest.numberToFish.Value + 1.5) * quest.fish.Value.Price;
			quest.target.Value = "Carmen";
			quest.parts.Clear();
			//have to patch the CSFiles for this... ugh
			//its in the CP part, data/Quests/StringsForQuests
			quest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:Carmen.FishingQuest.Description", quest.fish.Value, quest.numberToFish.Value)); //actual quest text
			quest.dialogueparts.Clear();
			quest.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:Carmen.FishingQuest.HandInDialogue", quest.fish.Value));
			quest.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", 0, quest.numberToFish.Value, quest.fish.Value); // progress

			quest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13274", quest.reward.Value)); // reward
			quest.parts.Add("Strings\\StringsFromCSFiles:FishingQuest.cs.13275"); //keep fish note
			quest.daysLeft.Value = 7;
			quest.id.Value = 80000000;
			return quest;

		}
		static internal Quest getQuestFromId(int id)
		{
			Quest quest = Quest.getQuestFromId(id);

			if (quest is SlayMonsterQuest monsterQuest)
			{
				Dictionary<int, string> questData = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");

				if (questData != null && questData.ContainsKey(id))
				{
					string[] rawData = questData[id].Split('/');
					string questType = rawData[0];
					string[] conditionsSplit = rawData[4].Split(' ');

					monsterQuest.loadQuestInfo();
					monsterQuest.monster.Value.Name = conditionsSplit[0].Replace('_', ' ');
					monsterQuest.monsterName.Value = monsterQuest.monster.Value.Name;
					monsterQuest.numberToKill.Value = Convert.ToInt32(conditionsSplit[1]);
					if (conditionsSplit.Length > 2)
					{
						monsterQuest.target.Value = conditionsSplit[2];
					}
					else
					{
						monsterQuest.target.Value = "null";
					}
					monsterQuest.questType.Value = 4;
                    if (rawData.Length>9)
                    {
						monsterQuest.targetMessage = rawData[9];
                    }

					monsterQuest.moneyReward.Value = Convert.ToInt32(rawData[6]);
					monsterQuest.reward.Value = monsterQuest.moneyReward.Value;
					monsterQuest.rewardDescription.Value = (rawData[6].Equals("-1") ? null : rawData[7]);
					monsterQuest.parts.Clear();
					monsterQuest.dialogueparts.Clear();
					ModEntry.Helper.Reflection.GetField<bool>(monsterQuest, "_loadedDescription").SetValue(true);
					return monsterQuest;
				}
			}

			return quest;
		}
	}

}
