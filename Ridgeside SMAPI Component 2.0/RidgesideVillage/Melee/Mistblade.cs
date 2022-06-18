using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Network;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using StardewValley.Tools;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{
    internal static class Mistblade
    {
        const string MISTBLADENAME = "Mountain Mistblade";
        public static Random myRand;

        static IModHelper Helper;
        static IMonitor Monitor;

        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;
            //myRand = new Random();

            Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            Farmer user = Game1.player;
            if (user.CurrentTool?.Name is not MISTBLADENAME)
            {
                return;
            }
            if ((!e.Button.IsActionButton()) || (MeleeWeapon.defenseCooldown > 0))
            {
                return;
            }
            float rotation = (float)Math.PI / 16f;
            int bounces = 0;

            Game1.currentLocation.playSound("throw");

            float angle = 30 * MathF.PI / 360;
            Vector2 velocity1 = TranslateVector(new Vector2(0, 10), user.FacingDirection);
            Vector2 startPos1 = TranslateVector(new Vector2(0, 96), user.FacingDirection);
            //int damage1 = myRand.Next(5, 15);
            Game1.currentLocation.projectiles.Add(new BloomProjectile(-1, bounces, rotation, velocity1.X, velocity1.Y, user.Position + new Vector2(0, -64) + startPos1, user.currentLocation, user));

            Vector2 velocity2 = TranslateVector(new Vector2(-10 * MathF.Sin(angle), 10 * MathF.Cos(angle)), user.FacingDirection);
            Vector2 startPos2 = TranslateVector(new Vector2(-72 * MathF.Sin(angle), 72 * MathF.Cos(angle)), user.FacingDirection);
            //int damage2 = myRand.Next(5, 15);
            Game1.currentLocation.projectiles.Add(new BloomProjectile(-1, bounces, rotation, velocity2.X, velocity2.Y, user.Position + new Vector2(0, -64) + startPos2, user.currentLocation, user));

            Vector2 velocity3 = TranslateVector(new Vector2(10 * MathF.Sin(angle), 10 * MathF.Cos(angle)), user.FacingDirection);
            Vector2 startPos3 = TranslateVector(new Vector2(72 * MathF.Sin(angle), 72 * MathF.Cos(angle)), user.FacingDirection);
            //int damage3 = myRand.Next(5, 15);
            Game1.currentLocation.projectiles.Add(new BloomProjectile(-1, bounces, rotation, velocity3.X, velocity3.Y, user.Position + new Vector2(0, -64) + startPos3, user.currentLocation, user));
        }

        public static Vector2 TranslateVector(Vector2 vector, int facingDirection)
        {
            float outx = vector.X;
            float outy = vector.Y;
            switch (facingDirection)
            {
                case 2:
                    break;
                case 3:
                    outx = -vector.Y;
                    outy = vector.X;
                    break;
                case 0:
                    outx = -vector.X;
                    outy = -vector.Y;
                    break;
                case 1:
                    outx = vector.Y;
                    outy = -vector.X;
                    break;
            }
            return new Vector2(outx, outy);
        }
    }

}
