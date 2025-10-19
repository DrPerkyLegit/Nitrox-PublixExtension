using Nitrox_PublixExtension.Core.Plugin.Attributes;
using NitroxModel.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nitrox_PublixExtension.Core.Plugin
{
    public class PluginLogger
    {
        protected BasePlugin Plugin;
        protected PluginInfoAttribute PluginInfo;

        public PluginLogger(BasePlugin plugin)
        {
            this.Plugin = plugin;
            this.PluginInfo = plugin.GetType().GetCustomAttribute<PluginInfoAttribute>();
        }

        public string FormatMessage(string message)
        {
            return $"[{this.PluginInfo.name}] {message}";
        }

        public void Info(string message)
        {
            Log.Info(FormatMessage(message));
        }

        public void Error(string message)
        {
            Log.Error(FormatMessage(message));
        }
    }
}
