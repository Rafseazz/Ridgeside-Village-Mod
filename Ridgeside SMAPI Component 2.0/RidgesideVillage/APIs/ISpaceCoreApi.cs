using System;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley;

namespace RidgesideVillage
{
    public interface ISpaceCoreApi
    {
        /// Must take (Event, GameLocation, GameTime, string[])

        void RegisterCustomProperty(Type declaringType, string name, Type propType, MethodInfo getter, MethodInfo setter);
    }

}