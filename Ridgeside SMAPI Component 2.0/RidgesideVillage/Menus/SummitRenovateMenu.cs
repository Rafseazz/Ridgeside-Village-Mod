using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using xTile.Dimensions;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;


namespace RidgesideVillage
{
	internal class SummitBlueprint
	{
		public string translationKey;
		public string materials;
		public int price;
		public int buildDuration;
		public int imgWidth;
		public int imgHeight;
#nullable enable
		public Dictionary<int, int>? itemsRequired;
#nullable disable

		public void getItemsRequired()
        {
			itemsRequired = new Dictionary<int, int>();
			string[] recipeSplit = materials.Split(' ');
			for (int j = 0; j < recipeSplit.Length; j += 2)
			{
				if (!recipeSplit[j].Equals(""))
				{
					itemsRequired.Add(Convert.ToInt32(recipeSplit[j]), Convert.ToInt32(recipeSplit[j + 1]));
				}
			}
		}

		public bool doesFarmerHaveEnoughResourcesToBuild()
		{
			if (price < 0)
			{
				return false;
			}
			foreach (KeyValuePair<int, int> kvp in itemsRequired)
			{
				if (!Game1.player.hasItemInInventory(kvp.Key, kvp.Value))
				{
					return false;
				}
			}
			if (Game1.player.Money < price)
			{
				return false;
			}
			return true;
		}

		public void consumeResources()
		{
			foreach (KeyValuePair<int, int> kvp in itemsRequired)
			{
				Game1.player.consumeObject(kvp.Key, kvp.Value);
			}
			Game1.player.Money -= price;
		}
	}

	internal class SummitRenovateMenu : IClickableMenu
    {
		static IModHelper Helper;
		static IMonitor Monitor;

		const string FARMUPGRADE = "Summit House Upgrade";
		const string CLIMATECONTROL = "Climate Control";
		const string SPRINKLERS = "Sprinkler System";
		const string OREAREA = "Ore Area";
		const string SHED = "Summit Shed";

		public const int region_backButton = 101;

		public const int region_forwardButton = 102;

		public const int region_okButton = 106;

		public const int region_cancelButton = 107;

		public int maxWidthOfBuildingViewer = 448;

		public int maxHeightOfBuildingViewer = 512;

		public int maxWidthOfDescription = 416;

		private Dictionary<string, SummitBlueprint> blueprints;

		private string currentBlueprint;

		private string[] blueprintKeys;

		private int currentBlueprintIndex = 0;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent cancelButton;

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent forwardButton;

		private string buildingDescription;

		private string buildingName;

		Texture2D buildingImage;

		private List<Item> ingredients = new List<Item>();

		private int price;

		private bool onFarm = false;

		private bool drawBG = true;

		private string hoverText = "";

		public bool readOnly
		{
			set
			{
				if (value)
				{
					okButton.visible = false;
					cancelButton.leftNeighborID = 102;
				}
			}
		}

		internal static void Initialize(IMod ModInstance)
		{
			Helper = ModInstance.Helper;
			Monitor = ModInstance.Monitor;
		}

