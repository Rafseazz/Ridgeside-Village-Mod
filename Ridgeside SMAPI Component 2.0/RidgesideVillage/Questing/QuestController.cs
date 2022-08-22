using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Quests;
using StardewModdingAPI.Utilities;
using Microsoft.Xna.Framework.Graphics;

namespace RidgesideVillage.Questing
{
    internal class QuestController
    {

		static IModHelper Helper;
		static IMonitor Monitor;
		//static Lazy<Texture2D> questionMarkSprite = new Lazy<Texture2D>(() => Helper.Content.Load<Texture2D>(PathUtilities.NormalizePath("assets/questMark.png"), ContentSource.ModFolder));

		//store available quest data for each user
		internal static readonly PerScreen<QuestData> dailyQuestData = new PerScreen<QuestData>();

		private static readonly PerScreen<LocationForMarkers> CurrentLocationFormarkers = new(() => LocationForMarkers.Other);

		//is board unlocked in the current map (if there is one)
		private static readonly PerScreen<bool> SOBoardUnlocked = new();
		private static readonly PerScreen<bool> QuestBoardUnlocked = new();
		private static readonly PerScreen<bool> OrdersGenerated = new();

		internal static readonly PerScreen<HashSet<int>> FinishedQuests = new(() => new());

		private enum LocationForMarkers
        {
			RSVVillage,
			NinjaHouse,
			Other
        }

		internal static void Initialize(IMod ModInstance)
		{
			Helper = ModInstance.Helper;
			Monitor = ModInstance.Monitor;
			TileActionHandler.RegisterTileAction("RSVQuestBoard", OpenQuestBoard);
			TileActionHandler.RegisterTileAction("RSVSpecialOrderBoard", OpenSOBoard);
			Helper.ConsoleCommands.Add("RSVrefresh", "", (s1, s2) => {

				RSVSpecialOrderBoard.UpdateAvailableRSVSpecialOrders(force_refresh: true);
				Log.Info("RSV Special Orders refreshed");
			});
			Helper.ConsoleCommands.Add("RSVQuestState", "", (s1, s2) => QuestController.PrintQuestState());
			Helper.ConsoleCommands.Add("RSVCheckQuests", "", (s1,s2) => QuestController.CheckQuests());
			Helper.Events.GameLoop.DayStarted += OnDayStarted;
			Helper.Events.Player.Warped += OnWarped;
			Helper.Events.Display.RenderedWorld += RenderQuestMarkersIfNeeded;
			Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
			Helper.Events.GameLoop.DayEnding += OnDayEnding;

			OrdersGenerated.Value = false;
		}

        private static void CheckQuests()
        {
			var questData = Helper.GameContent.Load<Dictionary<int, string>>(PathUtilities.NormalizeAssetName("Data/Quests"));
			foreach(var key in questData.Keys)
            {
                try
                {
					Log.Debug($"Checking {key}");
					Quest.getQuestFromId(key);

				}
                catch(Exception e)
                {
					Log.Error($"Failed for quest ID {key}. Stacktrace in Trace");
					Log.Trace(e.Message);
					Log.Trace(e.StackTrace);
                }
            }
        }

        static void PrintQuestState()
        {
			foreach(var data in dailyQuestData.GetActiveValues())
            {
				var questData = data.Value;
				Log.Debug($"TownQuest: {questData?.dailyTownQuest?.id}");
				Log.Debug($"Townquest accepted: {questData.acceptedDailyQuest}");
				Log.Debug($"NinjaQuest: {questData?.dailyNinjaHouseQuest?.id}");
				Log.Debug($"NinjaQuest accepted: {questData.acceptedDailyNinjaHouseQuest}");
				Log.Debug($"Quests done: {string.Join(",", FinishedQuests.Value)}");

			}
        }

		//save the players dailies
		private static void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            Game1.player.modData["RSVDailiesDone"] = string.Join(",", FinishedQuests.Value);
			if (OrdersGenerated.Value && Game1.dayOfMonth % 7 == 0 && Game1.player.IsMainPlayer)
            {
				OrdersGenerated.Value = false;
            }

		}

