using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nitrox_PublixExtension.Core.Plugin.Config.Processors
{
    public abstract class GenericProcessor
    {
        public GenericProcessor() { }

        public abstract T Load<T>(string path) where T : new();
        public abstract void Save<T>(string path, T config);
    }
}