		public static void tryOpenRenovateMenu()
        {
			var all_blueprints = Helper.Data.ReadJsonFile<Dictionary<string, SummitBlueprint>>(PathUtilities.NormalizePath("assets/SummitUpgrades.json"));
			var valid_blueprints = new Dictionary<string, SummitBlueprint>();

			if (!Game1.MasterPlayer.mailReceived.Contains(RSVConstants.M_HOUSEUPGRADED))
			{
				valid_blueprints.Add(FARMUPGRADE, all_blueprints[FARMUPGRADE]);
			}
			else
			{
				if (!Game1.MasterPlayer.mailReceived.Contains(RSVConstants.M_CLIMATECONTROLLED))
				{
					valid_blueprints.Add(CLIMATECONTROL, all_blueprints[CLIMATECONTROL]);
				}
				if (!Game1.MasterPlayer.mailReceived.Contains(RSVConstants.M_GOTSPRINKLERS))
				{
					valid_blueprints.Add(SPRINKLERS, all_blueprints[SPRINKLERS]);
				}
				if (!Game1.MasterPlayer.mailReceived.Contains(RSVConstants.M_OREAREAOPENED))
				{
					valid_blueprints.Add(OREAREA, all_blueprints[OREAREA]);
				}
				if (!Game1.MasterPlayer.mailReceived.Contains(RSVConstants.M_SHEDADDED))
				{
					valid_blueprints.Add(SHED, all_blueprints[SHED]);
				}
			}
			if (valid_blueprints.Count == 0)
			{
				NPC worker = Game1.isRaining ? Game1.getCharacterFromName("Ian") : Game1.getCharacterFromName("Sean");
				worker.CurrentDialogue.Clear();
				worker.CurrentDialogue.Push(new Dialogue(Helper.Translation.Get("IanShop.AllRenovated"), worker));
				Game1.drawDialogue(worker);
				return;
            }
            else
            {

				Game1.activeClickableMenu = new SummitRenovateMenu(valid_blueprints);
			}
		}

