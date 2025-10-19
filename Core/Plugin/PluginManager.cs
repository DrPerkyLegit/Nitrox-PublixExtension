using Nitrox_PublixExtension.Core.Plugin.Attributes;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using System.Reflection;
using System.Runtime.InteropServices;

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

            Span<string> publixVersionSplit = Entry._version.Split(".", StringSplitOptions.RemoveEmptyEntries);

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

                            if (!infoAttribute.publixVersion.Contains("."))
                            {
                                Log.Error($"Tried To Load Plugin At \"{dll}\" But It Had Invalid Version Format, Must Be Like \"{Entry._version}\" Not \"{infoAttribute.publixVersion}\"");
                                continue;
                            }

                            Span<string> pluginVersionSplit = infoAttribute.publixVersion.Split(".", StringSplitOptions.RemoveEmptyEntries);

                            if (pluginVersionSplit.Length != 3)
                            {
                                Log.Error($"Tried To Load Plugin At \"{dll}\" But It Had Invalid Version Format, Must Be Like \"1.0.0\" Not \"{infoAttribute.publixVersion}\"");
                                continue;
                            }

                            if (pluginVersionSplit[0] != publixVersionSplit[0])
                            {
                                Log.Error($"Tried To Load Plugin At \"{dll}\" But It Uses Incorrect Major Version \"{pluginVersionSplit[0]}\"");
                                continue;
                            }

                            if (pluginVersionSplit[1] != publixVersionSplit[1])
                            {
                                Log.Error($"Tried To Load Plugin At \"{dll}\" But It Uses Incorrect Minor Version \"{pluginVersionSplit[1]}\", Continuing But Expect Issues");
                            }

                            //add better version checking, this is just basic for now

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
