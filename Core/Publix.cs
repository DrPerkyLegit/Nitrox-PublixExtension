using Nitrox.Server.Subnautica;
using Nitrox_PublixExtension.Core.Commands;
using Nitrox_PublixExtension.Core.Events;
using Nitrox_PublixExtension.Core.Plugin;
using Nitrox.Model.Logger;
using Nitrox.Model.Serialization;
using Nitrox.Server.Subnautica;
using Nitrox.Server.Subnautica.Models.Communication;
using System.Reflection;

namespace Nitrox_PublixExtension.Core
{
    public class Publix
    {
        public static PlayerCommandProcessor playerCommandProcessor;

        protected static EventManager eventManager = null;
        protected static PluginManager pluginManager = null;

        protected static ReflectionWrappers.PlayerManager playerManager = null;

        public static Server NitroxServerInstance;
        public static NitroxServer NitroxRawServer;

        protected static FieldInfo serverConfigField;

        protected static void Init()
        {
            eventManager = new EventManager();
            pluginManager = new PluginManager();

            playerCommandProcessor = new PlayerCommandProcessor(pluginManager.GetAllCommands().Append(new HelpCommandOverwrite()));

            //TODO: revamp this
            Server.OnSystemMessage = (msg) => 
            {
                if (msg == null)
                    return;

                if (msg[0] == '1') //server started
                {
                    NitroxServerInstance = ((Server)typeof(Server).GetProperty("Instance").GetGetMethod().Invoke(null, null));

                    serverConfigField = NitroxServerInstance.GetType().GetField("serverConfig", BindingFlags.Instance | BindingFlags.NonPublic);

                    RepeatingTask.RunInBackground(async () =>
                    {
                        NitroxRawServer = ((NitroxServer)Entry.ServerAssembly.GetType("NitroxServer.Server")?.GetField("server", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(NitroxServerInstance));

                        if (NitroxRawServer != null)
                        {

                            playerManager = new ReflectionWrappers.PlayerManager(NitroxRawServer);

                            Nitrox.Server.Subnautica.Server.OnPacketRecieved = (connection, packet) => { return eventManager.OnPacketCallback(connection, packet, EventManager.PacketType.Recieved); };
                            Server.OnPacketSentToPlayer = (connection, packet) => { return eventManager.OnPacketCallback(connection, packet, EventManager.PacketType.SentOne); };
                            Server.OnPacketSentToOtherPlayers = (connection, packet) => { return eventManager.OnPacketCallback(connection, packet, EventManager.PacketType.SentOthers); };
                            Server.OnPacketSentToAllPlayers = (packet) => { return eventManager.OnPacketCallback(null, packet, EventManager.PacketType.SentAll); };
                            
                            pluginManager.OnServerStarted();
                            return false;
                        }

                        return true;
                    }, 100); //every 100 ms
                }
                else if (msg[0] == '0') //server shutdown
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

        public static SubnauticaServerConfig GetSubnauticaServerConfig()
        {
            return (SubnauticaServerConfig)serverConfigField.GetValue(NitroxServerInstance);
        }


        public static ReflectionWrappers.PlayerManager getPlayerManager()
        {
            return playerManager;
        }



    }
}
