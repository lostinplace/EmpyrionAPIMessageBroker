using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmpyrionMessageBroker
{
    public static partial class Broker
    {
        private static Dictionary<CmdId, List<handlerMethod>> unsolicitedEventHandlers = new Dictionary<CmdId, List<handlerMethod>>();

        private static void handleUnsolicitedEvent(apiEvent anEvent)
        {
            if (!unsolicitedEventHandlers.ContainsKey(anEvent.eventId)) return;
            var handlerList = unsolicitedEventHandlers[anEvent.eventId];

            handlerList.ForEach(x => x.handler(anEvent.eventId, anEvent.data));
        }

        private static void RegisterUnsolicitedEventHandler(CmdId eventId, handlerMethod handler)
        {
            log($"adding handler {handler.ToString()}");
            List<handlerMethod> handlerList;
            handlerList = unsolicitedEventHandlers.TryGetValue(eventId, out handlerList) ? handlerList : new List<handlerMethod>();
            handlerList.Add(handler);
            unsolicitedEventHandlers[eventId] = handlerList;
        }

        public static void RegisterUnsolicitedEventHandler(CmdId eventId, Action<CmdId, object> handler)
        {
            var tmpHandler = new handlerMethod(handler);
            RegisterUnsolicitedEventHandler(eventId, tmpHandler);
        }

        /// <summary>
        /// Adds a handler for unsolicited events, i.e. events that are triggerd by the game, not in response to mod requests)
        /// </summary>
        /// <typeparam name="DataType">The type of the expected payload</typeparam>
        /// <param name="eventId">the CmdId relating to the event being handled</param>
        /// <param name="handler">a function to call when the event is received</param>
        /// <exception cref="System.ArgumentException">Thrown when the type of the payload doesn't match with the API contract</exception>
        public static void RegisterUnsolicitedEventHandler<DataType>(CmdId eventId, Action<CmdId, DataType> handler)
        {
            validateUnsolicitedEventHandler<DataType>(eventId);
            Action<CmdId, object> actualHandler = (evt, data) => handler(evt, (DataType)data);
            var tmpHandler = new handlerMethod(actualHandler, handler.GetHashCode());
            RegisterUnsolicitedEventHandler(eventId, tmpHandler);
        }

        public static void RegisterUnsolicitedEventHandler<DataType>(CmdId eventId, Action<DataType> handler)
        {
            validateUnsolicitedEventHandler<DataType>(eventId);
            Action<CmdId, object> actualHandler = (evt, data) => handler((DataType)data);
            var tmpHandler = new handlerMethod(actualHandler, handler.GetHashCode());
            RegisterUnsolicitedEventHandler(eventId, tmpHandler);
        }

        public static void RegisterUnsolicitedEventHandler<DataType>(Action<DataType> handler)
        {
            var eventDataType = typeof(DataType);
            var potentialEvents = EmpyrionAPIContractManager.UnsolicitedEvents.Values.Where(x => x.responseDataType == typeof(DataType)).ToList();
            if (potentialEvents.Count > 1)
                throw new ArgumentException($"result type {eventDataType}  is found in {potentialEvents.Count} unsolicited events, you must be more specific");
            else if (potentialEvents.Count == 0)
                throw new ArgumentException($"result type {eventDataType} is not found in the list of unsolicited events");
            
            Action<CmdId, object> actualHandler = (evt, data) => handler((DataType)data);
            var tmpHandler = new handlerMethod(actualHandler, handler.GetHashCode());
            RegisterUnsolicitedEventHandler(potentialEvents.First().responseCmdId, tmpHandler);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="DataType"></typeparam>
        /// <param name="handler">function to be invoked upon receipt of a compatible event</param>
        /// <param name="validateSingle">used to validate that the specified handler will only be associated with one event type</param>
        /// <exception cref="AmbiguousEventException">Thrown when the type of the payload matches more than one unsolicited event type</exception>
        public static void RegisterUnsolicitedEventHandler<DataType>(Action<CmdId, DataType> handler, bool validateSingle = true)
        {
            var eventTypes = EmpyrionAPIContractManager.UnsolicitedEvents.Values.ToList();
            var typeDataType = typeof(DataType);
            var matchingEvents = eventTypes.Where(x => x.responseDataType == typeDataType);
            if (validateSingle && matchingEvents.Count() > 1)
                throw new AmbiguousEventException($"{typeDataType.ToString()} is the registered data type for {matchingEvents.Count()} events, if you meant " +
                    $"to register multiple handlers, make sure \"validateSingle\" is set to false");
            else if (matchingEvents.Count() == 0)
                throw new ArgumentException($"{typeDataType.ToString()} is not a valid data type for any unsolicited event types");
            matchingEvents.ToList().ForEach(x => RegisterUnsolicitedEventHandler<DataType>(x.responseCmdId, handler));
        }

        /// <summary>
        /// Does basic validation of values against the contract
        /// </summary>
        /// <typeparam name="DataType">the payload type being evaluated</typeparam>
        /// <param name="eventId">the cldid being evaluated</param>
        /// <exception cref="System.ArgumentException">Thrown when the type of the payload doesn't match with the specified API contract</exception>
        private static void validateUnsolicitedEventHandler<DataType>(CmdId eventId)
        {
            var applicableCOntract = EmpyrionAPIContractManager.UnsolicitedEvents[eventId];

            var argType = typeof(DataType);
            if (applicableCOntract.responseDataType != typeof(DataType))
            {
                var message = $"{argType.ToString()} is not a valid data type API event {eventId}; expected: {applicableCOntract.responseDataType.ToString()} ";
                throw new ArgumentException(message);
            }
        }


        /// <summary>
        /// unregisters an event handler for the specified event id
        /// </summary>
        /// <param name="handler">handler to remove</param>
        /// <param name="eventId">the cmdid to unregister the handler for</param>
        /// <param name="removeAll">whether or not this should remove all of the handler instances</param>
        public static void UnregisterUnsolicitedEventHandler(Action<CmdId, object> handler, CmdId eventId, bool removeAll = true)
        {
            unregisterUnsolicitedEventHandler(handler.GetHashCode(), eventId, removeAll);
        }

        /// <summary>
        /// TODO: COMMENT THIS
        /// </summary>
        /// <typeparam name="ExpectedResult"></typeparam>
        /// <param name="handler"></param>
        /// <param name="removeAll"></param>
        public static void UnregisterUnsolicitedEventHandler<ExpectedResult>(Action<CmdId, ExpectedResult> handler, bool removeAll = true)
        {
            var handlerId = handler.GetHashCode();
            var handlers = unsolicitedEventHandlers.Values.SelectMany(x => x.Where(y => y.id == handlerId)).ToList();
            if (!removeAll && handlers.Count > 1)
            {
                throw new ArgumentException($"the specified handler is regietered multiple times, " +
                    $"if you wish to remove them all, call the function with \"removeAll\" set to true");
            }
            else if (handlers.Count == 0)
                throw new ArgumentException($"the specified handler is not a registered handler for the specified cmdId");

            foreach (var item in unsolicitedEventHandlers.Keys.ToList())
            {
                var newHandlers = unsolicitedEventHandlers[item].Where(x => x.id != handlerId).ToList();
                unsolicitedEventHandlers[item] = newHandlers;
            }
        }

        /// <summary>
        /// TODO: Comment this
        /// </summary>
        /// <param name="eventId"></param>
        public static void UnregisterUnsolicitedEventHandlers(CmdId eventId)
        {
            unsolicitedEventHandlers[eventId] = new List<handlerMethod>();
        }


        /// <summary>
        /// does the unregistering work
        /// </summary>
        /// <param name="id">the id of the handler being removed </param>
        /// <param name="eventId">the cmdid to unregister the handler for</param>
        /// <param name="removeAll">whether or not this should remove all of the handler instances</param>
        /// <exception cref="System.ArgumentException">Thrown when the specified handler isn't registered for the specified cmdId, or when it is registered multiple times 
        /// and removeAll is set to false</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when there are no handlers registered for the specified event</exception>
        private static void unregisterUnsolicitedEventHandler(int id, CmdId eventId, bool removeAll = true)
        {
            if (!unsolicitedEventHandlers.ContainsKey(eventId))
            {
                throw new InvalidOperationException($"no handlers registered for event: {eventId}");
            }
            List<handlerMethod> handlerList = unsolicitedEventHandlers[eventId];
            var newHandlers = handlerList.Where(x => x.id != id).ToList();
            if (!removeAll && newHandlers.Count > 1)
                throw new ArgumentException($"the specified handler is regietered multiple times for the specified cmdId, " +
                    $"if you wish to remove them all, call the function with \"removeAll\" set to true");
            else if (newHandlers.Count == 0)
                throw new ArgumentException($"the specified handler is not a registered handler for the specified cmdId");
            unsolicitedEventHandlers[eventId] = newHandlers;
        }
    }
}