		public SummitRenovateMenu(Dictionary<string, SummitBlueprint> blueprints)
		{
			Game1.player.forceCanMove();
			resetBounds();
			this.blueprints = blueprints;
			blueprintKeys = blueprints.Keys.ToArray();

			setNewActiveBlueprint();
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override bool shouldClampGamePadCursor()
		{
			return onFarm;
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(107);
			snapCursorToCurrentSnappedComponent();
		}

		private void resetBounds()
		{
			xPositionOnScreen = Game1.uiViewport.Width / 2 - maxWidthOfBuildingViewer - spaceToClearSideBorder;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - maxHeightOfBuildingViewer / 2 - spaceToClearTopBorder + 32;
			width = maxWidthOfBuildingViewer + maxWidthOfDescription + spaceToClearSideBorder * 2 + 64;
			height = maxHeightOfBuildingViewer + spaceToClearTopBorder;
			initialize(xPositionOnScreen, yPositionOnScreen, width, height, showUpperRightCloseButton: true);
			okButton = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - borderWidth - spaceToClearSideBorder - 192 - 12, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(366, 373, 16, 16), 4f)
			{
				myID = 106,
				rightNeighborID = 104,
				leftNeighborID = 105
			};
			cancelButton = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
			{
				myID = 107,
				leftNeighborID = 104
			};
			backButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 64, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 48, 44), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 101,
				rightNeighborID = 102
			};
			forwardButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + maxWidthOfBuildingViewer - 256 + 16, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 48, 44), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 102,
				leftNeighborID = 101,
				rightNeighborID = -99998
			};
		}

		public void setNewActiveBlueprint()
        {
            try
            {
				currentBlueprint = blueprintKeys.ElementAt(currentBlueprintIndex);
				buildingImage = Helper.ModContent.Load<Texture2D>($"assets/{currentBlueprint}.png");
				var blueprintData = blueprints[currentBlueprint];
				price = blueprintData.price;
				ingredients.Clear();
				blueprintData.getItemsRequired();
				foreach (KeyValuePair<int, int> v in blueprintData.itemsRequired)
				{
					ingredients.Add(new StardewValley.Object(v.Key, v.Value));
				}
				buildingName = Helper.Translation.Get(blueprintData.translationKey + ".Name");
				buildingDescription = Helper.Translation.Get(blueprintData.translationKey+".Description");
			}
            catch(Exception e)
			{
				Log.Error($"Failed at blueprint {currentBlueprint}. Please notify the authors of RSV about this. :c");
				Log.Error(e.Message);
				Log.Error(e.StackTrace);
            }
			
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (!onFarm && b == Buttons.LeftTrigger)
			{
				currentBlueprintIndex--;
				if (currentBlueprintIndex < 0)
				{
					currentBlueprintIndex = blueprints.Count - 1;
				}
				setNewActiveBlueprint();
				Game1.playSound("shwip");
			}
			if (!onFarm && b == Buttons.RightTrigger)
			{
				currentBlueprintIndex = (currentBlueprintIndex + 1) % blueprints.Count;
				setNewActiveBlueprint();
				Game1.playSound("shwip");
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (!onFarm || Game1.IsFading())
			{
				return;
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!onFarm)
			{
				base.receiveLeftClick(x, y, playSound);
			}
			if (cancelButton.containsPoint(x, y))
			{
				exitThisMenu();
				Game1.player.forceCanMove();
				Game1.playSound("bigDeSelect");
			}
			if (!onFarm && backButton.containsPoint(x, y))
			{
				currentBlueprintIndex--;
				if (currentBlueprintIndex < 0)
				{
					currentBlueprintIndex = blueprints.Count - 1;
				}
				setNewActiveBlueprint();
				Game1.playSound("shwip");
				backButton.scale = backButton.baseScale;
			}
			if (!onFarm && forwardButton.containsPoint(x, y))
			{
				currentBlueprintIndex = (currentBlueprintIndex + 1) % blueprints.Count;
				setNewActiveBlueprint();
				backButton.scale = backButton.baseScale;
				Game1.playSound("shwip");
			}
			if (okButton.containsPoint(x, y) && !onFarm && price >= 0 && Game1.player.Money >= price && blueprints[currentBlueprint].doesFarmerHaveEnoughResourcesToBuild())
			{
				//Game1.globalFadeToBlack(setUpForBuildingPlacement);
				Game1.playSound("smallSelect");
				Game1.player.team.buildLock.RequestLock(delegate
				{
					if (tryToBuild())
					{
						blueprints[currentBlueprint].consumeResources();
						seanConstructionMessage();
					}
					else
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), Color.Red, 3500f));
					}
					Game1.player.team.buildLock.ReleaseLock();
				});
			}
				

		}

		public bool tryToBuild()
		{
            try
            {
				switch (currentBlueprint)
				{
					case FARMUPGRADE:
						Game1.MasterPlayer.activeDialogueEvents.Add(RSVConstants.CT_HOUSEUPGRADE, 3);
						Game1.getLocationFromName(RSVConstants.L_SUMMITFARM).modData["renovated"] = "true";
						break;
					case CLIMATECONTROL:
						Game1.MasterPlayer.activeDialogueEvents.Add(RSVConstants.CT_CLIMATE, 3);
						break;
					case SPRINKLERS:
						Game1.MasterPlayer.activeDialogueEvents.Add(RSVConstants.CT_SPRINKLER, 3);
						break;
					case OREAREA:
						Game1.MasterPlayer.activeDialogueEvents.Add(RSVConstants.CT_OREAREA, 3);
						break;
					case SHED:
						Game1.MasterPlayer.activeDialogueEvents.Add(RSVConstants.CT_SHED, 3);
						break;
				}
				Game1.MasterPlayer.activeDialogueEvents.Add(RSVConstants.CT_ACTIVECONSTRUCTION, 3);
				return true;
			}
			catch (Exception e)
            {
				Log.Trace($"RSV: Could not build {currentBlueprint}:\n{e}");
				return false;
            }
			//return ((Farm)Game1.getLocationFromName("Farm")).buildStructure(this.CurrentBlueprint, new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64), Game1.player, this.magicalConstruction);
		}

		public void seanConstructionMessage()
		{
			exitThisMenu();
			Game1.player.forceCanMove();
			NPC worker = Game1.isRaining ? Game1.getCharacterFromName("Ian") : Game1.getCharacterFromName("Sean");
			worker.CurrentDialogue.Clear();
			if (blueprints[currentBlueprint].buildDuration <= 0)
			{
				worker.CurrentDialogue.Push(new Dialogue(Helper.Translation.Get("IanShop.Instant", new { project = buildingName }), worker));
			}
            else
            {
				worker.CurrentDialogue.Push(new Dialogue(Helper.Translation.Get("IanShop.Construction", new { project = buildingName }), worker));
			}
			Game1.drawDialogue(worker);
		}

		
		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return onFarm;
		}

		public override void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
		{
			resetBounds();
		}

		public override void draw(SpriteBatch b)
		{
			if (drawBG)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
			}
			if (Game1.IsFading())
			{
				return;
			}
			var currentBlueprintData = blueprints[currentBlueprint];
			if (!onFarm)
			{
				base.draw(b);
				drawTextureBox(b, xPositionOnScreen - 96, yPositionOnScreen - 16, maxWidthOfBuildingViewer + 64, maxHeightOfBuildingViewer + 64, Color.White);
				b.Draw(buildingImage, new Vector2(xPositionOnScreen + maxWidthOfBuildingViewer / 2 - 8 * 64 / 2 - 64, yPositionOnScreen + maxHeightOfBuildingViewer / 2 - currentBlueprintData.imgHeight * 4 / 2), new Microsoft.Xna.Framework.Rectangle(0, 0, currentBlueprintData.imgWidth, currentBlueprintData.imgHeight), Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1);
				string placeholder = " Deluxe  Barn   ";
				if (SpriteText.getWidthOfString(buildingName) >= SpriteText.getWidthOfString(placeholder))
				{
					placeholder = buildingName + " ";
				}
				SpriteText.drawStringWithScrollCenteredAt(b, buildingName, xPositionOnScreen + maxWidthOfBuildingViewer - spaceToClearSideBorder - 16 + 64 + (width - (maxWidthOfBuildingViewer + 128)) / 2, yPositionOnScreen, SpriteText.getWidthOfString(placeholder));
				int descriptionWidth = maxWidthOfDescription + 64;
				drawTextureBox(b, xPositionOnScreen + maxWidthOfBuildingViewer - 16, yPositionOnScreen + 80, descriptionWidth, maxHeightOfBuildingViewer - 32, Color.White);
				Utility.drawTextWithShadow(b, Game1.parseText(buildingDescription, Game1.dialogueFont, descriptionWidth - 32), Game1.dialogueFont, new Vector2(xPositionOnScreen + maxWidthOfBuildingViewer, yPositionOnScreen + 80 + 16), Game1.textColor, 1f, -1f, -1, -1, 0.75f);
				Vector2 ingredientsPosition = new Vector2(xPositionOnScreen + maxWidthOfBuildingViewer + 16, yPositionOnScreen + 256 + 32);
				if (ingredients.Count < 3 && (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt))
				{
					ingredientsPosition.Y += 64f;
				}
				if (price >= 0)
				{
					SpriteText.drawString(b, "$", (int)ingredientsPosition.X, (int)ingredientsPosition.Y);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 4f, ingredientsPosition.Y + 4f), (Game1.player.Money < price) ? Color.Red : Game1.textColor, 1f, -1f, -1, -1, 0.25f);
				}
				ingredientsPosition.X -= 16f;
				ingredientsPosition.Y -= 21f;
				foreach (Item i in ingredients)
				{
					ingredientsPosition.Y += 68f;
					i.drawInMenu(b, ingredientsPosition, 1f);
					bool hasItem = (!(i is StardewValley.Object) || Game1.player.hasItemInInventory((i as StardewValley.Object).ParentSheetIndex, i.Stack)) ? true : false;
					Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 16f, ingredientsPosition.Y + 20f), hasItem ? Game1.textColor : Color.Red, 1f, -1f, -1, -1, 0.25f);
				}
				backButton.draw(b, blueprintKeys.Length > 1 ? Color.White : (Color.Gray * 0.8f), 0.88f);
				forwardButton.draw(b, blueprintKeys.Length > 1 ? Color.White : (Color.Gray * 0.8f), 0.88f);
				okButton.draw(b, currentBlueprintData.doesFarmerHaveEnoughResourcesToBuild() ? Color.White : (Color.Gray * 0.8f), 0.88f);
			}
			cancelButton.draw(b);
			drawMouse(b);
			if (hoverText.Length > 0)
			{
				drawHoverText(b, hoverText, Game1.dialogueFont);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}
	}
}
