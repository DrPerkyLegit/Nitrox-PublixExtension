using Nitrox.Server.Subnautica;
using Nitrox_PublixExtension.Core.Commands;
using Nitrox_PublixExtension.Core.Events;
using Nitrox_PublixExtension.Core.Plugin;
using NitroxModel.Logger;
using NitroxServer;
using System.Reflection;

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
                if (msg == null)
                    return;

                if (msg == "1") //server started
                {
                    Server NitroxServerInstance = ((Server)Entry.ServerAssembly.GetType("NitroxServer.Server").GetProperty("Instance").GetGetMethod().Invoke(null, null));

                    RepeatingTask.RunInBackground(async () =>
                    {
                        NitroxRawServer = ((NitroxServer.Communication.NitroxServer)Entry.ServerAssembly.GetType("NitroxServer.Server")?.GetField("server", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(NitroxServerInstance));

                        if (NitroxRawServer != null)
                        {

                            playerManager = new ReflectionWrappers.PlayerManager(NitroxRawServer);

                            Server.OnPacketRecieved = (connection, packet) => { return eventManager.OnPacketCallback(connection, packet, EventManager.PacketType.Recieved); };
                            Server.OnPacketSent = (connection, packet) => { return eventManager.OnPacketCallback(connection, packet, EventManager.PacketType.Sent); };

                            return false;
                        }

                        return true;
                    }, 100); //every 100 ms
                }
                else if (msg == "0") //server shutdown
                {
                    pluginManager.OnServerShutdown();
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
