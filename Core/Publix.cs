using Nitrox_PublixExtension.Core.Commands;
using Nitrox_PublixExtension.Core.Events;
using Nitrox_PublixExtension.Core.Plugin;
using NitroxModel.Serialization;
using System.Reflection;

namespace Nitrox_PublixExtension.Core
{
    public class Publix
    {
        public static PlayerCommandProcessor playerCommandProcessor;

        protected static EventManager eventManager = null;
        protected static PluginManager pluginManager = null;

        protected static ReflectionWrappers.PlayerManager playerManager = null;

        public static NitroxServer.Server NitroxServerInstance;
        public static NitroxServer.Communication.NitroxServer NitroxRawServer;

        protected static FieldInfo serverConfigField;

        protected static void Init()
        {
            eventManager = new EventManager();
            pluginManager = new PluginManager();

            playerCommandProcessor = new PlayerCommandProcessor(pluginManager.GetAllCommands().Append(new HelpCommandOverwrite()));

            //TODO: revamp this
            NitroxServer.Server.OnSystemMessage = (msg) => 
            {
                if (msg == null)
                    return;

                if (msg[0] == '1') //server started
                {
                    NitroxServerInstance = ((NitroxServer.Server)typeof(NitroxServer.Server).GetProperty("Instance").GetGetMethod().Invoke(null, null));

                    serverConfigField = NitroxServerInstance.GetType().GetField("serverConfig", BindingFlags.Instance | BindingFlags.NonPublic);

                    RepeatingTask.RunInBackground(async () =>
                    {
                        NitroxRawServer = ((NitroxServer.Communication.NitroxServer)typeof(NitroxServer.Server).GetField("server", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(NitroxServerInstance));

                        if (NitroxRawServer != null)
                        {

                            playerManager = new ReflectionWrappers.PlayerManager(NitroxRawServer);

                            NitroxServer.Server.OnPacketRecieved = (connection, packet) => { return eventManager.OnPacketCallback(connection, packet, EventManager.PacketType.Recieved); };
                            NitroxServer.Server.OnPacketSentToPlayer = (connection, packet) => { return eventManager.OnPacketCallback(connection, packet, EventManager.PacketType.SentOne); };
                            NitroxServer.Server.OnPacketSentToOtherPlayers = (connection, packet) => { return eventManager.OnPacketCallback(connection, packet, EventManager.PacketType.SentOthers); };
                            NitroxServer.Server.OnPacketSentToAllPlayers = (packet) => { return eventManager.OnPacketCallback(null, packet, EventManager.PacketType.SentAll); };
                            
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
