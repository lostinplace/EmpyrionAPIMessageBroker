using Eleon.Modding;
using System;
using System.Linq;

namespace EmpyrionMessageBroker
{
    public class APIResult<DataType>
    {
        private CmdId cmdId;
        private object data;
        private Action<DataType> handler;
        private Action<ErrorInfo> errorHandler;

        private DataType value;
        private bool responseReceived;
        private bool succeeded;

        public APIResult() { }

        public APIResult<DataType> From(CmdId cmdId, object data=null, Action<DataType> responseHandler = null, Action<ErrorInfo> errorHandler = null)
        {
            this.cmdId = cmdId;
            this.data = data;
            return this;
        }

        public APIResult<DataType> From(object data, Action<DataType> responseHandler=null, Action<ErrorInfo> errorHandler = null){

            var datatype = data.GetType();
            var responseDataType = typeof(DataType);

            var potentialCalls = EmpyrionAPIContractManager.APIContracts.Values.Where(x => x.ArgType == datatype && x.ExpectedResponseEvent.responseDataType == responseDataType).ToList();
            if (potentialCalls.Count > 1)
                throw new ArgumentException($"result type {responseDataType} with argument type {datatype} is found in {potentialCalls.Count} contracts, you must be more specific");
            else if(potentialCalls.Count == 0)
                throw new ArgumentException($"result type {responseDataType} with argument type {datatype} is not found in the list of API contracts");
            var applicableCall = potentialCalls.First();
            this.cmdId = applicableCall.cmdId;
            this.data = data;
            this.handler = responseHandler;
            this.errorHandler = errorHandler;
            return this;
        }

        public APIResult<DataType> OnResponse(Action<DataType> handler)
        {
            this.handler = handler;
            return this;
        }

        public APIResult<DataType> OnError(Action<ErrorInfo> handler)
        {
            this.errorHandler = handler;
            return this;
        }

        private void handleResult(CmdId evt, object payload)
        {
            this.responseReceived = true;
            this.succeeded = true;
            this.value = (DataType)payload;
            this.handler(this.value);

        }

        private void handleError(ErrorInfo info)
        {
            this.responseReceived = true;
            this.succeeded = false;
            this.errorHandler(info);
        }

        public APIResult<DataType> Execute()
        {
            var cmd = new GenericAPICommand(this.cmdId, this.data, handleResult, handleError );
            cmd.Execute();
            return this;
        }

    }
}
