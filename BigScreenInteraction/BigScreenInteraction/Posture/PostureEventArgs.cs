using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigScreenInteraction
{
    public class PostureEventArgs : EventArgs
    {
        private string _EventName;

        public string EventName { get { return _EventName; } }
        public PostureEventArgs()
        {
            _EventName = "";
        }
        public PostureEventArgs(string eventId)
        {
            _EventName = eventId;
        }
    }

    public delegate void PostureEventHandler(object obj, PostureEventArgs args);
}
