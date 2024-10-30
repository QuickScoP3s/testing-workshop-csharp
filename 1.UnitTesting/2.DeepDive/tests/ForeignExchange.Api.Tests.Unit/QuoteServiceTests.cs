using Castle.Core.Logging;
using FluentAssertions;
using ForeignExchange.Api.Database;
using ForeignExchange.Api.Logging;
using ForeignExchange.Api.Models;
using ForeignExchange.Api.Repositories;
using ForeignExchange.Api.Services;
using ForeignExchange.Api.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace ForeignExchange.Api.Tests.Unit;

public class QuoteServiceTests
{
    private readonly QuoteService _sut;
    private readonly IRatesRepository _ratesRepository = Substitute.For<IRatesRepository>();
    private readonly ILoggerAdapter<QuoteService> _logger = Substitute.For<ILoggerAdapter<QuoteService>>();

    public QuoteServiceTests()
    {
        _sut = new QuoteService(
            _ratesRepository,
            _logger
        );
    }
    
    [Fact]
    public async Task GetQuoteAsync_ShouldReturnQuote_WhenCurrenciesAreValid()
    {
        // Arrange
        var fromCurrency = "USD";
        var toCurrency = "GBP";
        var amount = 100;

        _ratesRepository.GetRateAsync(fromCurrency, toCurrency)
            .Returns(new FxRate
            {
                Rate = 1.6m,
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                TimestampUtc = DateTime.UtcNow
            });
        
        var expectedResult = new ConversionQuote
        {
            BaseCurrency = fromCurrency,
            QuoteCurrency = toCurrency,
            BaseAmount = 100,
            QuoteAmount = 160
        };

        // Act
        var result = await _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public async Task GetQuoteAsync_ShouldLogAppropriateMessage_WhenCurrenciesAreValid()
    {
        // Arrange
        var fromCurrency = "USD";
        var toCurrency = "GBP";
        var amount = 100;

        _ratesRepository.GetRateAsync(fromCurrency, toCurrency)
            .Returns(new FxRate
            {
                Rate = 1.6m,
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                TimestampUtc = DateTime.UtcNow
            });

        // Act
        await _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);

        // Assert
        _logger.Received(1).LogInformation(
            "Retrieved quote for currencies {FromCurrency}->{ToCurrency} in {ElapsedMilliseconds}ms",
            Arg.Any<object[]>());
    }
    
    [Fact]
    public async Task GetQuoteAsync_ShouldThrowNegativeAmountException_WhenAmountIsNegative()
    {
        // Arrange
        var fromCurrency = "USD";
        var toCurrency = "GBP";
        var amount = -1;

        // Act
        var action = () => _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);

        // Assert
        await action.Should().ThrowAsync<NegativeAmountException>()
            .WithMessage("You can only convert a positive amount of money");
    }
}
