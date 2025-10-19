using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nitrox_PublixExtension.Core.Plugin.Attributes
{
    public class PluginInfoAttribute : Attribute
    {
        public readonly string package;
        public readonly string name;
        public readonly string version;

        public PluginInfoAttribute(string package, string name, string version)
        {
            this.package = package;
            this.name = name;
            this.version = version;
        }
    }
}
