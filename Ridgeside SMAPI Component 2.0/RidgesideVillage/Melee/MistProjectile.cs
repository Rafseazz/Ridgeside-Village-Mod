using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netcode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.BellsAndWhistles;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{
    public class MistProjectile : Projectile
    {
        public static Texture2D sprite;
        static IModHelper Helper;
        static IMonitor Monitor;

        public delegate void onCollisionBehavior(GameLocation location, int xPosition, int yPosition, Character who);
        public readonly NetInt damage = new NetInt();
        public NetInt debuff = new NetInt(-1);

        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;
            sprite = Helper.ModContent.Load<Texture2D>(PathUtilities.NormalizePath("assets/Poof.png"));
        }
        public MistProjectile()
        { }


        public MistProjectile(float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, GameLocation location = null, Character owner = null)
            : this()
        {
            this.damage.Value = 0;
            base.theOneWhoFiredMe.Set(location, owner);
            base.currentTileSheetIndex.Value = 0;
            base.bouncesLeft.Value = 0;
            base.tailLength.Value = 1;
            base.rotationVelocity.Value = rotationVelocity;
            base.xVelocity.Value = xVelocity;
            base.yVelocity.Value = yVelocity;
            base.position.Value = startingPosition;
        }
        public override void updatePosition(GameTime time)
        {
            base.position.X += xVelocity;
            base.position.Y += yVelocity;
        }

        public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
        {
            this.explosionAnimation(location);
        }

        public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location)
        {
            this.explosionAnimation(location);
        }

        public override void behaviorOnCollisionWithMineWall(int tileX, int tileY)
        {
        }

        public override void behaviorOnCollisionWithOther(GameLocation location)
        {
            this.explosionAnimation(location);
        }

        public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
        {
            this.explosionAnimation(location);
        }

        private void explosionAnimation(GameLocation location)
        {
            Multiplayer multiplayer = Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("TileSheets\\Animations", new Rectangle(192, 320, 64, 64), 60, 5, 1, base.position, flicker: false, flipped: false));
            base.destroyMe = true;
        }

        public static void explodeOnImpact(GameLocation location, int x, int y, Character who)
        {
        }

        public override void draw(SpriteBatch b)
        {
            float current_scale = 4f * this.localScale;
            b.Draw(sprite, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(0f, 0f - this.height.Value) + new Vector2(32f, 32f)), new Rectangle(0, 0, 16, 16), this.color.Value, this.rotation, new Vector2(8f, 8f), current_scale, SpriteEffects.None, (this.position.Y + 96f) / 10000f);
        }
    }
}