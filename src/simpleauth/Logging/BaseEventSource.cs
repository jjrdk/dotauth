//namespace SimpleAuth.Logging
//{
//    using System;
//    using Microsoft.Extensions.Logging;

//    public class BaseEventSource
//    {
//        protected readonly ILogger _logger;
//        protected const string MessagePattern = "{Id} : {Task}, {Message} : {Operation}";

//        public BaseEventSource(ILogger logger)
//        {
//            _logger = logger;
//        }

//        public void Info(string message)
//        {
//            var evt = new Event
//            {
//                Id = 8000,
//                Task = EventTasks.Information,
//                Message = message
//            };

//            LogInformation(evt);
//        }

//        public void Failure(string message)
//        {
//            var evt = new Event
//            {
//                Id = 9000,
//                Task = EventTasks.Failure,
//                Message = $"Something goes wrong, code : {message}"
//            };

//            LogError(evt);
//        }

//        public void Failure(Exception exception)
//        {
//            var evt = new Event
//            {
//                Id = 9001,
//                Task = EventTasks.Failure,
//                Message = "an error occured"
//            };

//            LogError(evt, new EventId(28), exception);
//        }

//        protected void LogInformation(Event evt)
//        {
//            _logger.LogInformation(MessagePattern, evt.Id, evt.Task, evt.Message, evt.Operation);
//        }

//        protected void LogError(Event evt)
//        {
//            _logger.LogError(MessagePattern, evt.Id, evt.Task, evt.Message, evt.Operation);
//        }

//        protected void LogError(Event evt, EventId evtId, Exception ex)
//        {
//            _logger.LogError(evtId, ex, MessagePattern, evt.Id, evt.Task, evt.Message, evt.Operation);
//        }
//    }
//}
