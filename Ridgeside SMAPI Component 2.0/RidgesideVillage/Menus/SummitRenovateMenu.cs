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


namespace RidgesideVillage
{
    internal class SummitRenovateMenu : IClickableMenu
    {
		static IModHelper Helper;
		static IMonitor Monitor;

		public const string FARMUPGRADE = "Summit House Upgrade";
		public const string CLIMATECONTROL = "Climate Control";
		public const string SPRINKLERS = "Sprinkler System";
		public const string OREAREA = "Ore Area";
		public const string SHED = "Summit Shed";
		public const string HOUSETOPIC = "RSV.HouseCT";
		public const string CLIMATETOPIC = "RSV.ClimateCT";
		public const string SPRINKLERTOPIC = "RSV.SprinklerCT";
		public const string ORETOPIC = "RSV.OreCT";
		public const string SHEDTOPIC = "RSV.ShedCT";
		public const string ACTIVECONSTRUCTION = "RSV.ActiveConstruction";

		public const int region_backButton = 101;

		public const int region_forwardButton = 102;

		//public const int region_upgradeIcon = 103;

		//public const int region_demolishButton = 104;

		//public const int region_moveBuitton = 105;

		public const int region_okButton = 106;

		public const int region_cancelButton = 107;

		//public const int region_paintButton = 108;

		public int maxWidthOfBuildingViewer = 448;

		public int maxHeightOfBuildingViewer = 512;

		public int maxWidthOfDescription = 416;

		private List<BluePrint> blueprints;

		private int currentBlueprintIndex;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent cancelButton;

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent forwardButton;

		//public ClickableTextureComponent upgradeIcon;

		//public ClickableTextureComponent demolishButton;

		//public ClickableTextureComponent moveButton;

		//public ClickableTextureComponent paintButton;

		private BluePrint currentBuilding;

		//private Building buildingToMove;

		private string buildingDescription;

		private string buildingName;

		Texture2D buildingImage;

		private List<Item> ingredients = new List<Item>();

		private int price;

		private bool onFarm = false;

		private bool drawBG = true;

		private bool freeze;

		//private bool upgrading;

		//private bool demolishing;

		//private bool moving;

		//private bool magicalConstruction;

		//private bool painting;

		//protected BluePrint _demolishCheckBlueprint;

		private string hoverText = "";

		public bool readOnly
		{
			set
			{
				if (value)
				{
					//this.upgradeIcon.visible = false;
					//this.demolishButton.visible = false;
					//this.moveButton.visible = false;
					this.okButton.visible = false;
					//this.paintButton.visible = false;
					this.cancelButton.leftNeighborID = 102;
				}
			}
		}

		internal static void Initialize(IMod ModInstance)
		{
			Helper = ModInstance.Helper;
			Monitor = ModInstance.Monitor;
		}

		public BluePrint CurrentBlueprint => blueprints[currentBlueprintIndex];

