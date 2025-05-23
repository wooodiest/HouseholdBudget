using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Tests.Models
{
    public class CurrencyTests
    {
        [Fact]
        public void Create_WithValidInput_ShouldReturnProperlyInitializedCurrency()
        {
            // Arrange
            var code = "USD";
            var symbol = "$";
            var name = "US Dollar";

            // Act
            var currency = Currency.Create(code, symbol, name);

            // Assert
            currency.Code.Should().Be("USD");
            currency.Symbol.Should().Be(symbol);
            currency.Name.Should().Be(name);
            currency.Id.Should().NotBeEmpty();
            currency.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            currency.UpdatedAt.Should().BeNull();
        }

        [Fact]
        public void Create_WithNullCode_ShouldThrowValidationException()
        {
            // Act
            Action act = () => Currency.Create(null!, "$", "Dollar");

            // Assert
            act.Should()
               .Throw<ValidationException>()
               .WithMessage("*Currency code must be a valid 3-letter ISO code*");
        }

        [Fact]
        public void Create_WithEmptyCode_ShouldThrowValidationException()
        {
            Action act = () => Currency.Create("", "$", "Dollar");

            act.Should()
               .Throw<ValidationException>()
               .WithMessage("*Currency code must be a valid 3-letter ISO code*");
        }

        [Fact]
        public void Create_WithWhitespaceCode_ShouldThrowValidationException()
        {
            Action act = () => Currency.Create("   ", "$", "Dollar");

            act.Should()
               .Throw<ValidationException>()
               .WithMessage("*Currency code must be a valid 3-letter ISO code*");
        }

        [Fact]
        public void Create_WithInvalidCodeLength_ShouldThrowValidationException()
        {
            Action act = () => Currency.Create("EU", "€", "Euro");

            act.Should()
               .Throw<ValidationException>()
               .WithMessage("*Currency code must be a valid 3-letter ISO code*");
        }

        [Fact]
        public void Create_WithNonAlphabeticCode_ShouldThrowValidationException()
        {
            Action act = () => Currency.Create("12$", "$", "Dollar");

            act.Should()
               .Throw<ValidationException>()
               .WithMessage("*Currency code must be a valid 3-letter ISO code*");
        }

        [Fact]
        public void Create_WithNullSymbol_ShouldThrowValidationException()
        {
            Action act = () => Currency.Create("USD", null!, "Dollar");

            act.Should()
               .Throw<ValidationException>()
               .WithMessage("*Currency symbol is required*");
        }

        [Fact]
        public void Create_WithTooLongSymbol_ShouldThrowValidationException()
        {
            var longSymbol = new string('#', Currency.MaxSymbolLength + 1);
            Action act = () => Currency.Create("USD", longSymbol, "Dollar");

            act.Should()
               .Throw<ValidationException>()
               .WithMessage($"*cannot exceed {Currency.MaxSymbolLength}*");
        }

        [Fact]
        public void Create_WithNullName_ShouldThrowValidationException()
        {
            Action act = () => Currency.Create("USD", "$", null!);

            act.Should()
               .Throw<ValidationException>()
               .WithMessage("*Currency name is required*");
        }

        [Fact]
        public void Create_WithTooLongName_ShouldThrowValidationException()
        {
            var longName = new string('A', Currency.MaxNameLength + 1);
            Action act = () => Currency.Create("USD", "$", longName);

            act.Should()
               .Throw<ValidationException>()
               .WithMessage($"*cannot exceed {Currency.MaxNameLength}*");
        }

        [Fact]
        public void ValidateCode_WithInvalidCode_ShouldReturnError()
        {
            var result = Currency.ValidateCode("1X!");

            result.Should().ContainSingle()
                .Which.Should().Contain("Currency code must be a valid 3-letter ISO code");
        }

        [Fact]
        public void ValidateSymbol_WithNull_ShouldReturnError()
        {
            var result = Currency.ValidateSymbol(null!);

            result.Should().ContainSingle()
                .Which.Should().Contain("Currency symbol is required");
        }

        [Fact]
        public void ValidateName_WithNull_ShouldReturnError()
        {
            var result = Currency.ValidateName(null!);

            result.Should().ContainSingle()
                .Which.Should().Contain("Currency name is required");
        }
    }
}
