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
        string[] GetCustomSkills();
        int GetLevelForCustomSkill(Farmer farmer, string skill);
        void AddExperienceForCustomSkill(Farmer farmer, string skill, int amt);
        int GetProfessionId(string skill, string profession);

        /// Must take (Event, GameLocation, GameTime, string[])
        void AddEventCommand(string command, MethodInfo info);

        /// Must have [XmlType("Mods_SOMETHINGHERE")] attribute (required to start with "Mods_")
        void RegisterSerializerType(Type type);

        void RegisterCustomProperty(Type declaringType, string name, Type propType, MethodInfo getter, MethodInfo setter);

        void RegisterCustomLocationContext(string name, Func<Random, LocationWeather> getLocationWeatherForTomorrowFunc/*, Func<Farmer, string> passoutWakeupLocationFunc, Func<Farmer, Point?> passoutWakeupPointFunc*/ );
    }

}