		public SummitRenovateMenu()
		{
			Game1.player.forceCanMove();
			resetBounds();
			blueprints = new List<BluePrint>();

			if (!Game1.MasterPlayer.mailReceived.Contains(IanShop.HOUSEUPGRADED))
			{
				blueprints.Add(new BluePrint(FARMUPGRADE));
			}
			else
			{
				if (!Game1.MasterPlayer.mailReceived.Contains(IanShop.CLIMATECONTROLLED))
				{
					blueprints.Add(new BluePrint(CLIMATECONTROL));
				}
				if (!Game1.MasterPlayer.mailReceived.Contains(IanShop.GOTSPRINKLERS))
				{
					blueprints.Add(new BluePrint(SPRINKLERS));
				}
				if (!Game1.MasterPlayer.mailReceived.Contains(IanShop.OREAREAOPENED))
				{
					blueprints.Add(new BluePrint(OREAREA));
				}
				if (!Game1.MasterPlayer.mailReceived.Contains(IanShop.SHEDADDED))
				{
					blueprints.Add(new BluePrint(SHED));
				}
			}
			if (blueprints.Count == 0)
            {
				NPC sean = Game1.getCharacterFromName("Sean");
				sean.CurrentDialogue.Clear();
				sean.CurrentDialogue.Push(new Dialogue(Helper.Translation.Get("IanShop.AllRenovated"), sean));
				Game1.drawDialogue(sean);
				return;
            }
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
			/*
			this.demolishButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_Demolish"), new Microsoft.Xna.Framework.Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128 - 8, base.yPositionOnScreen + this.maxHeightOfBuildingViewer + 64 - 4, 64, 64), null, null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(348, 372, 17, 17), 4f)
			{
				myID = 104,
				rightNeighborID = 107,
				leftNeighborID = 106
			};
			this.upgradeIcon = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(base.xPositionOnScreen + this.maxWidthOfBuildingViewer - 128 + 32, base.yPositionOnScreen + 8, 36, 52), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(402, 328, 9, 13), 4f)
			{
				myID = 103,
				rightNeighborID = 104,
				leftNeighborID = 105
			};
			this.moveButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_MoveBuildings"), new Microsoft.Xna.Framework.Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 256 - 20, base.yPositionOnScreen + this.maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(257, 284, 16, 16), 4f)
			{
				myID = 105,
				rightNeighborID = 106,
				leftNeighborID = -99998
			};
			this.paintButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_PaintBuildings"), new Microsoft.Xna.Framework.Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 320 - 20, base.yPositionOnScreen + this.maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors2, new Microsoft.Xna.Framework.Rectangle(80, 208, 16, 16), 4f)
			{
				myID = 105,
				rightNeighborID = -99998,
				leftNeighborID = -99998
			};
			
			bool has_owned_buildings = false;
			bool has_paintable_buildings = this.CanPaintHouse() && this.HasPermissionsToPaint(null);
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building.hasCarpenterPermissions())
				{
					has_owned_buildings = true;
				}
				if (building.CanBePainted() && this.HasPermissionsToPaint(building))
				{
					has_paintable_buildings = true;
				}
			}
			
			this.demolishButton.visible = Game1.IsMasterGame;
			this.moveButton.visible = Game1.IsMasterGame || Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.On || (Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.OwnedBuildings && has_owned_buildings);
			this.paintButton.visible = has_paintable_buildings;
			if (this.magicalConstruction)
			{
				this.paintButton.visible = false;
			}
			if (!this.demolishButton.visible)
			{
				this.upgradeIcon.rightNeighborID = this.demolishButton.rightNeighborID;
				this.okButton.rightNeighborID = this.demolishButton.rightNeighborID;
				this.cancelButton.leftNeighborID = this.demolishButton.leftNeighborID;
			}
			if (!this.moveButton.visible)
			{
				this.upgradeIcon.leftNeighborID = this.moveButton.leftNeighborID;
				this.forwardButton.rightNeighborID = -99998;
				this.okButton.leftNeighborID = this.moveButton.leftNeighborID;
			}
			*/
		}

		public void setNewActiveBlueprint()
		{
			currentBuilding = blueprints[currentBlueprintIndex];
			price = currentBuilding.moneyRequired;
			ingredients.Clear();
			foreach (KeyValuePair<int, int> v in currentBuilding.itemsRequired)
			{
				ingredients.Add(new StardewValley.Object(v.Key, v.Value));
			}
			buildingDescription = currentBuilding.description;
			buildingName = currentBuilding.displayName;
			buildingImage = Helper.ModContent.Load<Texture2D>($"assets/{CurrentBlueprint.name}.png");
		}

