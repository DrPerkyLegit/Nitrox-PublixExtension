namespace Nitrox_PublixExtension.Core.Plugin.Attributes
{
    public class PluginDescriptionAttribute : Attribute
    {
        public readonly string description;

        public PluginDescriptionAttribute(string description)
        {
            this.description = description;
        }
    }
}
