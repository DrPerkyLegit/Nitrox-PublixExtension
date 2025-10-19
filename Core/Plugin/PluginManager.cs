using Nitrox_PublixExtension.Core.Plugin.Attributes;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nitrox_PublixExtension.Core.Plugin
{
    public class PluginManager
    {
        protected Dictionary<PluginInfoAttribute, BasePlugin> pluginDict = new Dictionary<PluginInfoAttribute, BasePlugin>();
        protected Dictionary<BasePlugin, List<Command>> pluginCommands = new Dictionary<BasePlugin, List<Command>>();
        public PluginManager()
        {
            LoadPlugins();

            RegisterPluginCommands();
        }

        public IEnumerable<Command> GetAllCommands()
        {
            List<Command> commands = new List<Command>();

            foreach (var pluginPair in pluginCommands)
            {
                foreach (Command command in pluginPair.Value)
                {
                    commands.Add(command);
                }
            }

            return commands.AsEnumerable();
        }

        protected void RegisterPluginCommands()
        {
            Type CommandType = typeof(Command);

            foreach (KeyValuePair<PluginInfoAttribute, BasePlugin> keyPair in pluginDict)
            {
                List<Command> commands = new List<Command>();

                foreach (Type type in keyPair.Value.GetType().Assembly.GetTypes())
                {
                    if (type.IsSubclassOf(CommandType))
                    {
                        commands.Add((Command)Activator.CreateInstance(type));
                    }
                }

                pluginCommands.Add(keyPair.Value, commands);
            }
        }


        protected void LoadPlugins()
        {
            string PluginsDir = Path.Combine(AppContext.BaseDirectory, "plugins");
            Directory.CreateDirectory(PluginsDir);

            Type BasePluginType = typeof(BasePlugin);

            foreach (var dll in Directory.GetFiles(PluginsDir, "*.dll"))
            {
                try
                {
                    Assembly PluginAssembly = Assembly.LoadFrom(dll);

                    foreach (Type type in PluginAssembly.GetTypes())
                    {
                        if (type.IsSubclassOf(BasePluginType))
                        {
                            PluginInfoAttribute infoAttribute = type.GetCustomAttribute<PluginInfoAttribute>();

                            if (infoAttribute == null)
                            {
                                Log.Error($"Tried To Load Plugin At \"{dll}\" But It Was Missing PluginInfoAttribute");
                                continue;
                            }

                            BasePlugin PluginInstance = (BasePlugin)Activator.CreateInstance(type);
                            pluginDict.Add(infoAttribute, PluginInstance);

                            PluginInstance.OnEnable();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed To Load Plugin At \"{dll}\" With Exception: {ex}");
                }
            }
        }
    }
}
