using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace EmpyrionAPIMessageBroker
{
    public class MessageBroker
    {
        public ModGameAPI GameAPI;
        private Dictionary<ushort, Action<CmdId, object>> actionTracker = new Dictionary<ushort, Action<CmdId, object>>();
        private static ushort handledMessageCount = 0;
        private static ushort offset = 1000;

        private bool verbose;

        public MessageBroker(ModGameAPI dediAPI, bool verbose=false)
        {
            this.GameAPI = dediAPI;
            this.verbose = verbose;
        }

        private void defaultHandler(CmdId cmd, object any) { }


        public void ExecuteCommand(APICmd cmd)
        {
            this.ExecuteCommand<Object>(cmd, defaultHandler);
        }

        public void ExecuteCommand<ResponseType>(APICmd cmd, Action<ResponseType> handler)
        {
            Action<CmdId, object> outerHandler = (x, y) => handler((ResponseType)y);
            trackHandler(cmd, outerHandler);
        }

        public void ExecuteCommand<ResponseType>(APICmd cmd, Action<CmdId, ResponseType> handler)
        {
            Action<CmdId, object> outerHandler = (x, y) => handler(x, (ResponseType)y);
            trackHandler(cmd, outerHandler);
        }

        private void trackHandler(APICmd cmd, Action<CmdId, object> handler)
        {
            var seqNr = (ushort)((handledMessageCount++ % 30000) + offset);
            actionTracker[seqNr] = handler;
            GameAPI.Game_Request(cmd.cmd, seqNr, cmd.data);
        }

        public void HandleMessage(CmdId eventId, ushort seqNr, object data)
        {
            if (!actionTracker.ContainsKey(seqNr)) return;
            var action = actionTracker[seqNr];
            action(eventId, data);
            deprovisionSequenceNumber(seqNr);

        }

        private void log(Func<string> logMsg)
        {
            if (this.verbose)
            {
                GameAPI.Console_Write(logMsg());
            }
        }

        public void deprovisionSequenceNumber(ushort seqnr)
        {
            actionTracker.Remove(seqnr);
        }
    }

    public class APICmd
    {
        public CmdId cmd { get; }
        public object data { get; }

        public APICmd(CmdId cmd, object data = null)
        {
            this.cmd = cmd;
            this.data = data;
        }

        public static APICmd operator +(APICmd cmd, object data)
        {
            return new APICmd(cmd.cmd, data);
        }

        public static implicit operator APICmd(CmdId d)
        {
            return new APICmd(d);
        }
    }
}
