
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;

namespace RidgesideVillage
{
    public interface ICustomCompanionsApi
    {
        void ReloadContentPack(string packUniqueId);
    }
}