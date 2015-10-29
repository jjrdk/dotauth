﻿using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.RateLimitation.Configuration;
using SimpleIdentityServer.RateLimitation.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using DOMAINS = SimpleIdentityServer.Core.Models;
using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetAccessTokenMultipleTime")]
    public sealed class GetAccessTokenMultipleTimeSpec
    {
        private readonly ConfigureWebApi _configureWebApi;

        private readonly ISecurityHelper _securityHelper;

        private List<DOMAINS.GrantedToken> _tokens;

        private List<TooManyRequestResponse> _errors;

        private RateLimitationElement _rateLimitationElement;

        private List<HttpResponse> _httpResponses;

        public GetAccessTokenMultipleTimeSpec()
        {
            _rateLimitationElement = new RateLimitationElement
            {
                Name = "PostToken"
            };
            var fakeGetRateLimitationElementOperation = new FakeGetRateLimitationElementOperation
            {
                Enabled = true,
                RateLimitationElement = _rateLimitationElement
            };
            _configureWebApi = new ConfigureWebApi();
            _configureWebApi.Container.RegisterInstance<IGetRateLimitationElementOperation>(fakeGetRateLimitationElementOperation);

            _securityHelper = new SecurityHelper();
            _tokens = new List<DOMAINS.GrantedToken>();
            _errors = new List<TooManyRequestResponse>();
            _httpResponses = new List<HttpResponse>();
        }

        [Given("a resource owner with username (.*) and password (.*) is defined")]
        public void GivenResourceOwner(string userName, string password)
        {
            var resourceOwner = new MODELS.ResourceOwner
            {
                UserName = userName,
                Password = _securityHelper.ComputeHash(password)
            };

            FakeDataSource.Instance().ResourceOwners.Add(resourceOwner);
        }

        [Given("a mobile application (.*) is defined")]
        public void GivenClient(string clientId)
        {
            var client = new MODELS.Client
            {
                ClientId = clientId,
                AllowedScopes = new List<MODELS.Scope>()
            };

            FakeDataSource.Instance().Clients.Add(client);
        }

        [Given("allowed number of requests is (.*)")]
        public void GivenAllowedNumberOfRequests(int numberOfRequests)
        {
            _rateLimitationElement.NumberOfRequests = numberOfRequests;
        }

        [Given("sliding time is (.*)")]
        public void GivenSlidingTime(double slidingTime)
        {
            _rateLimitationElement.SlidingTime = slidingTime;
        }

        [When("requesting access tokens")]
        public void WhenRequestingAccessTokens(Table table)
        {
            var tokenRequests = table.CreateSet<TokenRequest>();
            var responseCacheManager = CacheFactory.GetCacheManager();
            responseCacheManager.Flush();

            var server = _configureWebApi.CreateServer();
            foreach (var tokenRequest in tokenRequests)
            {
                var httpClient = server.HttpClient;
                var parameter = string.Format(
                    "grant_type=password&username={0}&password={1}&client_id={2}&scope={3}",
                    tokenRequest.username,
                    tokenRequest.password,
                    tokenRequest.client_id,
                    tokenRequest.scope);
                var content = new StringContent(parameter, Encoding.UTF8, "application/x-www-form-urlencoded");
            
                var result = httpClient.PostAsync("/api/token", content).Result;
                var httpStatusCode = result.StatusCode;
                _httpResponses.Add(new HttpResponse
                {
                    StatusCode = httpStatusCode,
                    NumberOfRequests = result.Headers.GetValues(RateLimitationConstants.XRateLimitLimitName).FirstOrDefault(),
                    NumberOfRemainingRequests = result.Headers.GetValues(RateLimitationConstants.XRateLimitRemainingName).FirstOrDefault()
                });
                if (httpStatusCode == HttpStatusCode.OK)
                {
                    _tokens.Add(result.Content.ReadAsAsync<DOMAINS.GrantedToken>().Result);
                    continue;
                }
            
                _errors.Add(new TooManyRequestResponse
                {
                    Message = result.Content.ReadAsAsync<string>().Result,
                });
            }
        }

        [When("waiting for (.*) seconds")]
        public void WhenWaitingForSeconds(int milliSeconds)
        {
            Thread.Sleep(milliSeconds);
        }

        [Then("(.*) access tokens are generated")]
        public void ThenTheResultShouldBe(int numberOfAccessTokens)
        {
            Assert.That(_tokens.Count, Is.EqualTo(numberOfAccessTokens));
        }

        [Then("the errors should be returned")]
        public void ThenErrorsShouldBe(Table table)
        {
            var records = table.CreateSet<TooManyRequestResponse>().ToList();
            Assert.That(records.Count, Is.EqualTo(_errors.Count()));
            for (var i = 0; i < records.Count() - 1; i++)
            {
                var record = records[i];
                var error = _errors[i];
                Assert.That(record.Message, Is.EqualTo(error.Message));
            }
        }

        [Then("the http responses should be returned")]
        public void ThenHttpHeadersShouldContain(Table table)
        {
            var records = table.CreateSet<HttpResponse>().ToList();
            Assert.That(records.Count, Is.EqualTo(_httpResponses.Count()));
            for(var i = 0; i < records.Count() - 1; i++)
            {
                var record = records[i];
                var httpResponse = _httpResponses[i];
                Assert.That(record.StatusCode, Is.EqualTo(record.StatusCode));
                Assert.That(record.NumberOfRemainingRequests, Is.EqualTo(record.NumberOfRemainingRequests));
                Assert.That(record.NumberOfRequests, Is.EqualTo(record.NumberOfRequests));
            }
        }
    }
}
