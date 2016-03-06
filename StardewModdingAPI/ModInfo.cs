using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public string Texture { get; set; }
        public bool IsPassable { get; set; }
        public bool IsPlacable { get; set; }
    }

    class ModInfo
    {
        public string Name { get; set; }
        public string ModDLL { get; set; }
        public List<ModItemDesc> Items { get; set; }

        [JsonIgnore]
        public string ModRoot { get; set; }

        [JsonIgnore]
        public Mod ModInstance { get; set; }

        [JsonIgnore]
        public bool HasDLL { get { return !string.IsNullOrWhiteSpace(ModDLL); } }
        
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

    }
}
