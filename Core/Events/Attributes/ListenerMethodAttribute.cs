namespace Nitrox_PublixExtension.Core.Events.Attributes
{
    public enum ListenerType
    {
        None,
        PacketRecieved,
        PacketSent,
        PacketSentAll,
        PacketSentOthers,
        PluginEvent
    }
    public class ListenerMethodAttribute : Attribute
    {
        public ListenerType type { get; protected set; }
        public ListenerMethodAttribute()
        {
            this.type = ListenerType.None;
        }

        public ListenerMethodAttribute(ListenerType type)
        {
            this.type = type;
        }
    }
}
