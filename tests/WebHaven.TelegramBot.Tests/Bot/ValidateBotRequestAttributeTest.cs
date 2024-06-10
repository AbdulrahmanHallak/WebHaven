using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using WebHaven.TelegramBot.Bot;

namespace WebHaven.TelegramBot.Tests.Bot;

public class ValidateBotRequestAttributeTest
{

    [Fact]
    public async void Request_with_valid_secret_passes()
    {
        // Arrange
        var secret = "Very Important secret";
        var configs = new BotConfigs() { Secret = secret };

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Telegram-Bot-Api-Secret-Token"] = secret;

        var invocationContext = Substitute.For<EndpointFilterInvocationContext>();
        invocationContext.HttpContext.Returns(httpContext);

        var next = Substitute.For<EndpointFilterDelegate>();

        var filter = new ValidateBotRequestAttribute(configs);

        // Act
        await filter.InvokeAsync(invocationContext, next);

        // Assert
        await next.Received(1).Invoke(invocationContext);
    }

    [Fact]
    public async Task Request_with_invalid_secret_returns_403()
    {
        // Arrange
        var secret = "Very Important secret";
        var configs = new BotConfigs { Secret = secret };

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Telegram-Bot-Api-Secret-Token"] = "Wrong secret";

        var invocationContextMock = Substitute.For<EndpointFilterInvocationContext>();
        invocationContextMock.HttpContext.Returns(httpContext);

        var nextMock = Substitute.For<EndpointFilterDelegate>();

        var filter = new ValidateBotRequestAttribute(configs);

        // Act
        var result = await filter.InvokeAsync(invocationContextMock, nextMock);

        // Assert
        await nextMock.DidNotReceive().Invoke(invocationContextMock);
        var problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(403, problemResult.StatusCode);
    }
}
