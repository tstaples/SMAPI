using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StardewModdingAPI.Helpers
{
    public static class StardewAssembly
    {
        private static Assembly ModifiedGameAssembly { get; set; }
        private static CecilContext StardewContext { get; set; }
        private static CecilContext SmapiContext { get; set; }

        public static void ModifyStardewAssembly()
        {
            StardewContext = new CecilContext(CecilContextType.Stardew);
            SmapiContext = new CecilContext(CecilContextType.SMAPI);

            CecilHelper.InjectEntryMethod(StardewContext, SmapiContext, "StardewValley.Game1", ".ctor", "StardewModdingAPI.Program", "Test");
            CecilHelper.InjectExitMethod(StardewContext, SmapiContext, "StardewValley.Game1", "Initialize", "StardewModdingAPI.Events.GameEvents", "InvokeInitialize");
            CecilHelper.InjectExitMethod(StardewContext, SmapiContext, "StardewValley.Game1", "LoadContent", "StardewModdingAPI.Events.GameEvents", "InvokeLoadContent");
            CecilHelper.InjectExitMethod(StardewContext, SmapiContext, "StardewValley.Game1", "Update", "StardewModdingAPI.Events.GameEvents", "InvokeUpdateTick");
            CecilHelper.InjectExitMethod(StardewContext, SmapiContext, "StardewValley.Game1", "Draw", "StardewModdingAPI.Events.GraphicsEvents", "InvokeDrawTick");
        }

        public static void LoadStardewAssembly()
        {
            ModifiedGameAssembly = Assembly.Load(StardewContext.ModifiedAssembly.GetBuffer());
            //ModifiedGameAssembly = Assembly.UnsafeLoadFrom(Constants.ExecutionPath + "\\Stardew Valley.exe");
        }

        internal static void Launch()
        {
            ModifiedGameAssembly.EntryPoint.Invoke(null, new object[] { new string[] { } });
        }

        internal static void WriteModifiedExe()
        {
            StardewContext.WriteAssembly("StardewValley-Modified.exe");
        }
    }
}
