using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Tests.Models
{
    public class ExchangeRateTests
    {
        [Fact]
        public void Create_WithValidInput_ShouldReturnProperlyInitializedExchangeRate()
        {
            // Arrange
            var from = "USD";
            var to = "EUR";
            var rate = 1.2345m;

            // Act
            var exchangeRate = ExchangeRate.Create(from, to, rate);

            // Assert
            exchangeRate.FromCurrencyCode.Should().Be("USD");
            exchangeRate.ToCurrencyCode.Should().Be("EUR");
            exchangeRate.Rate.Should().Be(rate);
            exchangeRate.RetrievedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            exchangeRate.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            exchangeRate.UpdatedAt.Should().BeNull();
        }

        [Fact]
        public void Create_WithInvalidFromCurrencyCode_ShouldThrowValidationException()
        {
            // Act
            Action act = () => ExchangeRate.Create("X", "EUR", 1.0m);

            // Assert
            act.Should()
               .Throw<ValidationException>()
               .WithMessage("*FromCurrencyCode: Currency code must be a valid 3-letter ISO code*");
        }

        [Fact]
        public void Create_WithInvalidToCurrencyCode_ShouldThrowValidationException()
        {
            // Act
            Action act = () => ExchangeRate.Create("USD", "ZZZZ", 1.0m);

            // Assert
            act.Should()
               .Throw<ValidationException>()
               .WithMessage("*ToCurrencyCode: Currency code must be a valid 3-letter ISO code*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-0.00001)]
        public void Create_WithNonPositiveRate_ShouldThrowValidationException(decimal rate)
        {
            // Act
            Action act = () => ExchangeRate.Create("USD", "EUR", rate);

            // Assert
            act.Should()
               .Throw<ValidationException>()
               .WithMessage("*Exchange rate must be greater than zero*");
        }

        [Fact]
        public void UpdateRate_WithDifferentValidRate_ShouldUpdateRateAndTimestamp()
        {
            // Arrange
            var rate = ExchangeRate.Create("USD", "EUR", 1.0m);

            // Act
            rate.UpdateRate(1.25m);

            // Assert
            rate.Rate.Should().Be(1.25m);
            rate.RetrievedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            rate.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UpdateRate_WithSameRate_ShouldNotUpdateAnything()
        {
            // Arrange
            var rate = ExchangeRate.Create("USD", "EUR", 1.5m);

            // Act
            rate.UpdateRate(1.5m);

            // Assert
            rate.Rate.Should().Be(1.5m);
            rate.UpdatedAt.Should().BeNull();
        }

        [Fact]
        public void UpdateRate_WithInvalidRate_ShouldThrowValidationException()
        {
            // Arrange
            var rate = ExchangeRate.Create("USD", "EUR", 1.0m);

            // Act
            Action act = () => rate.UpdateRate(0);

            // Assert
            act.Should()
               .Throw<ValidationException>()
               .WithMessage("*Exchange rate must be greater than zero*");
        }

        [Fact]
        public void ValidateRate_WithInvalidRate_ShouldReturnError()
        {
            // Act
            var errors = ExchangeRate.ValidateRate(0).ToList();

            // Assert
            errors.Should().ContainSingle()
                .Which.Should().Contain("Exchange rate must be greater than zero");
        }
    }
}
