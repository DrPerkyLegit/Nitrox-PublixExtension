namespace Nitrox_PublixExtension.Core.Plugin
{
    public class BasePlugin
    {
        private PluginLogger _logger;

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

    }
}
