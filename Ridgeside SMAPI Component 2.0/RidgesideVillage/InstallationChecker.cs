using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;

namespace RidgesideVillage
{
    internal class InstallationChecker
    {
        bool isInstalledCorrectly = true;
        IModHelper helper;
        List<Dependency> missing_dependencies = new List<Dependency>();
        public bool checkInstallation(IModHelper helper)
        {
            this.helper = helper;
            var dependencies = helper.Data.ReadJsonFile<Dictionary<string, Dependency>>(PathUtilities.NormalizePath("assets/Dependencies.json"));
            
            foreach(var dependency in dependencies.Values)
            {
                if (!helper.ModRegistry.IsLoaded(dependency.uniqueID))
                {
                    this.missing_dependencies.Add(dependency);
                }
            }

            if(this.missing_dependencies.Count > 0 || !helper.ModRegistry.IsLoaded("Rafseazz.RSVCP"))
            {
                this.isInstalledCorrectly = false;
            }

            if (!this.isInstalledCorrectly)
            {
                helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            }
            return this.isInstalledCorrectly;
        }

        
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            for(int i=0; i<3; i++)
            {
                Log.Error("########################################################################################");
            }

            Log.Error("");
            Log.Error(helper.Translation.Get("installation.incorrect"));
            Log.Error("");

            if (this.missing_dependencies.Count > 0)
            {
                Log.Error(this.helper.Translation.Get("missing.mods"));
                Log.Error("");
                foreach (var dependency in this.missing_dependencies)
                {
                    Log.Error(this.helper.Translation.Get("mod.missing", new { modName = dependency.name, author = dependency.author, link = dependency.url }));
                }
            }
            else
            { 
                Log.Error(helper.Translation.Get("ridgeside.CP.missing"));
            }
            Log.Error("");
            Log.Error(ModEntry.Helper.Translation.Get("help.message"));
            Log.Error("");

            for (int i = 0; i < 3; i++)
            {
                Log.Error("########################################################################################");
            }
        }

        
    }

    public class Dependency
    {
        public string url;
        public string author;
        public string name;
        public string uniqueID;
    }
}
