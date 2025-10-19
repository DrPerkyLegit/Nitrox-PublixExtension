using NitroxModel.Logger;
using System.Reflection;

namespace Nitrox_PublixExtension
{
    internal class Entry
    {
        public static readonly string _version = "1.0.0"; //Publix version
        public static readonly string _supported = "1.8.0.0"; //Supported Nitrox version

        public static Assembly ServerAssembly = null;

        public static void Init()
        {
            foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (item.GetName().Name == "NitroxServer")
                {
                    ServerAssembly = item;
                    break;
                }
            }

            if (ServerAssembly == null) return; //this is just a 2fa step to verify we are running from a nitrox server

            Log.Info($"Loading Nitrox-PublixExtension V{_version}");

            if (ServerAssembly?.GetName().Version?.ToString() != _supported)
            {
                Log.Error($"Trying To Load Nitrox-PublixExtension V{_version} With Nitrox Version \"{ServerAssembly?.GetName().Version}\" But This Version Only Supports Nitrox Version \"{_supported}\", Continuing But Expect Errors Or Crashing");
            }

            //little work around to calling a protected function so its not exposed to end user
            Assembly.GetAssembly(typeof(Entry))?.GetType("Nitrox_PublixExtension.Core.Publix")?.GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null);
        }
    }
}
