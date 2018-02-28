using Eleon.Modding;
using System;
using System.Threading;

namespace EmpyrionMessageBroker
{
    public class UnhandledGameAPIError : Exception {
        public UnhandledGameAPIError(string message) : base(message) { }
    }


    public class GenericAPICommand
    {
        private static Random  rnd = new Random();

        public APICall call;
        public object argument;
        public Action<CmdId, object> responseHandler;
        public Action<ErrorInfo> errorHandler;
        public bool validated;
        public bool responseReceived { get; private set; } = false;
        public bool succeeded { get; private set; }
        public object data { get; private set; } = null;

        public ushort SequenceNumber
        {
            get
            {
                var argPart = argument == null ? 1 : argument.GetHashCode() % 511;
                var cmdIdPart = (ushort)call.cmdId + 1;
                var combined = (cmdIdPart << 7) | argPart;
                return (ushort)combined;
            }
        }

        private void defaultResponseHandler(CmdId evt, object data)
        {
            Broker.log(() => $"unhandled response to {evt} event with {data}");
        }

        private void defaultErrorHandler(ErrorInfo err)
        {
            throw new UnhandledGameAPIError($"{err.errorType} : {err.ToString()}");
        }

        private Action<CmdId, object> responseManager(Action<CmdId, object> handler)
        {
            return (evt, data) =>
            {
                this.succeeded = true;
                this.data = data;
                this.responseReceived = true;
                handler(evt, data);
            };
        }

        private Action<ErrorInfo> errorManager(Action<ErrorInfo> handler)
        {
            return (err) =>
            {
                this.succeeded = false;
                this.data = null;
                this.responseReceived = true;
                handler(err);
            };
        }

        public GenericAPICommand(CmdId cmdId, object argument, Action<CmdId, object> responseHandler=null, Action<ErrorInfo> errorHandler=null, bool validate = true)
        {

            this.call = EmpyrionAPIContractManager.APIContracts[cmdId];
            this.argument = argument;
            this.responseHandler = responseHandler != null ? responseHandler : defaultResponseHandler;
            this.errorHandler = errorHandler != null ? errorHandler : defaultErrorHandler;
            this.validated = validate ? validateArguments(this.call, this.argument, this.responseHandler) : validate;
        }

        private static bool validateArguments(APICall call, object argument, Action<CmdId, object> responseHandler)
        {
            var argType = argument != null ? argument.GetType() : null;
            string message = null;
            if (argType != call.ArgType)
            {
                message = $"{argType.ToString()} is not a valid argument type for API call {call.cmdId}; expected: {call.ArgType.ToString()} ";
            }
            if (message != null)
            {
                throw new ArgumentException(message);
            }
            return true;
        }

        public GenericAPICommand Execute()
        {
            Broker.Execute(this);
            return this;
        }
    }
}
