using Nitrox_PublixExtension.Core.Events.Attributes;
using Nitrox_PublixExtension.Core.Events.Base;
using Nitrox_PublixExtension.Core.Plugin;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer;
using NitroxServer.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

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

                if (methodAttribute.type == ListenerType.PacketEvent)
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

                    if (parameterInfos[1].ParameterType != typeof(INitroxConnection))
                    {
                        plugin.GetLogger().Error($"Error Registering ListenerMethod \"{method.Name}\" in EventListener \"{listener.GetType().ToString()}\": Invalid Parameter Type, Parameter 1 Must Be \"{typeof(INitroxConnection)}\" Not \"{parameterInfos[1].ParameterType.ToString()}\"");
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

        //INTERNAL USE ONLY / NOT FOR PLUGIN USE, Calls PacketEvent Listeners To Handle This Packet, Doesnt Send It To Server
        public bool OnPacketCallback(INitroxConnection connection, Packet packet)
        {
            Type packetType = packet.GetType();
            Event asscociatedEvent = new Event(); //decide if packet can be canceled and create a cancelable event if we can

            //Log.Info($"Debug Packet Log: {packetType}");

            foreach (var pluginPair in registeredListeners)
            {
                foreach (var listenerPair in pluginPair.Value)
                {
                    foreach (var methodPair in listenerPair.Value)
                    {
                        if (methodPair.Value.type == ListenerType.PacketEvent)
                        {
                            if (methodPair.Key.GetParameters()[2].ParameterType == packetType)
                            {
                                methodPair.Key.Invoke(listenerPair.Key, new object[] { asscociatedEvent, connection, packet });
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

            if (shouldContinue && packetType == typeof(ServerCommand))
            {
                ServerCommand commandPacket = ((ServerCommand)packet);
                Player commandSender = Publix.getPlayerManager().GetPlayerByConnection(connection);

                shouldContinue = !Publix.playerCommandProcessor.ProcessCommand(commandPacket.Cmd, commandSender, commandSender.Permissions);
            }

            return shouldContinue;
        }
    }
}
