using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmpyrionMessageBroker
{
    public class AmbiguousEventException: Exception
    {
        public AmbiguousEventException(string message) : base(message)
        {}
    }

   

    public static partial class Broker
    {
        private class handlerMethod
        {
            public Action<CmdId, object> handler;
            public int id;

            public handlerMethod(Action<CmdId, object> handler, int id)
            {
                this.handler = handler;
                this.id = id;
            }

            public handlerMethod(Action<CmdId, object> handler) : this(handler, handler.GetHashCode()) { }

        }

        private class apiEvent
        {
            public CmdId eventId;
            public ushort seqNr;
            public object data;

            public apiEvent(CmdId eventId, ushort seqNr, object data)
            {
                this.eventId = eventId;
                this.seqNr = seqNr;
                this.data = data;
            }
        }
        
    }
}
