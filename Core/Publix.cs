using HarmonyLib;
using Nitrox.Server.Subnautica;
using Nitrox_PublixExtension.Core.Commands;
using Nitrox_PublixExtension.Core.Events;
using Nitrox_PublixExtension.Core.Plugin;
using Nitrox_PublixExtension.Core.ReflectionWrappers;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer;
using NitroxServer.Communication;
using NitroxServer.ConsoleCommands.Processor;
using NitroxServer.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nitrox_PublixExtension.Core
{
    public class Publix
    {
        public static PlayerCommandProcessor playerCommandProcessor;

        protected static EventManager eventManager = null;
        protected static PluginManager pluginManager = null;

        protected static ReflectionWrappers.PlayerManager playerManager = null;

        public static NitroxServer.Communication.NitroxServer NitroxRawServer;

        protected static void Init()
        {
            eventManager = new EventManager();
            pluginManager = new PluginManager();

            playerCommandProcessor = new PlayerCommandProcessor(pluginManager.GetAllCommands().Append(new HelpCommandOverwrite()));

            //TODO: revamp this
            Program.serializedPluginMessager += (msg) => 
            {
                Log.Info($"Got Message: {msg}");
                if (msg == null)
                    return;

                if (msg == "Server Started")
                {
                    var NitroxServerInstance = Entry.ServerAssembly.GetType("NitroxServer.Server").GetProperty("Instance").GetGetMethod().Invoke(null, null);

                    RepeatingTask.RunInBackground(async () =>
                    {
                        NitroxRawServer = ((NitroxServer.Communication.NitroxServer)Entry.ServerAssembly.GetType("NitroxServer.Server")?.GetField("server", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(NitroxServerInstance));

                        if (NitroxRawServer != null)
                        {
                            playerManager = new ReflectionWrappers.PlayerManager(NitroxRawServer);

                            NitroxRawServer.OnPacketRecieved = (connection, packet) => { return eventManager.OnPacketCallback(connection, packet); };

                            return false;
                        }

                        return true;
                    }, 100); //every 100 ms
                }
            }; 

        }

        public static PluginManager getPluginManager()
        {
            return pluginManager; 
        }

        public static EventManager getEventManager()
        {
            return eventManager;
        }


        public static ReflectionWrappers.PlayerManager getPlayerManager()
        {
            return playerManager;
        }



    }
}
