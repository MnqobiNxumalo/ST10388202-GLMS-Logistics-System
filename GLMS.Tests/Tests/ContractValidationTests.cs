using Xunit;
using GLMS.Web.Models;

namespace GLMS.Tests
{
    public class ContractValidationTests
    {
        [Fact]
        public void CanCreateServiceRequest_WhenContractIsActive_ReturnsTrue()
        {
            var contract = new Contract
            {
                Status = ContractStatus.Active,
                StartDate = DateTime.Today.AddDays(-10),
                EndDate = DateTime.Today.AddDays(10)
            };
            bool result = contract.CanCreateServiceRequest();
            Assert.True(result);
        }

        [Fact]
        public void CanCreateServiceRequest_WhenContractIsDraft_ReturnsTrue()
        {
            var contract = new Contract
            {
                Status = ContractStatus.Draft,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            };
            bool result = contract.CanCreateServiceRequest();
            Assert.True(result);
        }

        [Fact]
        public void CanCreateServiceRequest_WhenContractIsExpired_ReturnsFalse()
        {
            var contract = new Contract
            {
                Status = ContractStatus.Expired,
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today.AddDays(-1)
            };
            bool result = contract.CanCreateServiceRequest();
            Assert.False(result);
        }

        [Fact]
        public void CanCreateServiceRequest_WhenContractIsOnHold_ReturnsFalse()
        {
            var contract = new Contract
            {
                Status = ContractStatus.OnHold,
                StartDate = DateTime.Today.AddDays(-10),
                EndDate = DateTime.Today.AddDays(20)
            };
            bool result = contract.CanCreateServiceRequest();
            Assert.False(result);
        }

        [Fact]
        public void IsActive_WhenStatusActiveAndDatesValid_ReturnsTrue()
        {
            var contract = new Contract
            {
                Status = ContractStatus.Active,
                StartDate = DateTime.Today.AddDays(-5),
                EndDate = DateTime.Today.AddDays(5)
            };
            bool result = contract.IsActive();
            Assert.True(result);
        }

        [Fact]
        public void IsActive_WhenEndDatePassed_ReturnsFalse()
        {
            var contract = new Contract
            {
                Status = ContractStatus.Active,
                StartDate = DateTime.Today.AddDays(-20),
                EndDate = DateTime.Today.AddDays(-1)
            };
            bool result = contract.IsActive();
            Assert.False(result);
        }

        [Fact]
        public void IsActive_WhenStartDateNotReached_ReturnsFalse()
        {
            var contract = new Contract
            {
                Status = ContractStatus.Active,
                StartDate = DateTime.Today.AddDays(5),
                EndDate = DateTime.Today.AddDays(30)
            };
            bool result = contract.IsActive();
            Assert.False(result);
        }

        [Fact]
        public void IsActive_WhenStatusIsExpired_ReturnsFalse()
        {
            var contract = new Contract
            {
                Status = ContractStatus.Expired,
                StartDate = DateTime.Today.AddDays(-10),
                EndDate = DateTime.Today.AddDays(10)
            };
            bool result = contract.IsActive();
            Assert.False(result);
        }
    }
}