		/*
		public override void performHoverAction(int x, int y)
		{
			this.cancelButton.tryHover(x, y);
			base.performHoverAction(x, y);
			if (!this.onFarm)
			{
				this.backButton.tryHover(x, y, 1f);
				this.forwardButton.tryHover(x, y, 1f);
				this.okButton.tryHover(x, y);
				this.demolishButton.tryHover(x, y);
				this.moveButton.tryHover(x, y);
				this.paintButton.tryHover(x, y);
				if (this.CurrentBlueprint.isUpgrade() && this.upgradeIcon.containsPoint(x, y))
				{
					this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Upgrade", new BluePrint(this.CurrentBlueprint.nameOfBuildingToUpgrade).displayName);
				}
				else if (this.demolishButton.containsPoint(x, y) && this.CanDemolishThis(this.CurrentBlueprint))
				{
					this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Demolish");
				}
				else if (this.moveButton.containsPoint(x, y))
				{
					this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_MoveBuildings");
				}
				else if (this.okButton.containsPoint(x, y) && this.CurrentBlueprint.doesFarmerHaveEnoughResourcesToBuild())
				{
					this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Build");
				}
				else if (this.paintButton.containsPoint(x, y))
				{
					this.hoverText = this.paintButton.name;
				}
				else
				{
					this.hoverText = "";
				}
			}
			else
			{
				if ((!this.upgrading && !this.demolishing && !this.moving && !this.painting) || this.freeze)
				{
					return;
				}
				Farm farm = Game1.getFarm();
				Vector2 tile_pos = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
				if (this.painting && farm.GetHouseRect().Contains(Utility.Vector2ToPoint(tile_pos)) && this.HasPermissionsToPaint(null) && this.CanPaintHouse())
				{
					farm.frameHouseColor = Color.Lime;
				}
				foreach (Building building in ((Farm)Game1.getLocationFromName("Farm")).buildings)
				{
					building.color.Value = Color.White;
				}
				Building b = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(tile_pos);
				if (b == null)
				{
					b = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false) + 128) / 64));
					if (b == null)
					{
						b = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false) + 192) / 64));
					}
				}
				if (this.upgrading)
				{
					if (b != null && this.CurrentBlueprint.nameOfBuildingToUpgrade != null && this.CurrentBlueprint.nameOfBuildingToUpgrade.Equals(b.buildingType))
					{
						b.color.Value = Color.Lime * 0.8f;
					}
					else if (b != null)
					{
						b.color.Value = Color.Red * 0.8f;
					}
				}
				else if (this.demolishing)
				{
					if (b != null && this.hasPermissionsToDemolish(b) && this.CanDemolishThis(b))
					{
						b.color.Value = Color.Red * 0.8f;
					}
				}
				else if (this.moving)
				{
					if (b != null && this.hasPermissionsToMove(b))
					{
						b.color.Value = Color.Lime * 0.8f;
					}
				}
				else if (this.painting && b != null && b.CanBePainted() && this.HasPermissionsToPaint(b))
				{
					b.color.Value = Color.Lime * 0.8f;
				}
			}
		}

		public bool hasPermissionsToDemolish(Building b)
		{
			if (Game1.IsMasterGame)
			{
				return this.CanDemolishThis(b);
			}
			return false;
		}

		public bool CanPaintHouse()
		{
			return Game1.MasterPlayer.HouseUpgradeLevel >= 2;
		}
		
		public bool HasPermissionsToPaint(Building b)
		{
			if (b == null)
			{
				if (Game1.player.UniqueMultiplayerID == Game1.MasterPlayer.UniqueMultiplayerID)
				{
					return true;
				}
				if (Game1.player.spouse == Game1.MasterPlayer.UniqueMultiplayerID.ToString())
				{
					return true;
				}
				return false;
			}
			if (b.isCabin && b.indoors.Value is Cabin)
			{
				Farmer cabin_owner = (b.indoors.Value as Cabin).owner;
				if (Game1.player.UniqueMultiplayerID == cabin_owner.UniqueMultiplayerID)
				{
					return true;
				}
				if (Game1.player.spouse == cabin_owner.UniqueMultiplayerID.ToString())
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public bool hasPermissionsToMove(Building b)
		{
			if (!Game1.getFarm().greenhouseUnlocked.Value && b is GreenhouseBuilding)
			{
				return false;
			}
			if (Game1.IsMasterGame)
			{
				return true;
			}
			if (Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.On)
			{
				return true;
			}
			if (Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.OwnedBuildings && b.hasCarpenterPermissions())
			{
				return true;
			}
			return false;
		}
		*/

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

		/*
		public override void receiveKeyPress(Keys key)
		{
			if (freeze)
			{
				return;
			}
			if (!onFarm)
			{
				base.receiveKeyPress(key);
			}
			if (Game1.IsFading() || !onFarm)
			{
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose() && Game1.locationRequest == null)
			{
				this.returnToCarpentryMenu();
			}
			else if (!Game1.options.SnappyMenus)
			{
				if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
				{
					Game1.panScreen(0, 4);
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
				{
					Game1.panScreen(4, 0);
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
				{
					Game1.panScreen(0, -4);
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
				{
					Game1.panScreen(-4, 0);
				}
			}
		}
		*/