        //load the player's finished quests
        private static void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Game1.player.modData.TryGetValue("RSVDailiesDone", out string dailisDone))
			{

				Log.Trace($"dailies Done: {dailisDone}");
				FinishedQuests.Value = new HashSet<int>();
				if (!string.IsNullOrEmpty(dailisDone))
				{
					foreach (string id in dailisDone.Split(","))
					{
						FinishedQuests.Value.Add(int.Parse(id));
					}
				}                
            }

			//Game1.player.team.availableSpecialOrders.OnElementChanged += AvailableSpecialOrders_OnElementChanged;
		}

		/*
		//to log if other mods mess with it
		private static void AvailableSpecialOrders_OnElementChanged(Netcode.NetList<SpecialOrder, Netcode.NetRef<SpecialOrder>> list, int index, SpecialOrder oldValue, SpecialOrder newValue)
		{
			Log.Trace("Available SpecialOrders were changed");
			Log.Trace($"Current Ingame Date: {SDate.Now()} {Game1.timeOfDay}");
			Log.Trace($"{Environment.StackTrace}");
		}
		*/

		private static void OnWarped(object sender, WarpedEventArgs e)
        {
			if(e.Player == Game1.player)
            {
				if (!OrdersGenerated.Value && Game1.dayOfMonth % 7 == 1 && Game1.IsMasterGame &&
					(e.NewLocation.Name.Equals("Custom_Ridgeside_RidgesideVillage") || e.NewLocation.Name.Equals("Custom_Ridgeside_RSVNinjaHouse")))
				{
					RSVSpecialOrderBoard.UpdateAvailableRSVSpecialOrders(force_refresh: true);
				}

				if (e.NewLocation.Name.Equals("Custom_Ridgeside_RidgesideVillage"))
				{
					CurrentLocationFormarkers.Value = LocationForMarkers.RSVVillage;
					SOBoardUnlocked.Value = Game1.player.eventsSeen.Contains(75160207);

				}
				else if (e.NewLocation.Name.Equals("Custom_Ridgeside_RSVNinjaHouse"))
				{
					CurrentLocationFormarkers.Value = LocationForMarkers.NinjaHouse;
					SOBoardUnlocked.Value = Game1.player.eventsSeen.Contains(75160264);
					QuestBoardUnlocked.Value = Game1.player.eventsSeen.Contains(75160187);
				}
				else
				{
					CurrentLocationFormarkers.Value = LocationForMarkers.Other;
					SOBoardUnlocked.Value = false;
					QuestBoardUnlocked.Value = false;
				}
			}
           
        }

        private static void RenderQuestMarkersIfNeeded(object sender, RenderedWorldEventArgs e)
        {
			SpriteBatch sb = e.SpriteBatch;
            switch (CurrentLocationFormarkers.Value)
            {
				case LocationForMarkers.RSVVillage:
					float offset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
					if (!dailyQuestData.Value.acceptedDailyQuest)
					{
						sb.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(109f * 64f + 32f, 38.5f * 64f + offset)),
							new Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - offset / 16f), SpriteEffects.None, 1f);

					}
					if (SOBoardUnlocked.Value && !Game1.player.team.acceptedSpecialOrderTypes.Contains("RSVTownSO"))
					{
						Vector2 questMarkPosition = new Vector2(119f * 64f + 27f, 39f * 64f);
						sb.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(119f * 64f + 32f, 39.5f * 64f + offset)),
							new Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - offset / 16f), SpriteEffects.None, 1f);
					}
					
					break;
				case LocationForMarkers.NinjaHouse:
					offset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
					if (SOBoardUnlocked.Value && !dailyQuestData.Value.acceptedDailyNinjaHouseQuest)
					{
						sb.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(6f * 64f + 32f, 5.5f * 64f + offset)),
							new Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - offset / 16f), SpriteEffects.None, 1f);

					}
					if (!Game1.player.team.acceptedSpecialOrderTypes.Contains("RSVNinjaSO"))
					{
						sb.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3f * 64f + 32f, 3f * 64f + offset)),
							new Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - offset / 16f), SpriteEffects.None, 1f);
					}
					break;
			}
			return;
		}
        private static void OpenQuestBoard(string name, Vector2 position)
		{
			string type = name.Split()[^1];
			Log.Trace($"Opening RSVQuestBoard {type}");
			Log.Trace(dailyQuestData.ToString());
			Game1.activeClickableMenu = new RSVQuestBoard(dailyQuestData.Value, type);
		}


		private static void OpenSOBoard(string name, Vector2 position)
		{
			string type = name.Split()[^1];
			Log.Trace($"Opening RSVSOBoard {type}");
			Game1.activeClickableMenu = new RSVSpecialOrderBoard(type);
		}


		private static void OnDayStarted(object sender, DayStartedEventArgs e)
		{
			try
            {
				Log.Trace($"Player has done following quests: {String.Join(",", FinishedQuests.Value)}");
				Quest townQuest = QuestFactory.GetDailyQuest();
				Quest ninjaQuest = QuestFactory.GetDailyNinjaQuest();
				dailyQuestData.Value = new QuestData(townQuest, ninjaQuest);
			}
            catch
            {
				dailyQuestData.Value = new QuestData(null, null);
				Log.Error("Failed parsing quests.");
			}
		}
	}


	internal class QuestData
	{
		internal Quest dailyTownQuest;
		internal bool acceptedDailyQuest;
		internal Quest dailyNinjaHouseQuest;
		internal bool acceptedDailyNinjaHouseQuest;

		internal QuestData(Quest dailyTownQuest, Quest dailyNinjaHouseQuest)
		{
			this.dailyTownQuest = dailyTownQuest;
			this.acceptedDailyQuest = dailyTownQuest is null;
			this.dailyNinjaHouseQuest = dailyNinjaHouseQuest;
			this.acceptedDailyNinjaHouseQuest = dailyNinjaHouseQuest is null;
		}

        public override string ToString()
        {
			return $"Quest data: Town ID {dailyTownQuest?.id} {acceptedDailyQuest}, NinjaHouse ID {dailyNinjaHouseQuest?.id} {acceptedDailyQuest}";
        }
    }
}

