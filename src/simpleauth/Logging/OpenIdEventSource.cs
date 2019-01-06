//namespace SimpleAuth.Logging
//{
//    using Microsoft.Extensions.Logging;
//    using Shared;

//    public class OpenIdEventSource
//    {
//        private static class Tasks
//        {
//            public const string UserInteraction = "UserInteraction";
//            public const string UserManagement = "UserManagement";
//        }

//        public OpenIdEventSource(ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger<OpenIdEventSource>())
//        {
//        }

//        public void AuthenticateResourceOwner(string subject)
//        {
//            var evt = new Event
//            {
//                Id = 201,
//                Task = Tasks.UserInteraction,
//                Message = $"The resource owner is authenticated {subject}"
//            };
//            LogInformation(evt);
//        }

//        public void GetConfirmationCode(string code)
//        {
//            var evt = new Event
//            {
//                Id = 202,
//                Task = Tasks.UserInteraction,
//                Message = $"Get confirmation code {code}"
//            };
//            LogInformation(evt);
//        }

//        public void InvalidateConfirmationCode(string code)
//        {
//            var evt = new Event
//            {
//                Id = 203,
//                Task = Tasks.UserInteraction,
//                Message = $"Remove confirmation code {code}"
//            };
//            LogInformation(evt);
//        }

//        public void ConfirmationCodeNotValid(string code)
//        {
//            var evt = new Event
//            {
//                Id = 204,
//                Task = Tasks.UserInteraction,
//                Message = $"Confirmation code is not valid {code}"
//            };
//            LogError(evt);
//        }

//        public void AddResourceOwner(string subject)
//        {
//            var evt = new Event
//            {
//                Id = 205,
//                Task = Tasks.UserManagement,
//                Message = $"The resource owner is created {subject}"
//            };
//            LogInformation(evt);
//        }

//        public void OpenIdFailure(string code,
//            string description,
//            string state)
//        {
//            var evt = new Event
//            {
//                Id = 300,
//                Task = EventTasks.Failure,
//                Message = $"Something goes wrong in the openid process, code : {code}, description : {description}, state : {state}"
//            };

//            LogError(evt);
//        }
//    }
//}
