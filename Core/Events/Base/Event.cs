namespace Nitrox_PublixExtension.Core.Events.Base
{
    public class Event
    {
        public readonly bool isCancelable = false;
        public bool isCancelled = false;
        public Event()
        {
        }
    }
}
