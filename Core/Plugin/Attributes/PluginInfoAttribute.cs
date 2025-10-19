namespace Nitrox_PublixExtension.Core.Plugin.Attributes
{
    public class PluginInfoAttribute : Attribute
    {
        public readonly string package;
        public readonly string name;
        public readonly string version;
        public readonly string publixVersion;
        public readonly string nitroxVersion;

        public PluginInfoAttribute(string package, string name, string version, string publixVersion)
        {
            this.package = package;
            this.name = name;
            this.version = version;
            this.publixVersion = publixVersion;
        }
    }
}
