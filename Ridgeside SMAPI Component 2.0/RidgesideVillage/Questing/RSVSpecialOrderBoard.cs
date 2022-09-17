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
		static IModHelper Helper;

		int timestampOpened;
		static int safetyTimer = 500;


		internal RSVSpecialOrderBoard(string boardType = "") : base(boardType)
		{
			Helper = ModEntry.Helper;
			LogCurrentlyAvailableSpecialOrders();

			timestampOpened = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
			Texture2D texture;
            if (boardType.Equals(RSVConstants.Z_NINJASPECIALORDERBOARD)){
				texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\RSVNinjaSOBoard");

			}else if (boardType.Equals(RSVConstants.Z_VILLAGESPECIALORDERBOARD)){
				texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\RSVTownSO");
            }
            else
            {
				texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\SpecialOrdersBoard");
			}
			Helper.Reflection.GetField<Texture2D>(this, "billboardTexture").SetValue(texture);

		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (timestampOpened + safetyTimer < Game1.currentGameTime.TotalGameTime.TotalMilliseconds)
			{
				base.receiveRightClick(x, y, playSound);
			}
			return;
		}

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
			if (leftOrder is null)
            {
				b.DrawString(Game1.dialogueFont, Helper.Translation.Get("RSV.NoNewOrders"), new Vector2(xPositionOnScreen + 125, yPositionOnScreen + 375), Game1.textColor);
			}
			if (rightOrder is null)
            {
				int indent = (boardType == RSVConstants.Z_NINJASPECIALORDERBOARD) ? 800 : 775;
				b.DrawString(Game1.dialogueFont, Helper.Translation.Get("RSV.NoNewOrders"), new Vector2(xPositionOnScreen + indent, yPositionOnScreen + 375), Game1.textColor);
			}
		}

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
			bool hadQuestBefore = false;
			foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
			{
				if (specialOrder.questKey.Value == RSVConstants.SO_PIKAQUEST)
				{
					hadQuestBefore = true;
				}
			}
			Log.Trace($"RSV: Had order before is {hadQuestBefore}");
			base.receiveLeftClick(x, y, playSound);
			foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
			{
				if ((specialOrder.questKey.Value == RSVConstants.SO_PIKAQUEST) && (!hadQuestBefore))
				{
					foreach (Farmer player in Game1.getAllFarmers())
					{
						if (player.activeDialogueEvents.ContainsKey(RSVConstants.CT_PIKATOPIC))
                        {
							player.activeDialogueEvents.Remove(RSVConstants.CT_PIKATOPIC);
                        }
						player.activeDialogueEvents.Add(RSVConstants.CT_PIKATOPIC, specialOrder.GetDaysLeft());
						Log.Trace($"RSV: Added pika_pickup conversation topic.");
					}
					return;
				}
			}
		}


        //mostly copied from the decompile
        //randomly chooses 2 SOs for each RSV board
        public static void UpdateAvailableRSVSpecialOrders(bool force_refresh)
		{
			if (Game1.player.team.availableSpecialOrders is not null)
			{
				foreach (SpecialOrder order in Game1.player.team.availableSpecialOrders)
				{
					if ((order.questDuration.Value == SpecialOrder.QuestDuration.TwoDays || order.questDuration.Value == SpecialOrder.QuestDuration.ThreeDays) && !Game1.player.team.acceptedSpecialOrderTypes.Contains(order.orderType.Value))
					{
						order.SetDuration(order.questDuration.Value);
					}
				}
			}

			if (force_refresh) Log.Trace("Refreshing RSV Special Orders");
			var availableOrders = Game1.player.team.availableSpecialOrders;

			Game1.player.team.acceptedSpecialOrderTypes.Remove(RSVConstants.Z_VILLAGESPECIALORDERBOARD);
			Game1.player.team.acceptedSpecialOrderTypes.Remove(RSVConstants.Z_NINJASPECIALORDERBOARD);

			for(int i=0; i< availableOrders.Count; i++)
            {
                if (availableOrders[i].orderType.Equals(RSVConstants.Z_NINJASPECIALORDERBOARD) || availableOrders[i].orderType.Equals(RSVConstants.Z_VILLAGESPECIALORDERBOARD))
                {
					Game1.player.team.availableSpecialOrders.RemoveAt(i);
					i--;
                }
            }

			Dictionary<string, SpecialOrderData> order_data = Game1.content.Load<Dictionary<string, SpecialOrderData>>("Data\\SpecialOrders");
			List<string> keys = new List<string>(order_data.Keys);

			for (int k = 0; k < keys.Count; k++)
			{
				string key = keys[k];
				if (force_refresh) Log.Trace($"Checking {key}");
				bool invalid = false;
				bool repeatable = order_data[key].Repeatable.Equals("True", StringComparison.OrdinalIgnoreCase);
				if (repeatable && Game1.MasterPlayer.team.completedSpecialOrders.ContainsKey(key))
				{
					if (force_refresh) Log.Trace($"Not repeatable and already done");
					invalid = true;
				}
				if (Game1.dayOfMonth >= 16 && order_data[key].Duration == "Month")
				{
					if (force_refresh) Log.Trace($"Month SO and after 16th");
					invalid = true;
				}
				if (!invalid && !SpecialOrder.CheckTags(order_data[key].RequiredTags))
				{
					if (force_refresh) Log.Trace($"Tags conditions not met.");
					invalid = true;
				}
				if (!invalid)
				{
					foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
					{
						if ((string)specialOrder.questKey.Value == key)
						{
							invalid = true;
							break;
						}
					}
				}
				if (force_refresh) Log.Trace($"Order {keys[k]} is valid: {!invalid}");
				if (invalid)
				{
					keys.RemoveAt(k);
					k--;
				}
			}
			Random r = new Random((int)Game1.uniqueIDForThisGame + (int)(Game1.stats.DaysPlayed * 1.3f));
			string[] array = new string[2] { RSVConstants.Z_NINJASPECIALORDERBOARD, RSVConstants.Z_VILLAGESPECIALORDERBOARD };
			foreach (string type_to_find in array)
			{
				List<string> typed_keys = new List<string>();
				foreach (string key3 in keys)
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

			LogCurrentlyAvailableSpecialOrders();

		}

		private static void LogCurrentlyAvailableSpecialOrders()
        {

			Log.Trace("Refreshed RSV SpecialOrders");
			foreach (var SO in Game1.player.team.availableSpecialOrders)
			{
				Log.Trace($"{SO.questKey.Value}, {SO.orderType.Value}");
			}
		}

	}
}