		public override void update(GameTime time)
		{
			base.update(time);
			if (!onFarm || Game1.IsFading())
			{
				return;
			}
			/*
			int mouseX = Game1.getOldMouseX(ui_scale: false) + Game1.viewport.X;
			int mouseY = Game1.getOldMouseY(ui_scale: false) + Game1.viewport.Y;
			if (mouseX - Game1.viewport.X < 64)
			{
				Game1.panScreen(-8, 0);
			}
			else if (mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -128)
			{
				Game1.panScreen(8, 0);
			}
			if (mouseY - Game1.viewport.Y < 64)
			{
				Game1.panScreen(0, -8);
			}
			else if (mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -64)
			{
				Game1.panScreen(0, 8);
			}
			Keys[] pressedKeys = Game1.oldKBState.GetPressedKeys();
			foreach (Keys key in pressedKeys)
			{
				this.receiveKeyPress(key);
			}
			if (Game1.IsMultiplayer)
			{
				return;
			}
			Farm farm = Game1.getFarm();
			foreach (FarmAnimal value in farm.animals.Values)
			{
				value.MovePosition(Game1.currentGameTime, Game1.viewport, farm);
			}
			*/
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (freeze)
			{
				return;
			}
			if (!onFarm)
			{
				base.receiveLeftClick(x, y, playSound);
			}
			if (cancelButton.containsPoint(x, y))
			{
				/*
				if (onFarm)
				{
					if (this.moving && this.buildingToMove != null)
					{
						Game1.playSound("cancel");
						return;
					}
					this.returnToCarpentryMenu();
					Game1.playSound("smallSelect");
					return;
				}
				*/
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
			/*
			if (!this.onFarm && this.demolishButton.containsPoint(x, y) && this.demolishButton.visible && this.CanDemolishThis(this.blueprints[this.currentBlueprintIndex]))
			{
				Game1.globalFadeToBlack(setUpForBuildingPlacement);
				Game1.playSound("smallSelect");
				this.onFarm = true;
				this.demolishing = true;
			}
			if (!this.onFarm && this.moveButton.containsPoint(x, y) && this.moveButton.visible)
			{
				Game1.globalFadeToBlack(setUpForBuildingPlacement);
				Game1.playSound("smallSelect");
				this.onFarm = true;
				this.moving = true;
			}
			if (!this.onFarm && this.paintButton.containsPoint(x, y) && this.paintButton.visible)
			{
				Game1.globalFadeToBlack(setUpForBuildingPlacement);
				Game1.playSound("smallSelect");
				this.onFarm = true;
				this.painting = true;
			}
			*/
			if (okButton.containsPoint(x, y) && !onFarm && price >= 0 && Game1.player.Money >= price && blueprints[currentBlueprintIndex].doesFarmerHaveEnoughResourcesToBuild())
			{
				//Game1.globalFadeToBlack(setUpForBuildingPlacement);
				Game1.playSound("smallSelect");
				Game1.player.team.buildLock.RequestLock(delegate
				{
					if (tryToBuild())
					{
						CurrentBlueprint.consumeResources();
						//DelayedAction.functionAfterDelay(returnToCarpentryMenuAfterSuccessfulBuild, 2000);
						//freeze = true;
						seanConstructionMessage();
					}
					else
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), Color.Red, 3500f));
					}
					Game1.player.team.buildLock.ReleaseLock();
				});
				//this.onFarm = true;
			}
				

		}

		public bool tryToBuild()
		{
            try
            {
				switch (currentBuilding.name)
				{
					case FARMUPGRADE:
						Game1.MasterPlayer.activeDialogueEvents.Add(HOUSETOPIC, 3);
						break;
					case CLIMATECONTROL:
						Game1.MasterPlayer.activeDialogueEvents.Add(CLIMATETOPIC, 3);
						break;
					case SPRINKLERS:
						Game1.MasterPlayer.activeDialogueEvents.Add(SPRINKLERTOPIC, 3);
						break;
					case OREAREA:
						Game1.MasterPlayer.activeDialogueEvents.Add(ORETOPIC, 3);
						break;
					case SHED:
						Game1.MasterPlayer.activeDialogueEvents.Add(SHEDTOPIC, 3);
						break;
				}
				Game1.MasterPlayer.activeDialogueEvents.Add(ACTIVECONSTRUCTION, 3);
				return true;
			}
			catch (Exception e)
            {
				Log.Trace($"RSV: Could not build {currentBuilding.name}:\n{e}");
				return false;
            }
			//return ((Farm)Game1.getLocationFromName("Farm")).buildStructure(this.CurrentBlueprint, new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64), Game1.player, this.magicalConstruction);
		}

		/*
		public void returnToCarpentryMenu()
		{
			LocationRequest locationRequest = Game1.getLocationRequest("Custom_Ridgeside_IanHouse");
			locationRequest.OnWarp += delegate
			{
				this.onFarm = false;
				Game1.player.viewingLocation.Value = null;
				this.resetBounds();
				this.upgrading = false;
				this.moving = false;
				this.painting = false;
				this.buildingToMove = null;
				this.freeze = false;
				Game1.displayHUD = true;
				Game1.viewportFreeze = false;
				Game1.viewport.Location = new Location(320, 1536);
				this.drawBG = true;
				this.demolishing = false;
				Game1.displayFarmer = true;
				if (Game1.options.SnappyMenus)
				{
					base.populateClickableComponentList();
					this.snapToDefaultClickableComponent();
				}
			};
			Game1.warpFarmer(locationRequest, Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.facingDirection);
		}

		public void returnToCarpentryMenuAfterSuccessfulBuild()
		{
			LocationRequest locationRequest = Game1.getLocationRequest(this.magicalConstruction ? "WizardHouse" : "ScienceHouse");
			locationRequest.OnWarp += delegate
			{
				Game1.displayHUD = true;
				Game1.player.viewingLocation.Value = null;
				Game1.viewportFreeze = false;
				Game1.viewport.Location = new Location(320, 1536);
				this.freeze = true;
				Game1.displayFarmer = true;
				this.seanConstructionMessage();
			};
			Game1.warpFarmer(locationRequest, Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.facingDirection);
		}
		*/

		public void seanConstructionMessage()
		{
			exitThisMenu();
			Game1.player.forceCanMove();
			NPC sean = Game1.getCharacterFromName("Sean");
			sean.CurrentDialogue.Clear();
			if (CurrentBlueprint.daysToConstruct <= 0)
			{
				sean.CurrentDialogue.Push(new Dialogue(Helper.Translation.Get("IanShop.Instant", new { project = buildingName }), sean));
			}
            else
            {
				sean.CurrentDialogue.Push(new Dialogue(Helper.Translation.Get("IanShop.Construction", new { project = buildingName }), sean));
			}
			Game1.drawDialogue(sean);
		}

		
		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return onFarm;
		}

		/*
		public void setUpForBuildingPlacement()
		{
			Game1.currentLocation.cleanupBeforePlayerExit();
			this.hoverText = "";
			Game1.currentLocation = Game1.getLocationFromName("Farm");
			Game1.player.viewingLocation.Value = "Farm";
			Game1.currentLocation.resetForPlayerEntry();
			Game1.globalFadeToClear();
			this.onFarm = true;
			this.cancelButton.bounds.X = Game1.uiViewport.Width - 128;
			this.cancelButton.bounds.Y = Game1.uiViewport.Height - 128;
			Game1.displayHUD = false;
			Game1.viewportFreeze = true;
			Game1.viewport.Location = new Location(3136, 320);
			Game1.panScreen(0, 0);
			this.drawBG = false;
			this.freeze = false;
			Game1.displayFarmer = false;
			if (!this.demolishing && this.CurrentBlueprint.nameOfBuildingToUpgrade != null && this.CurrentBlueprint.nameOfBuildingToUpgrade.Length > 0 && !this.moving && !this.painting)
			{
				this.upgrading = true;
			}
		}
		*/

		public override void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
		{
			resetBounds();
		}

		/*
		public virtual bool CanDemolishThis(Building building)
		{
			if (building == null)
			{
				return false;
			}
			if (this._demolishCheckBlueprint == null || this._demolishCheckBlueprint.name != building.buildingType.Value)
			{
				this._demolishCheckBlueprint = new BluePrint(building.buildingType);
			}
			if (this._demolishCheckBlueprint != null)
			{
				return this.CanDemolishThis(this._demolishCheckBlueprint);
			}
			return true;
		}

		public virtual bool CanDemolishThis(BluePrint blueprint)
		{
			if (blueprint.moneyRequired < 0)
			{
				return false;
			}
			return true;
		}
		*/

		public override void draw(SpriteBatch b)
		{
			if (drawBG)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
			}
			if (Game1.IsFading() || freeze)
			{
				return;
			}
			if (!onFarm)
			{
				base.draw(b);
				drawTextureBox(b, xPositionOnScreen - 96, yPositionOnScreen - 16, maxWidthOfBuildingViewer + 64, maxHeightOfBuildingViewer + 64, Color.White);
				b.Draw(buildingImage, new Vector2(xPositionOnScreen + maxWidthOfBuildingViewer / 2 - currentBuilding.tilesWidth * 64 / 2 - 64, yPositionOnScreen + maxHeightOfBuildingViewer / 2 - currentBuilding.sourceRectForMenuView.Height * 4 / 2), currentBuilding.sourceRectForMenuView, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1);
				//currentBuilding.drawInMenu(b, xPositionOnScreen + maxWidthOfBuildingViewer / 2 - currentBuilding.tilesWide.Value * 64 / 2 - 64, yPositionOnScreen + maxHeightOfBuildingViewer / 2 - currentBuilding.getSourceRectForMenu().Height * 4 / 2);
				/*if (this.CurrentBlueprint.isUpgrade())
				{
					this.upgradeIcon.draw(b);
				}*/
				string placeholder = " Deluxe  Barn   ";
				if (SpriteText.getWidthOfString(buildingName) >= SpriteText.getWidthOfString(placeholder))
				{
					placeholder = buildingName + " ";
				}
				SpriteText.drawStringWithScrollCenteredAt(b, buildingName, xPositionOnScreen + maxWidthOfBuildingViewer - spaceToClearSideBorder - 16 + 64 + (width - (maxWidthOfBuildingViewer + 128)) / 2, yPositionOnScreen, SpriteText.getWidthOfString(placeholder));
				int descriptionWidth = LocalizedContentManager.CurrentLanguageCode switch
				{
					LocalizedContentManager.LanguageCode.es => maxWidthOfDescription + 64 + ((CurrentBlueprint?.name == "Deluxe Barn") ? 96 : 0),
					LocalizedContentManager.LanguageCode.it => maxWidthOfDescription + 96,
					LocalizedContentManager.LanguageCode.fr => maxWidthOfDescription + 96 + ((CurrentBlueprint?.name == "Slime Hutch" || CurrentBlueprint?.name == "Deluxe Coop" || CurrentBlueprint?.name == "Deluxe Barn") ? 72 : 0),
					LocalizedContentManager.LanguageCode.ko => maxWidthOfDescription + 96 + ((CurrentBlueprint?.name == "Slime Hutch") ? 64 : ((CurrentBlueprint?.name == "Deluxe Coop") ? 96 : ((CurrentBlueprint?.name == "Deluxe Barn") ? 112 : ((CurrentBlueprint?.name == "Big Barn") ? 64 : 0)))),
					_ => maxWidthOfDescription + 64,
				};
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
					/*if (this.magicalConstruction)
					{
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", this.price), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f, ingredientsPosition.Y + 8f), Game1.textColor * 0.5f, 1f, -1f, -1, -1, 0.25f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", this.price), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 4f - 1f, ingredientsPosition.Y + 8f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0.25f);
					}*/
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 4f, ingredientsPosition.Y + 4f), (Game1.player.Money < price) ? Color.Red : Game1.textColor, 1f, -1f, -1, -1, 0.25f);
				}
				ingredientsPosition.X -= 16f;
				ingredientsPosition.Y -= 21f;
				foreach (Item i in ingredients)
				{
					ingredientsPosition.Y += 68f;
					i.drawInMenu(b, ingredientsPosition, 1f);
					bool hasItem = (!(i is StardewValley.Object) || Game1.player.hasItemInInventory((i as StardewValley.Object).ParentSheetIndex, i.Stack)) ? true : false;
					/*if (this.magicalConstruction)
					{
						Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 12f, ingredientsPosition.Y + 24f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, this.magicalConstruction ? 0f : 0.25f);
						Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 16f - 1f, ingredientsPosition.Y + 24f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, this.magicalConstruction ? 0f : 0.25f);
					}*/
					Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 16f, ingredientsPosition.Y + 20f), hasItem ? Game1.textColor : Color.Red, 1f, -1f, -1, -1, 0.25f);
				}
				backButton.draw(b);
				forwardButton.draw(b);
				okButton.draw(b, blueprints[currentBlueprintIndex].doesFarmerHaveEnoughResourcesToBuild() ? Color.White : (Color.Gray * 0.8f), 0.88f);
				/*
				this.demolishButton.draw(b, this.CanDemolishThis(this.blueprints[this.currentBlueprintIndex]) ? Color.White : (Color.Gray * 0.8f), 0.88f);
				this.moveButton.draw(b);
				this.paintButton.draw(b);
				*/
			}
			/*
			else
			{
				string message = "";
				message = (this.upgrading ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Upgrade", new BluePrint(this.CurrentBlueprint.nameOfBuildingToUpgrade).displayName) : (this.demolishing ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Demolish") : ((!this.painting) ? Game1.content.LoadString("Strings\\UI:Carpenter_ChooseLocation") : Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Paint"))));
				SpriteText.drawStringWithScrollBackground(b, message, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(message) / 2, 16);
				Game1.StartWorldDrawInUI(b);
				if (!this.upgrading && !this.demolishing && !this.moving && !this.painting)
				{
					Vector2 mousePositionTile2 = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
					for (int y4 = 0; y4 < this.CurrentBlueprint.tilesHeight; y4++)
					{
						for (int x3 = 0; x3 < this.CurrentBlueprint.tilesWidth; x3++)
						{
							int sheetIndex3 = this.CurrentBlueprint.getTileSheetIndexForStructurePlacementTile(x3, y4);
							Vector2 currentGlobalTilePosition3 = new Vector2(mousePositionTile2.X + (float)x3, mousePositionTile2.Y + (float)y4);
							if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(currentGlobalTilePosition3))
							{
								sheetIndex3++;
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition3 * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex3 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
						}
					}
					foreach (Point additionalPlacementTile in this.CurrentBlueprint.additionalPlacementTiles)
					{
						int x4 = additionalPlacementTile.X;
						int y3 = additionalPlacementTile.Y;
						int sheetIndex4 = this.CurrentBlueprint.getTileSheetIndexForStructurePlacementTile(x4, y3);
						Vector2 currentGlobalTilePosition4 = new Vector2(mousePositionTile2.X + (float)x4, mousePositionTile2.Y + (float)y3);
						if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(currentGlobalTilePosition4))
						{
							sheetIndex4++;
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition4 * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex4 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
					}
				}
				else if (!this.painting && this.moving && this.buildingToMove != null)
				{
					Vector2 mousePositionTile = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
					BuildableGameLocation bl = Game1.currentLocation as BuildableGameLocation;
					for (int y2 = 0; y2 < (int)this.buildingToMove.tilesHigh; y2++)
					{
						for (int x = 0; x < (int)this.buildingToMove.tilesWide; x++)
						{
							int sheetIndex = this.buildingToMove.getTileSheetIndexForStructurePlacementTile(x, y2);
							Vector2 currentGlobalTilePosition = new Vector2(mousePositionTile.X + (float)x, mousePositionTile.Y + (float)y2);
							bool occupiedByBuilding = bl.buildings.Contains(this.buildingToMove) && this.buildingToMove.occupiesTile(currentGlobalTilePosition);
							if (!bl.isBuildable(currentGlobalTilePosition) && !occupiedByBuilding)
							{
								sheetIndex++;
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
						}
					}
					foreach (Point additionalPlacementTile2 in this.buildingToMove.additionalPlacementTiles)
					{
						int x2 = additionalPlacementTile2.X;
						int y = additionalPlacementTile2.Y;
						int sheetIndex2 = this.buildingToMove.getTileSheetIndexForStructurePlacementTile(x2, y);
						Vector2 currentGlobalTilePosition2 = new Vector2(mousePositionTile.X + (float)x2, mousePositionTile.Y + (float)y);
						bool occupiedByBuilding2 = bl.buildings.Contains(this.buildingToMove) && this.buildingToMove.occupiesTile(currentGlobalTilePosition2);
						if (!bl.isBuildable(currentGlobalTilePosition2) && !occupiedByBuilding2)
						{
							sheetIndex2++;
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition2 * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex2 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
					}
				}
				Game1.EndWorldDrawInUI(b);
			}
			*/
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
