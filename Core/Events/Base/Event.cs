using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
