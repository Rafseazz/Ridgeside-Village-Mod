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
    public class BloomProjectile : Projectile
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
            sprite = Helper.Content.Load<Texture2D>("assets/Mistbloom.png");
        }
        public BloomProjectile()
        {}


        public BloomProjectile(int damage, int bounces, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, GameLocation location = null, Character owner = null)
            : this()
        {
            this.damage.Value = damage;
            this.damagesMonsters.Value = true;
            base.theOneWhoFiredMe.Set(location, owner);
            base.currentTileSheetIndex.Value = 0;
            base.bouncesLeft.Value = bounces;
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
            location.playSound("leafrustle");
            this.explosionAnimation(location);
        }

        public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location)
        {
            location.playSound("leafrustle");
            this.explosionAnimation(location);
        }

        public override void behaviorOnCollisionWithMineWall(int tileX, int tileY)
        {
        }

        public override void behaviorOnCollisionWithOther(GameLocation location)
        {
            location.playSound("leafrustle");
            this.explosionAnimation(location);
        }

        public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
        {
            location.playSound("coldSpell");
            this.explosionAnimation(location);
            if (n is Monster)
            {
                location.damageMonster(n.GetBoundingBox(), this.damage.Value, this.damage.Value, isBomb: false, (base.theOneWhoFiredMe.Get(location) is Farmer) ? (base.theOneWhoFiredMe.Get(location) as Farmer) : Game1.player);
                n.modData["RSV_bloomDebuff"] = "true";
            }
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
            if (this.height.Value > 0f)
            {
                b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(32f, 32f)), Game1.shadowTexture.Bounds, Color.White * 0.75f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 2f, SpriteEffects.None, (this.position.Y - 1f) / 10000f);
            }
        }
    }
}