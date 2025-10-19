using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
