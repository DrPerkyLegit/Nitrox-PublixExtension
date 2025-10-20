using Nitrox.Model.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nitrox_PublixExtension.Core.Plugin.Config.Processors
{
    public class JSONProcessor : GenericProcessor
    {
        public JSONProcessor() { }

        public override T Load<T>(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    T defaultConfig = new T();
                    Save(path, defaultConfig);
                    return defaultConfig;
                }

                string json = File.ReadAllText(path);
                var config = JsonSerializer.Deserialize<T>(json);
                return config ?? new T();
            }
            catch (Exception ex)
            {
                Log.Error($"Error While Loading Config From Path \"{path}\": \n{ex.StackTrace}");
                return new T();
            }

        }

        public override void Save<T>(string path, T config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed To Save Config To Path \"{path}\": \n{ex.StackTrace}");
            }
        }
    }
}
