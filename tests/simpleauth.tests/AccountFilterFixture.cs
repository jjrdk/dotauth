namespace SimpleAuth.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Moq;
    using Shared;
    using Shared.AccountFiltering;
    using Shared.Repositories;
    using Xunit;

    public class AccountFilterFixture
    {
        private Mock<IFilterStore> _filterRepositoryStub;
        private IAccountFilter _accountFilter;

        [Fact]
        public async Task When_Pass_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _accountFilter.Check(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Claim_Does_Not_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            IEnumerable<Filter> filters = new List<Filter>
            {
                new Filter
                {
                    Rules = new List<FilterRule>
                    {
                        new FilterRule
                        {
                            ClaimKey = "key",
                            ClaimValue = "val",
                            Operation = ComparisonOperations.Equal
                        }
                    }
                }
            };
            _filterRepositoryStub.Setup(f => f.GetAll()).Returns(Task.FromResult(filters));

            var result = await _accountFilter.Check(new List<Claim>
            {
                new Claim("keyv", "valv")
            }).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.AccountFilterRules.Count() == 1);
            Assert.Equal("the claim 'key' doesn't exist", result.AccountFilterRules.First().ErrorMessages.First());
        }

        [Fact]
        public async Task When_Filter_Claim_Value_Equal_To_Val_Is_Wrong_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            IEnumerable<Filter> filters = new List<Filter>
            {
                new Filter
                {
                    Rules = new List<FilterRule>
                    {
                        new FilterRule
                        {
                            ClaimKey = "key",
                            ClaimValue = "val",
                            Operation = ComparisonOperations.Equal
                        }
                    }
                }
            };
            _filterRepositoryStub.Setup(f => f.GetAll()).Returns(Task.FromResult(filters));

            var result = await _accountFilter.Check(new List<Claim>
            {
                new Claim("key", "valv")
            }).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.AccountFilterRules.Count() == 1);
            Assert.Equal("the filter claims['key'] == 'val' is wrong", result.AccountFilterRules.First().ErrorMessages.First());
        }

        [Fact]
        public async Task When_Filter_Claim_Value_Not_Equal_To_Val_Is_Wrong_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            IEnumerable<Filter> filters = new List<Filter>
            {
                new Filter
                {
                    Rules = new List<FilterRule>
                    {
                        new FilterRule
                        {
                            ClaimKey = "key",
                            ClaimValue = "val",
                            Operation = ComparisonOperations.NotEqual
                        }
                    }
                }
            };
            _filterRepositoryStub.Setup(f => f.GetAll()).Returns(Task.FromResult(filters));

            var result = await _accountFilter.Check(new List<Claim>
            {
                new Claim("key", "val")
            }).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.AccountFilterRules.Count() == 1);
            Assert.Equal("the filter claims['key'] != 'val' is wrong", result.AccountFilterRules.First().ErrorMessages.First());
        }

        [Fact]
        public async Task When_Filter_Claim_Value_Does_Not_Match_Regular_Expression_Is_Wrong_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            IEnumerable<Filter> filters = new List<Filter>
            {
                new Filter
                {
                    Rules = new List<FilterRule>
                    {
                        new FilterRule
                        {
                            ClaimKey = "key",
                            ClaimValue = "^[0-9]{1}$",
                            Operation = ComparisonOperations.RegularExpression
                        }
                    }
                }
            };
            _filterRepositoryStub.Setup(f => f.GetAll()).Returns(Task.FromResult(filters));

            var result = await _accountFilter.Check(new List<Claim>
            {
                new Claim("key", "111")
            }).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.AccountFilterRules.Count() == 1);
            Assert.Equal("the filter claims['key'] match regular expression ^[0-9]{1}$ is wrong", result.AccountFilterRules.First().ErrorMessages.First());
        }

        [Fact]
        public async Task When_Filter_Claim_Value_Equal_To_Val_Is_Correct_Then_True_Is_Returned()
        {
            InitializeFakeObjects();
            IEnumerable<Filter> filters = new List<Filter>
            {
                new Filter
                {
                    Rules = new List<FilterRule>
                    {
                        new FilterRule
                        {
                            ClaimKey = "key",
                            ClaimValue = "val",
                            Operation = ComparisonOperations.Equal
                        }
                    }
                }
            };
            _filterRepositoryStub.Setup(f => f.GetAll()).Returns(Task.FromResult(filters));

            var result = await _accountFilter.Check(new List<Claim>
            {
                new Claim("key", "val")
            }).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task When_Filter_Claim_Value_Equal_To_Val_Is_Correct_And_Filter_Claim_Value_Different_To_Val_Is_Incorrect_Then_True_Is_Returned()
        {
            InitializeFakeObjects();
            IEnumerable<Filter> filters = new List<Filter>
            {
                new Filter
                {
                    Rules = new List<FilterRule>
                    {
                        new FilterRule
                        {
                            ClaimKey = "key",
                            ClaimValue = "val",
                            Operation = ComparisonOperations.Equal
                        }
                    }
                },
                new Filter
                {
                    Rules = new List<FilterRule>
                    {
                        new FilterRule
                        {
                            ClaimKey = "key",
                            ClaimValue = "val",
                            Operation = ComparisonOperations.NotEqual
                        }
                    }
                }
            };
            _filterRepositoryStub.Setup(f => f.GetAll()).Returns(Task.FromResult(filters));

            var result = await _accountFilter.Check(new List<Claim>
            {
                new Claim("key", "val")
            }).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        private void InitializeFakeObjects()
        {
            _filterRepositoryStub = new Mock<IFilterStore>();
            _accountFilter = new AccountFilter(_filterRepositoryStub.Object);
        }
    }
}
