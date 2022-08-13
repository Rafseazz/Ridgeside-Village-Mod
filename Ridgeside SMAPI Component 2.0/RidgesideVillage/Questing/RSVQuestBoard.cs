using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Quests;
using StardewModdingAPI;
using StardewValley.BellsAndWhistles;
using StardewModdingAPI.Events;

namespace RidgesideVillage.Questing
{
    internal class RSVQuestBoard : Billboard
    {


		static IModHelper Helper => ModEntry.Helper;
		int timestampOpened;
		static int safetyTimer = 500;
		internal Quest dailyQuest;
		private QuestData questData;
		private string boardType;
		const string VILLAGEBOARD = "VillageQuestBoard";
		const string NINJAQUESTBOARD = "RSVNinjaBoard";
		Texture2D billboardTexture;
		Color fontColor = Game1.textColor;

		string description;
		

  

        internal RSVQuestBoard(QuestData questData, string boardType = "") : base(dailyQuest: true)
        {
			this.questData = questData;
			this.boardType = boardType;
			timestampOpened = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
			if( boardType == NINJAQUESTBOARD)
            {
				this.dailyQuest = questData.dailyNinjaHouseQuest;
				this.acceptQuestButton.visible = !questData.acceptedDailyNinjaHouseQuest;
			}
            else
            {
				this.dailyQuest = questData.dailyTownQuest;
				this.acceptQuestButton.visible = !questData.acceptedDailyQuest;
			}

			if(this.dailyQuest is not null)
            {
				this.description = dailyQuest.questDescription.Replace("^", "\n");
			}
			//setting here and in parent. not sure if parent is needed
			Texture2D billboardTexture;
			Log.Debug($"{boardType}, {VILLAGEBOARD}, {boardType.Equals(VILLAGEBOARD)}");
			if (boardType.Equals(VILLAGEBOARD))
			{
				billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\RSVQuestBoard");

			}
			else if (boardType.Equals(NINJAQUESTBOARD))
			{
				billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\RSVNinjaBoard");
			}
			else
			{
				//this case shouldnt happen, it'll draw the board a bit wrong
				billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Billboard");
			}
			Helper.Reflection.GetField<Texture2D>(this, "billboardTexture").SetValue(billboardTexture);
			this.billboardTexture = billboardTexture;


		}


		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (this.timestampOpened + safetyTimer < Game1.currentGameTime.TotalGameTime.TotalMilliseconds)
			{
				base.receiveRightClick(x, y, playSound);
			}
			return;
		}
		public override void draw(SpriteBatch b)
		{
			bool hide_mouse = false;
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			b.Draw(this.billboardTexture, new Vector2(base.xPositionOnScreen, base.yPositionOnScreen), null, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			
			if (Game1.options.SnappyMenus)
			{
				hide_mouse = true;
			}
			if (this.dailyQuest == null || this.dailyQuest.currentObjective == null || this.dailyQuest.currentObjective.Length == 0)
			{
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:Billboard_NothingPosted"), new Vector2(base.xPositionOnScreen + 384, base.yPositionOnScreen + 320), this.fontColor);
			}
			else
			{
				SpriteFont font = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont);
				string description = Game1.parseText(this.description, font, 640);
				Utility.drawTextWithShadow(b, description, font, new Vector2(base.xPositionOnScreen + 320 + 32, base.yPositionOnScreen + 256), this.fontColor, 1f, -1f, -1, -1, 0.5f);
				if (this.acceptQuestButton.visible)
				{
					hide_mouse = false;
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.acceptQuestButton.bounds.X, this.acceptQuestButton.bounds.Y, this.acceptQuestButton.bounds.Width, this.acceptQuestButton.bounds.Height, (this.acceptQuestButton.scale > 1f) ? Color.LightPink : Color.White, 4f * this.acceptQuestButton.scale);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2(this.acceptQuestButton.bounds.X + 12, this.acceptQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), this.fontColor);
				}
			}

			if (this.upperRightCloseButton != null && this.shouldDrawCloseButton())
			{
				this.upperRightCloseButton.draw(b);
			}

			if (!hide_mouse)
			{
				Game1.mouseCursorTransparency = 1f;
				base.drawMouse(b);
			}
		}

		public override void performHoverAction(int x, int y)
		{
			if (dailyQuest!= null && this.acceptQuestButton.visible)
			{
				float oldScale = this.acceptQuestButton.scale;
				this.acceptQuestButton.scale = (this.acceptQuestButton.bounds.Contains(x, y) ? 1.5f : 1f);
				if (this.acceptQuestButton.scale > oldScale)
				{
					Game1.playSound("Cowboy_gunshot");
				}

				if (this.upperRightCloseButton != null)
				{
					this.upperRightCloseButton.tryHover(x, y, 0.5f);
				}
			}

		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.acceptQuestButton.visible && this.acceptQuestButton.containsPoint(x, y))
			{
				Game1.playSound("newArtifact");
				if (this.boardType == NINJAQUESTBOARD)
				{
					this.questData.acceptedDailyNinjaHouseQuest = true;
				}
				else
				{
					this.questData.acceptedDailyQuest = true;
				}
				Game1.player.questLog.Add(this.dailyQuest);

				this.acceptQuestButton.visible = false;
			}
			else if (this.upperRightCloseButton != null && this.upperRightCloseButton.containsPoint(x, y))
			{
				if (playSound)
				{
					Game1.playSound("bigDeSelect");
				}
				this.exitThisMenu();
			}
		}
	}
}
