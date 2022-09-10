using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Events;
using StardewModdingAPI.Events;
using StardewModdingAPI;
using Netcode;

namespace RidgesideVillage
{
	internal static class NightlyEvent
    {
		static IModHelper Helper;
		static IMonitor Monitor;

		internal static void Initialize(IMod ModInstance)
		{
			Helper = ModInstance.Helper;
			Monitor = ModInstance.Monitor;

			Helper.Events.GameLoop.DayStarted += OnDayStart;
			//Helper.Events.GameLoop.DayEnding += OnDayEnd;
		}

		private static void OnDayStart(object sender, DayStartedEventArgs e)
		{
			if (!Game1.IsMasterGame)
				return;
			if (Game1.player.hasItemInInventoryNamed("Old Lucky Foxtail Charm") && Game1.player.eventsSeen.Contains(RSVConstants.E_LORENZO1H) && !Game1.player.eventsSeen.Contains(RSVConstants.E_GRANDMA))
			{
				var location = Game1.getLocationFromName("FarmHouse");
				var events = location.GetLocationEvents();
				string eventString = events["grandmaMessage"];
				Game1.player.eventsSeen.Add(RSVConstants.E_GRANDMA);
				Log.Debug($"Now playing grandma event");
				UtilFunctions.StartEvent(new Event(eventString), "FarmHouse", -1000, -1000);
			}
		}

		private static void OnDayEnd(object sender, DayEndingEventArgs e)
		{
			/*
			if (!Game1.player.IsMainPlayer)
				return;
			if (Game1.player.hasItemInInventoryNamed("Old Lucky Foxtail Charm") && Game1.player.eventsSeen.Contains(lorenzo_id) && !Game1.player.eventsSeen.Contains(grandma_id))
			{
				var location = Game1.getLocationFromName("FarmHouse");
				var events = location.GetLocationEvents();
				string eventString = events["grandmaMessage"];
				Game1.player.eventsSeen.Add(id);
				Log.Debug($"Player has seen now this event: {Game1.player.eventsSeen.Contains(id)}");
				UtilFunctions.StartEvent(new Event(eventString), "FarmHouse", -1000, -1000);
			}
			*/
			Game1.farmEvent = new GrandmaEvent();
		}
	}

    public class GrandmaEvent : FarmEvent
	{
		static IModHelper Helper;
		static IMonitor Monitor;

		private Texture2D sprite;

		private Vector2 spritePosition;

		private int timer;

		private string message;

		public NetFields NetFields { get; } = new NetFields();

		public GrandmaEvent()
		{
			/*
			Log.Trace($"RSV: Initializing grandma event");
			message = Helper.Translation.Get("GrandmaMessage.1");
			sprite = Helper.Content.Load<Texture2D>("assets/Foxtail.png");
			spritePosition = new Vector2(Game1.viewport.Width/2-8, Game1.viewport.Height/2-8);
			*/
		}

		public bool setUp()
		{
			/*
			Game1.changeMusicTrack("aweAmbience");
			return true;
			*/
			var location = Game1.getLocationFromName("FarmHouse");
			var events = location.GetLocationEvents();
			string eventString = events["grandmaMessage"];
			UtilFunctions.StartEvent(new Event(eventString), "FarmHouse", -1000, -1000);
			return false;
		}

		public bool tickUpdate(GameTime time)
		{
			/*
			Game1.player.CanMove = false;
			this.timer += time.ElapsedGameTime.Milliseconds;
			Game1.fadeToBlackAlpha = 1f;
			this.spritePosition.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 512.0) * 1f;
			if (this.timer > 1500)
			{
				Game1.drawObjectDialogue(this.message);
			}
			else if (!Game1.fadeToBlack && this.timer > 10000)
			{
				Game1.globalFadeToBlack();
				Game1.changeMusicTrack("none");
				this.spritePosition.X = -999999f;
			}
			return !Game1.dialogueUp;
			*/
			return true;
		}

		public void draw(SpriteBatch b)
		{
			//b.Draw(Game1.mouseCursors2, this.spritePosition, new Rectangle(0, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9999999f);
		}

		public void drawAboveEverything(SpriteBatch b)
		{
		}

		public void makeChangesToLocation()
		{
			//Game1.messagePause = false;
		}
	}
}