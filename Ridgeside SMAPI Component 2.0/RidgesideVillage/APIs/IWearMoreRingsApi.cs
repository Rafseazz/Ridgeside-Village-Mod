
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;

namespace RidgesideVillage
{
    public interface IWearMoreRingsApi
    {
        /// <summary>
        /// Count how many of the specified ring type the given player has equipped. This includes the vanilla left & right rings.
        /// Furthermore, it includes the rings contained in combined rings, but excludes the combined rings itself.
        /// </summary>
        /// <returns>How many of the specified ring the given player has equipped.</returns>
        /// <param name="f">The farmer/farmhand whose inventory is being checked. For the local player, use Game1.player.</param>
        /// <param name="which">The parentSheetIndex of the ring.</param>
        int CountEquippedRings(StardewValley.Farmer f, int which);

        /// <summary>
        /// Returns a list of all rings the player has equipped. This includes the vanilla left & right rings.
        /// Furthermore, it includes the rings contained in combined rings, but excludes the combined rings itself.
        /// </summary>
        /// <returns>A list of all equiped rings.</returns>
        /// <param name="f">The farmer/farmhand whose inventory is being checked. For the local player, use Game1.player.</param>
        System.Collections.Generic.IEnumerable<StardewValley.Objects.Ring> GetAllRings(StardewValley.Farmer f);
    }
}