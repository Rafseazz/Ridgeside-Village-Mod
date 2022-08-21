using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.GameData;

namespace RidgesideVillage.Questing
{
    internal class RSVSpecialOrderBoard : SpecialOrdersBoard
    {

		public const string NINJABOARDNAME = "RSVNinjaSO";
		public const string RSVBOARDNAME = "RSVTownSO";
		int timestampOpened;
		static int safetyTimer = 500;



		internal RSVSpecialOrderBoard(string boardType = "") : base(boardType)
		{
			timestampOpened = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
			Texture2D billboardTexture;
            if (boardType.Equals(NINJABOARDNAME)){
				billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\RSVNinjaSOBoard");

			}else if (boardType.Equals(RSVBOARDNAME)){
				billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\RSVTownSO");
            }
            else
            {
				billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\SpecialOrdersBoard");
			}
			//Helper.Reflection.GetField<Texture2D>(this, "billboardTexture").SetValue(billboardTexture); //throws NRE; check later

		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (this.timestampOpened + safetyTimer < Game1.currentGameTime.TotalGameTime.TotalMilliseconds)
			{
				base.receiveRightClick(x, y, playSound);
			}
			return;
		}


		//need this public for DailySpecialOrders :p
		//mostly copied from the decompile
		//randomly chooses 2 SOs for each RSV board
		public static void UpdateAvailableRSVSpecialOrders(string type)
		{
			if (Game1.player.team.availableSpecialOrders is not null)
			{
				foreach (SpecialOrder order in Game1.player.team.availableSpecialOrders)
				{
					if ((order.questDuration.Value == SpecialOrder.QuestDuration.TwoDays || order.questDuration.Value == SpecialOrder.QuestDuration.ThreeDays)) // && !Game1.player.team.acceptedSpecialOrderTypes.Contains(order.orderType.Value)
					{
						order.SetDuration(order.questDuration.Value);
					}
				}
			}
			/*
			if (!force_refresh)
			{
				return;
			}
			*/

			Log.Trace("Refreshing RSV Special Orders");
			Dictionary<string, SpecialOrderData> order_data = Game1.content.Load<Dictionary<string, SpecialOrderData>>("Data\\SpecialOrders");
			List<string> keys = order_data.Keys.ToList();
			List<string> valid_SOs = new List<string> { };
			Log.Trace($"RSV: Number of keys: {keys.Count}");

			for (int n = 0; n < keys.Count; n++)
			{
				string key = keys[n];
				Log.Trace($"Checking {key}");
				bool invalid = false;
				bool repeatable = order_data[key].Repeatable.Equals("True", StringComparison.OrdinalIgnoreCase);
				if (order_data[key].OrderType != type)
				{
					Log.Trace($"Not the correct type");
					invalid = true;
				}
				else if (repeatable && Game1.MasterPlayer.team.completedSpecialOrders.ContainsKey(key))
				{
					Log.Trace($"Not repeatable and already done");
					invalid = true;
				}
				else if (Game1.dayOfMonth >= 16 && order_data[key].Duration == "Month")
				{
					Log.Trace($"Month SO and after 16th");
					invalid = true;
				}
				else if (!invalid && !SpecialOrder.CheckTags(order_data[key].RequiredTags))
				{
					Log.Trace($"Tags conditions not met.");
					invalid = true;
				}
				if (!invalid)
				{
					foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
					{
						if ((string)specialOrder.questKey.Value == key)
						{
							Log.Trace($"Order currently active.");
							invalid = true;
							break;
						}
					}
					valid_SOs.Add(key);
				}
				Log.Trace($"Order {keys[n]} is valid: {!invalid}");
				/*
				if (invalid)
				{
					valid_SOs.RemoveAt(n);
					n--;
				}
				*/
			}
			Log.Trace($"RSV: {valid_SOs.Count} special orders in this type");
			Random r = new Random((int)Game1.uniqueIDForThisGame + (int)(Game1.stats.DaysPlayed * 1.3f));
			for (int i = 0; i < Math.Min(2, valid_SOs.Count); i++)
			{
				int index = r.Next(valid_SOs.Count);
				Log.Trace($"RSV: random index is {index}");
				SpecialOrder order = SpecialOrder.GetSpecialOrder(valid_SOs[index], r.Next());
				Game1.player.team.availableSpecialOrders.Add(order);
				Log.Trace($"RSV: Added order {order.questName.Value}");
			}
			/*
			string[] array = new string[2] { NINJABOARDNAME, RSVBOARDNAME };
			foreach (string type_to_find in array)
			{
				List<string> typed_keys = new List<string>();
				foreach (string key3 in valid_SOs)
				{
					if (order_data[key3].OrderType == type_to_find)
					{
						typed_keys.Add(key3);
					}
				}
				List<string> all_keys = new List<string>(typed_keys);

				for (int j = 0; j < typed_keys.Count; j++)
				{
					if (Game1.player.team.completedSpecialOrders.ContainsKey(typed_keys[j]))
					{
						typed_keys.RemoveAt(j);
						j--;
					}
				}

				for (int i = 0; i < 2; i++)
				{
					if (typed_keys.Count == 0)
					{
						if (all_keys.Count == 0)
						{
							break;
						}
						typed_keys = new List<string>(all_keys);
					}
					int index = r.Next(typed_keys.Count);
					string key2 = typed_keys[index];
					Game1.player.team.availableSpecialOrders.Add(SpecialOrder.GetSpecialOrder(key2, r.Next()));
					typed_keys.Remove(key2);
					all_keys.Remove(key2);
				}
			}
			*/
			Log.Trace("Refreshed RSV SpecialOrders");
			foreach (var SO in Game1.player.team.availableSpecialOrders)
			{
				Log.Trace($"{SO.questKey.Value}, {SO.orderType.Value}");
			}

		}

	}
}
