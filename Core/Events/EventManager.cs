using Nitrox_PublixExtension.Core.Events.Attributes;
using Nitrox_PublixExtension.Core.Events.Base;
using Nitrox_PublixExtension.Core.Plugin;
using NitroxModel.Packets;
using NitroxServer;
using NitroxServer.Communication;
using System.Reflection;

namespace Nitrox_PublixExtension.Core.Events
{
    public class EventManager
    {
        public Dictionary<BasePlugin, Dictionary<EventListener, List<KeyValuePair<MethodInfo, ListenerMethodAttribute>>>> registeredListeners = new Dictionary<BasePlugin, Dictionary<EventListener, List<KeyValuePair<MethodInfo, ListenerMethodAttribute>>>>();
        public EventManager()
        {

        }

        public void Register(BasePlugin plugin, EventListener listener)
        {
            if (!registeredListeners.TryGetValue(plugin, out Dictionary<EventListener, List<KeyValuePair<MethodInfo, ListenerMethodAttribute>>> entry))
            {
                entry = new Dictionary<EventListener, List<KeyValuePair<MethodInfo, ListenerMethodAttribute>>>();
            }

            if (!entry.TryGetValue(listener, out List<KeyValuePair<MethodInfo, ListenerMethodAttribute>> listeners))
            {
                listeners = new List<KeyValuePair<MethodInfo, ListenerMethodAttribute>>();
            }

            foreach (MethodInfo method in listener.GetType().GetMethods())
            {
                ListenerMethodAttribute methodAttribute = method.GetCustomAttribute<ListenerMethodAttribute>();
                if (methodAttribute == null)
                    continue;

                if (methodAttribute.type == ListenerType.PacketRecieved)
                {
                    if (method.ReturnParameter.ParameterType != typeof(void))
                    {
                        plugin.GetLogger().Error($"Error Registering ListenerMethod \"{method.Name}\" in EventListener \"{listener.GetType().ToString()}\": Invalid Return Type, Must Be Of Type \"void\"");
                        continue;
                    }

                    ParameterInfo[] parameterInfos = method.GetParameters();

                    if (parameterInfos.Length != 3)
                    {
                        plugin.GetLogger().Error($"Error Registering ListenerMethod \"{method.Name}\" in EventListener \"{listener.GetType().ToString()}\": Invalid Parameter Count, Must Be (Event, INitroxConnection, Packet)");
                        continue;
                    }

                    if (parameterInfos[0].ParameterType != typeof(Event))
                    {
                        plugin.GetLogger().Error($"Error Registering ListenerMethod \"{method.Name}\" in EventListener \"{listener.GetType().ToString()}\": Invalid Parameter Type, Parameter 0 Must Be \"{typeof(Event)}\" Not \"{parameterInfos[0].ParameterType.ToString()}\"");
                        continue;
                    }

                    if (parameterInfos[1].ParameterType != typeof(INitroxConnection) && parameterInfos[1].ParameterType != typeof(Player))
                    {
                        plugin.GetLogger().Error($"Error Registering ListenerMethod \"{method.Name}\" in EventListener \"{listener.GetType().ToString()}\": Invalid Parameter Type, Parameter 1 Must Be \"{typeof(INitroxConnection)}\" OR \"{typeof(Player)}\" Not \"{parameterInfos[1].ParameterType.ToString()}\"");
                        continue;
                    }

                    if (!parameterInfos[2].ParameterType.IsSubclassOf(typeof(Packet)))
                    {
                        plugin.GetLogger().Error($"Error Registering ListenerMethod \"{method.Name}\" in EventListener \"{listener.GetType().ToString()}\": Invalid Parameter Type, Parameter 2 Must Extend \"{typeof(Packet)}\" Not \"{parameterInfos[2].ParameterType.ToString()}\"");
                        continue;
                    }
                }

                listeners.Add(new KeyValuePair<MethodInfo, ListenerMethodAttribute>(method, methodAttribute));

                
            }

            entry[listener] = listeners;
            registeredListeners[plugin] = entry;
        }

        protected void EmitEvent()
        {

        }

        public enum PacketType
        {
            SentOne,
            SentAll,
            SentOthers,
            Recieved,
            Server
        }

        //INTERNAL USE ONLY / NOT FOR PLUGIN USE, Calls PacketEvent Listeners To Handle This Packet, Doesnt Send It To Server
        public bool OnPacketCallback(INitroxConnection? connection, Packet packet, PacketType type)
        {
            Type packetType = packet.GetType();
            Type playerType = typeof(Player);
            Type connectionType = typeof(INitroxConnection);
            Event asscociatedEvent = new Event(); //decide if packet can be canceled and create a cancelable event if we can

            //Log.Info($"Debug Packet Log: {packetType}");

            foreach (var pluginPair in registeredListeners)
            {
                foreach (var listenerPair in pluginPair.Value)
                {
                    foreach (var methodPair in listenerPair.Value)
                    {
                        if ((methodPair.Value.type == ListenerType.PacketRecieved && type == PacketType.Recieved) || (methodPair.Value.type == ListenerType.PacketSent && type == PacketType.SentOne) || (methodPair.Value.type == ListenerType.PacketSentOthers && type == PacketType.SentOthers)) //open to refractoring
                        {
                            if (methodPair.Key.GetParameters()[2].ParameterType == packetType)
                            {
                                if (methodPair.Key.GetParameters()[1].ParameterType == playerType)
                                {
                                    methodPair.Key.Invoke(listenerPair.Key, new object[] { asscociatedEvent, Publix.getPlayerManager().GetPlayerByConnection(connection), packet });
                                }
                                else if (methodPair.Key.GetParameters()[1].ParameterType == connectionType)
                                {
                                    methodPair.Key.Invoke(listenerPair.Key, new object[] { asscociatedEvent, connection, packet });
                                }
                            }
                        }
                        else if (methodPair.Value.type == ListenerType.PacketSentAll && type == PacketType.SentAll)
                        {
                            if (methodPair.Key.GetParameters()[1].ParameterType == packetType)
                            {
                                methodPair.Key.Invoke(listenerPair.Key, new object[] { asscociatedEvent, packet });
                            }
                        }
                    }
                }
            }

            bool shouldContinue = true;

            if (asscociatedEvent.isCancelable)
            {
                shouldContinue = !asscociatedEvent.isCancelled;
            }

            if (type == PacketType.Recieved)
            {
                if (shouldContinue && packetType == typeof(ServerCommand))
                {
                    ServerCommand commandPacket = ((ServerCommand)packet);
                    Player commandSender = Publix.getPlayerManager().GetPlayerByConnection(connection);

                    shouldContinue = !Publix.playerCommandProcessor.ProcessCommand(commandPacket.Cmd, commandSender, commandSender.Permissions);
                }
            }

            return shouldContinue;
        }
    }
}
