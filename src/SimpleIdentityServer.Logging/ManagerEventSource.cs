#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

namespace SimpleIdentityServer.Logging
{
    using Microsoft.Extensions.Logging;

    public interface IManagerEventSource : IEventSource
    {		
        #region Events linked to client

        void StartToRemoveClient(string clientId);
        void FinishToRemoveClient(string clientId);
        void StartToUpdateClient(string request);
        void FinishToUpdateClient(string request);

        #endregion

        #region Events linked to resource owner

        void StartToRemoveResourceOwner(string subject);
        void FinishToRemoveResourceOwner(string subject);
        void StartToUpdateResourceOwnerClaims(string subject);
        void FinishToUpdateResourceOwnerClaims(string subject);
        void StartToUpdateResourceOwnerPassword(string subject);
        void FinishToUpdateResourceOwnerPassword(string subject);
        void StartToAddResourceOwner(string subject);
        void FinishToAddResourceOwner(string subject);

        #endregion

        #region Events linked to Scopes

        void StartToRemoveScope(string scope);

        void FinishToRemoveScope(string scope);

        #endregion

        #region Events linked to export

        void StartToExport();

        void FinishToExport();

        #endregion

        #region Events linked to import

        void StartToImport();

        void RemoveAllClients();

        void FinishToImport();

        #endregion
    }

    public class ManagerEventSource : BaseEventSource, IManagerEventSource
    {
        private static class Tasks
        {
            public const string Client = "Client";
            public const string ResourceOwner = "ResourceOwner";
            public const string Scope = "Scope";
            public const string Failure = "Failure";
            public const string Export = "Export";
            public const string Import = "Import";
        }

        #region Constructor

        public ManagerEventSource(ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger<ManagerEventSource>())
        {
        }

		#endregion		
		
        #region Events linked to client

        public void StartToRemoveClient(string clientId)
        {
            var evt = new Event
            {
                Id = 400,
                Task = Tasks.Client,
                Message = $"Start to remove the client : {clientId}"
            };

            LogInformation(evt);
        }

        public void FinishToRemoveClient(string clientId)
        {
            var evt = new Event
            {
                Id = 401,
                Task = Tasks.Client,
                Message = $"Finish to remove the client : {clientId}"
            };

            LogInformation(evt);
        }

        public void StartToUpdateClient(string request)
        {
            var evt = new Event
            {
                Id = 402,
                Task = Tasks.Client,
                Message = $"Start to update the client : {request}"
            };

            LogInformation(evt);
        }

        public void FinishToUpdateClient(string request)
        {
            var evt = new Event
            {
                Id = 403,
                Task = Tasks.Client,
                Message = $"Finish to update the client : {request}"
            };

            LogInformation(evt);
        }

        #endregion

        #region Events linked to resource owner

        public void StartToRemoveResourceOwner(string subject)
        {
            var evt = new Event
            {
                Id = 410,
                Task = Tasks.ResourceOwner,
                Message = $"Start to remove the resource owner: {subject}"
            };

            LogInformation(evt);
        }

        public void FinishToRemoveResourceOwner(string subject)
        {
            var evt = new Event
            {
                Id = 411,
                Task = Tasks.ResourceOwner,
                Message = $"Finish to remove the resource owner: {subject}"
            };

            LogInformation(evt);
        }

        public void StartToUpdateResourceOwnerClaims(string subject)
        {
            var evt = new Event
            {
                Id = 412,
                Task = Tasks.ResourceOwner,
                Message = $"Start to update the resource owner claims : {subject}"
            };

            LogInformation(evt);
        }

        public void FinishToUpdateResourceOwnerClaims(string subject)
        {
            var evt = new Event
            {
                Id = 413,
                Task = Tasks.ResourceOwner,
                Message = $"Finish to update the resource owner claims : {subject}"
            };

            LogInformation(evt);
        }

        public void StartToUpdateResourceOwnerPassword(string subject)
        {
            var evt = new Event
            {
                Id = 414,
                Task = Tasks.ResourceOwner,
                Message = $"Start to update the resource owner password : {subject}"
            };

            LogInformation(evt);
        }

        public void FinishToUpdateResourceOwnerPassword(string subject)
        {
            var evt = new Event
            {
                Id = 415,
                Task = Tasks.ResourceOwner,
                Message = $"Finish to update the resource owner password : {subject}"
            };

            LogInformation(evt);
        }

        public void StartToAddResourceOwner(string subject)
        {
            var evt = new Event
            {
                Id = 416,
                Task = Tasks.ResourceOwner,
                Message = $"Start to add the resource owner : {subject}"
            };

            LogInformation(evt);
        }

        public void FinishToAddResourceOwner(string subject)
        {
            var evt = new Event
            {
                Id = 417,
                Task = Tasks.ResourceOwner,
                Message = $"Finish to add the resource owner : {subject}"
            };

            LogInformation(evt);
        }

        #endregion

        #region Events linked to Scopes

        public void StartToRemoveScope(string scope)
        {
            var evt = new Event
            {
                Id = 420,
                Task = Tasks.Scope,
                Message = $"Start to remove the scope: {scope}"
            };

            LogInformation(evt);
        }

        public void FinishToRemoveScope(string scope)
        {
            var evt = new Event
            {
                Id = 421,
                Task = Tasks.Scope,
                Message = $"Finish to remove the scope: {scope}"
            };

            LogInformation(evt);
        }

        #endregion

        #region Events linked to export operations

        public void StartToExport()
        {
            var evt = new Event
            {
                Id = 430,
                Task = Tasks.Export,
                Message = $"Start to export"
            };

            LogInformation(evt);
        }

        public void FinishToExport()
        {
            var evt = new Event
            {
                Id = 431,
                Task = Tasks.Export,
                Message = $"Finish to export"
            };

            LogInformation(evt);
        }

        #endregion

        #region Events linked to import

        public void StartToImport()
        {
            var evt = new Event
            {
                Id = 440,
                Task = Tasks.Import,
                Message = $"Start to import"
            };

            LogInformation(evt);

        }

        public void RemoveAllClients()
        {
            var evt = new Event
            {
                Id = 441,
                Task = Tasks.Import,
                Message = $"All clients have been removed"
            };

            LogInformation(evt);
        }

        public void FinishToImport()
        {
            var evt = new Event
            {
                Id = 442,
                Task = Tasks.Import,
                Message = $"Import is finished"
            };

            LogInformation(evt);
        }

        #endregion
    }
}
