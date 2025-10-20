using Nitrox_PublixExtension.Core.Plugin.Config;

namespace Nitrox_PublixExtension.Core.Plugin
{
    public class BasePlugin
    {
        private PluginLogger _logger;
        private ConfigManager _configManager;

        public virtual void OnLoad() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public PluginLogger GetLogger()
        {
            if (_logger == null)
            {
                _logger = new PluginLogger(this);
            }

            return _logger;
        }

        public ConfigManager GetConfigManager()
        {
            if (_configManager == null)
            {
                _configManager = new ConfigManager(this);
            }

            return _configManager;
        }

    }
}
