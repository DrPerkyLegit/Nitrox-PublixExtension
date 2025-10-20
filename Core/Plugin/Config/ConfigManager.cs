using Nitrox.Model.Logger;
using Nitrox_PublixExtension.Core.Plugin.Attributes;
using Nitrox_PublixExtension.Core.Plugin.Config.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nitrox_PublixExtension.Core.Plugin.Config
{
    public class ConfigManager
    {
        private static Dictionary<ConfigType, GenericProcessor> configProcessors = new Dictionary<ConfigType, GenericProcessor>()
        {
            { ConfigType.JSON, new JSONProcessor() }
        };

        private static Dictionary<ConfigType, string> configFileTypes = new Dictionary<ConfigType, string>()
        {
            { ConfigType.JSON, ".json" }
        };

        protected BasePlugin Plugin;
        protected PluginInfoAttribute PluginInfo;

        public ConfigManager(BasePlugin plugin)
        {
            this.Plugin = plugin;
            this.PluginInfo = plugin.GetType().GetCustomAttribute<PluginInfoAttribute>();
        }

        public T GetConfig<T>(string path = "config", ConfigType type = ConfigType.JSON) where T : new()
        {
            if (configProcessors.TryGetValue(type, out GenericProcessor value))
            {
                string configPath = Path.Combine(AppContext.BaseDirectory, "plugin_data");
                if (!Directory.Exists(configPath)) Directory.CreateDirectory(configPath);

                configPath = Path.Combine(configPath, PluginInfo.package);
                if (!Directory.Exists(configPath)) Directory.CreateDirectory(configPath);


                return value.Load<T>(Path.Combine(configPath, path + configFileTypes[type]));
            }
            else
            {
                Log.Error($"Unable To Get Config: ConfigType Missing Processor \"{type}\"");
                return new T();
            }
        }

        public void SaveConfig<T>(T config, string path = "config", ConfigType type = ConfigType.JSON)
        {
            if (configProcessors.TryGetValue(type, out GenericProcessor value))
            {
                string configPath = Path.Combine(AppContext.BaseDirectory, "plugin_data");
                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);

                configPath = Path.Combine(configPath, PluginInfo.package);
                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);


                value.Save<T>(Path.Combine(configPath, path + configFileTypes[type]), config);
            }
            else
            {
                Log.Error($"Unable To Get ConfigProcessor: ConfigType Missing Processor \"{type}\"");
            }
        }
    }
}
