using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI.Inheritance;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StardewModdingAPI
{
    class ModItemDesc
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Texture { get; set; }
        public bool IsPassable { get; set; }
        public bool IsPlacable { get; set; }
    }

    class ModInfo
    {
        public string Name { get; set; }
        public string ModDLL { get; set; }
        public string ConfigurationFile { get; set; }
        public List<ModItemDesc> Items { get; set; }

        [JsonIgnore]
        private object ConfigurationSettings { get; set;
        }
        [JsonIgnore]
        public string ModRoot { get; set; }

        [JsonIgnore]
        public Mod ModInstance { get; set; }

        [JsonIgnore]
        public List<SObject> ItemInstances { get; set; }

        [JsonIgnore]
        public bool HasDLL { get { return !string.IsNullOrWhiteSpace(ModDLL); } }
        [JsonIgnore]
        public bool HasConfig { get { return HasDLL && !string.IsNullOrWhiteSpace(ConfigurationFile); } }

        [JsonIgnore]
        public bool HasContent { get { return Items.Any(); } }
        
        public bool LoadModDLL()
        {
            if(ModInstance != null)
            {
                Log.Error("Error! Mod has already been loaded!");
            }

            ModInstance = null;

            var modDllPath = ModRoot + "\\" + ModDLL;

            if (!modDllPath.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
            {
                modDllPath += ".dll";
            }

            try
            {
                Assembly modAssembly = Assembly.UnsafeLoadFrom(modDllPath); //to combat internet-downloaded DLLs

                if (modAssembly.DefinedTypes.Count(x => x.BaseType == typeof(Mod)) > 0)
                {
                    TypeInfo tar = modAssembly.DefinedTypes.First(x => x.BaseType == typeof(Mod));
                    ModInstance = (Mod)modAssembly.CreateInstance(tar.ToString());
                    ModInstance.Entry();
                }
                else
                {
                    Log.Error("Invalid Mod DLL");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to load mod '{0}'. Exception details:\n" + ex, modDllPath);
            }

            return ModInstance != null;
        }

        public bool LoadContent()
        {
            bool succeeded = true;
            foreach(var item in Items)
            {
                SObject so = new SObject();
                so.Name = item.Name;
                so.CategoryName = Name + " - " + item.Category;
                so.Description = item.Description;
                so.Texture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, new FileStream(ModRoot + "\\Content\\" + item.Texture, FileMode.Open));
                so.IsPassable = item.IsPassable;
                so.IsPlaceable = item.IsPlacable;
                StardewModdingAPI.Log.Verbose("Registering new item for {0}: {1}, ID {2}", Name, item.Name, SGame.RegisterModItem(so));
            }
            return succeeded;
        }

        public T GetConfigurationSettings<T>()
        {
            if(HasConfig)
            {
                return (T)ConfigurationSettings;
            }
            else
            {
                return default(T);
            }
        }
    }
}
