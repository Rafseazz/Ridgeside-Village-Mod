using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;


namespace RidgesideVillage
{
    public class Dependency
    {
        public string url;
        public string author;
        public string name;
        public string uniqueID;
        public bool required;
#nullable enable
        public string? minVersion;
        public string? parents;
#nullable disable
    }

    internal class InstallationChecker
    {
        IModHelper helper;

        static string BOLDLINE = new string('#', 75);
        static string THINLINE = new string('-', 75);
        static string BULLET = "\t \x1A ";
        static string INDENT = "\t" + new string(' ', 5);

        bool isInstalledCorrectly = true;
        bool hasAllDependencies = true;
        List<Dependency> missing_dependencies = new List<Dependency>();
        List<Dependency> outdated_dependencies = new List<Dependency>();
        List<Dependency> missing_parents = new List<Dependency>();
        private List<string> outdatedRSVComponents = new();

        public bool checkInstallation(IModHelper Helper)
        {
            helper = Helper;
            var dependencies = helper.Data.ReadJsonFile<Dictionary<string, Dependency>>(PathUtilities.NormalizePath("assets/Dependencies.json"));
            if (dependencies is null)
            {
                Log.Error("Couldn't find dependency file");
                return false;
            }
            Log.Trace($"Number of dependencies to check: {dependencies.Values.Count}");
            foreach (var dependency in dependencies.Values)
            {
                Log.Trace($"InstallationChecker checking {dependency.name}...");
                if (dependency.name == "SMAPI")
                {
                    Log.Trace($"SMAPI is out of date: {Constants.ApiVersion.IsOlderThan(dependency.minVersion)}");
                    if (Constants.ApiVersion.IsOlderThan(dependency.minVersion))
                        outdated_dependencies.Add(dependency);
                }
                else if (dependency.required && !helper.ModRegistry.IsLoaded(dependency.uniqueID))
                {
                    Log.Trace($"{dependency.name} is missing.");
                    if (dependency.parents == null)  // no parent dependencies
                        missing_dependencies.Add(dependency);
                    else if (dependency.parents != null && !TheseModsLoaded(dependency.parents)) // not loaded bc missing parent dependencies
                        missing_parents.Add(dependency);
                    else // has parent dependencies but they're loaded
                        missing_dependencies.Add(dependency);
                    
                }
                else
                {
                    if (dependency.minVersion == null)
                        continue;
                    var mod = helper.ModRegistry.Get(dependency.uniqueID);
                    if (mod == null)
                        continue;
                    Log.Trace($"{dependency.name}: Local version ({mod.Manifest.Version.ToString()}) is older than required version ({dependency.minVersion}): {mod.Manifest.Version.IsOlderThan(dependency.minVersion)}");
                    if (mod.Manifest.Version.IsOlderThan(dependency.minVersion))
                        outdated_dependencies.Add(dependency);
                }
                Log.Trace($"{dependency.name} is loaded and up to date.");
            }

            Log.Trace($"Number of missing mods: {missing_dependencies.Count}");
            Log.Trace($"Number of out of date mods: {outdated_dependencies.Count}");

            if (outdated_dependencies.Any() || missing_dependencies.Any())
                hasAllDependencies = false;

            if (hasAllDependencies && !helper.ModRegistry.IsLoaded("Rafseazz.RSVCP"))
                isInstalledCorrectly = false;

            string[] oldMods = new[] { "Rafseazz.RSVJA", "Rafseazz.RSVSAAT", "Rafseazz.RSVSTF", "Rafseazz.RSVMFM" };

            foreach (var mod in oldMods)
            {
                if (helper.ModRegistry.IsLoaded(mod))
                {
                    outdatedRSVComponents.Add(mod);
                }
            }

            if (!isInstalledCorrectly || !hasAllDependencies || outdatedRSVComponents.Count != 0)
            {
                helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            }

            return isInstalledCorrectly && hasAllDependencies && outdatedRSVComponents.Count == 0;
        }

        public bool TheseModsLoaded(string mods)
        {
            string[] reqs = mods.Split(",");
            foreach(string req in reqs)
            {
                if (missing_dependencies.FindAll(d => d.name == req.Trim()).Any() || outdated_dependencies.FindAll(d => d.name == req.Trim()).Any())
                {
                    return false;
                }
            }
            return true;
        }


        [EventPriority(EventPriority.Low)]
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            for(int i=0; i<3; i++)
            {
                Log.Error(BOLDLINE);
            }

            Log.Error("");
            Log.Error(helper.Translation.Get("installation.incorrect"));

            if (missing_parents.Any())
            {
                Log.Error("");
                Log.Error(THINLINE);
                Log.Error("");
                Log.Error(helper.Translation.Get("orphaned.mods"));
                Log.Error(helper.Translation.Get("orphaned.mods.cont"));
                Log.Error("");
                foreach (var dependency in missing_parents)
                {
                    Log.Error(BULLET + helper.Translation.Get("mod.info", new { modName = dependency.name, author = dependency.author }));
                    Log.Error(INDENT + helper.Translation.Get("mod.parents", new { parents = dependency.parents }));
                }
            }
            if (missing_dependencies.Any())
            {
                Log.Error("");
                Log.Error(THINLINE);
                Log.Error("");
                Log.Error(helper.Translation.Get("missing.mods"));
                Log.Error(helper.Translation.Get("missing.mods.cont"));
                Log.Error("");
                foreach (var dependency in missing_dependencies)
                {
                    Log.Error(BULLET + helper.Translation.Get("mod.info", new { modName = dependency.name, author = dependency.author}));
                    Log.Error(INDENT + dependency.url);
                }
            }
            if (outdated_dependencies.Any())
            {
                Log.Error("");
                Log.Error(THINLINE);
                Log.Error("");
                Log.Error(helper.Translation.Get("outdated.mods"));
                Log.Error(helper.Translation.Get("outdated.mods.cont"));
                Log.Error("");
                foreach (var dependency in outdated_dependencies)
                {
                    if (dependency.name == "SMAPI")
                        Log.Error(BULLET + helper.Translation.Get("smapi.outdated"));
                    else
                        Log.Error(BULLET + helper.Translation.Get("mod.info", new { modName = dependency.name, author = dependency.author}));
                        Log.Error(INDENT + dependency.url);
                }
            }

            if (this.outdatedRSVComponents.Any())
            {
                Log.Error("");
                foreach(var modComponent in outdatedRSVComponents)
                {
                    Log.Error($"The oudated RSV component {modComponent} is still installed. Remove it and restart the game.");
                }
            }
            if (!isInstalledCorrectly)
            {
                Log.Error("");
                Log.Error(THINLINE);
                Log.Error("");
                Log.Error(helper.Translation.Get("ridgeside.CP.missing"));
            }
            Log.Error("");
            Log.Error(THINLINE);
            Log.Error("");
            Log.Error(helper.Translation.Get("help.message"));
            Log.Error("");

            for (int i = 0; i < 3; i++)
            {
                Log.Error(BOLDLINE);
            }

            Game1.activeClickableMenu =
                new DialogueBox(helper.Translation.Get("ErrorMessage"));
        }
        
    }

}
