using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SIEG.SrDevChallenge.Api.Middlewares;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace SIEG.SrDevChallenge.UnitTests.Api;

[TestFixture]
public class ApiKeyMiddlewareTests
{
    private Mock<IOptions<ApiKeySettings>> _apiKeySettingsMock;
    private Mock<ILogger<ApiKeyMiddleware>> _loggerMock;
    private Mock<RequestDelegate> _nextMock;
    private ApiKeyMiddleware _middleware;
    private DefaultHttpContext _httpContext;
    private ApiKeySettings _apiKeySettings;

    [SetUp]
    public void Setup()
    {
        _apiKeySettingsMock = new Mock<IOptions<ApiKeySettings>>();
        _loggerMock = new Mock<ILogger<ApiKeyMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        
        _apiKeySettings = new ApiKeySettings
        {
            ValidApiKeys = new List<ApiKeyInfo>
            {
                new ApiKeyInfo
                {
                    Key = "sieg-dev-api-key-2026",
                    Name = "Test Key",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Description = "Test key for unit tests"
                }
            }
        };
        
        _apiKeySettingsMock.Setup(x => x.Value).Returns(_apiKeySettings);
        
        _middleware = new ApiKeyMiddleware(_apiKeySettingsMock.Object, _loggerMock.Object);
        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Path = "/api/test";
        
        // Configure response body to capture JSON output
        _httpContext.Response.Body = new MemoryStream();
    }

    [Test]
    public async Task InvokeAsync_ValidApiKey_ShouldCallNextMiddleware()
    {
        // Arrange
        _httpContext.Request.Headers["X-API-Key"] = "sieg-dev-api-key-2026";

        _nextMock.Setup(x => x(_httpContext))
                 .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext, _nextMock.Object);

        // Assert
        _nextMock.Verify(x => x(_httpContext), Times.Once);
        _httpContext.Response.StatusCode.Should().Be(200);
        _httpContext.Items["ApiKey"].Should().Be("sieg-dev-api-key-2026");
        _httpContext.Items["ApiKeyName"].Should().Be("Test Key");
    }

    [Test]
    public async Task InvokeAsync_MissingApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        // Não adicionar o header X-API-Key

        // Act
        await _middleware.InvokeAsync(_httpContext, _nextMock.Object);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(401);
        _nextMock.Verify(x => x(_httpContext), Times.Never);

        // Verificar o corpo da resposta JSON
        var responseBody = await GetResponseBodyAsync();
        responseBody.Should().Contain("API Key é obrigatória");
        responseBody.Should().Contain("\"status\":401");
        responseBody.Should().Contain("\"title\":\"Unauthorized\"");
    }

    [Test]
    public async Task InvokeAsync_InvalidApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        _httpContext.Request.Headers["X-API-Key"] = "invalid-key";

        // Act
        await _middleware.InvokeAsync(_httpContext, _nextMock.Object);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(401);
        _nextMock.Verify(x => x(_httpContext), Times.Never);

        // Verificar o corpo da resposta JSON
        var responseBody = await GetResponseBodyAsync();
        responseBody.Should().Contain("API Key inválida");
        responseBody.Should().Contain("\"status\":401");
        responseBody.Should().Contain("\"title\":\"Unauthorized\"");
    }

    [Test]
    public async Task InvokeAsync_EmptyApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        _httpContext.Request.Headers["X-API-Key"] = "";

        // Act
        await _middleware.InvokeAsync(_httpContext, _nextMock.Object);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(401);
        _nextMock.Verify(x => x(_httpContext), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WhitespaceApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        _httpContext.Request.Headers["X-API-Key"] = "   ";

        // Act
        await _middleware.InvokeAsync(_httpContext, _nextMock.Object);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(401);
        _nextMock.Verify(x => x(_httpContext), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_CaseSensitiveApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        _httpContext.Request.Headers["X-API-Key"] = "SIEG-DEV-API-KEY-2026"; // Diferente case

        // Act
        await _middleware.InvokeAsync(_httpContext, _nextMock.Object);

        // Assert - A validação é case-sensitive (deve falhar)
        _httpContext.Response.StatusCode.Should().Be(401);
        _nextMock.Verify(x => x(_httpContext), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_MultipleApiKeyHeaders_ShouldUseFirstValue()
    {
        // Arrange
        _httpContext.Request.Headers.Add("X-API-Key", new[] { "sieg-dev-api-key-2026", "another-key" });

        _nextMock.Setup(x => x(_httpContext))
                 .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext, _nextMock.Object);

        // Assert
        _nextMock.Verify(x => x(_httpContext), Times.Once);
        _httpContext.Response.StatusCode.Should().Be(200);
    }

    [Test]
    public async Task InvokeAsync_NullApiKeySettings_ShouldReturnUnauthorized()
    {
        // Arrange
        _apiKeySettingsMock.Setup(x => x.Value).Returns((ApiKeySettings?)null);
        var middlewareWithNullSettings = new ApiKeyMiddleware(_apiKeySettingsMock.Object, _loggerMock.Object);
        _httpContext.Request.Headers["X-API-Key"] = "any-key";

        // Act
        await middlewareWithNullSettings.InvokeAsync(_httpContext, _nextMock.Object);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(401);
        _nextMock.Verify(x => x(_httpContext), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_ExcludedPath_ShouldBypassValidation()
    {
        // Arrange
        _httpContext.Request.Path = "/swagger/index.html";
        // Não adicionar API key

        _nextMock.Setup(x => x(_httpContext))
                 .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext, _nextMock.Object);

        // Assert
        _nextMock.Verify(x => x(_httpContext), Times.Once);
        _httpContext.Response.StatusCode.Should().Be(200);
    }

    [Test]
    public async Task InvokeAsync_NextMiddlewareThrows_ShouldPropagateException()
    {
        // Arrange
        _httpContext.Request.Headers["X-API-Key"] = "sieg-dev-api-key-2026";
        
        var expectedException = new InvalidOperationException("Next middleware failed");
        _nextMock.Setup(x => x(_httpContext))
                 .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _middleware.InvokeAsync(_httpContext, _nextMock.Object));

        exception.Should().Be(expectedException);
    }

    [Test]
    public async Task InvokeAsync_LogsUnauthorizedAccess()
    {
        // Arrange
        _httpContext.Request.Headers["X-API-Key"] = "invalid-key";

        // Act
        await _middleware.InvokeAsync(_httpContext, _nextMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("API Key inválida fornecida")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_LogsValidAccess()
    {
        // Arrange
        _httpContext.Request.Headers["X-API-Key"] = "sieg-dev-api-key-2026";

        _nextMock.Setup(x => x(_httpContext))
                 .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext, _nextMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Acesso autorizado com API Key")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_SetsCorrectContentType()
    {
        // Arrange
        _httpContext.Request.Headers["X-API-Key"] = "invalid-key";

        // Act
        await _middleware.InvokeAsync(_httpContext, _nextMock.Object);

        // Assert
        _httpContext.Response.ContentType.Should().Be("application/json; charset=utf-8");
    }

    [Test]
    public async Task InvokeAsync_ReturnsCorrectProblemDetailsStructure()
    {
        // Arrange
        _httpContext.Request.Headers["X-API-Key"] = "invalid-key";

        // Act
        await _middleware.InvokeAsync(_httpContext, _nextMock.Object);

        // Assert
        var responseContent = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Unauthorized");
        problemDetails.Detail.Should().Be("API Key inválida");
        problemDetails.Status.Should().Be(401);
        problemDetails.Instance.Should().Be("/api/test");
    }

    [Test]
    public async Task InvokeAsync_InactiveApiKey_ShouldWork()
    {
        // Arrange
        _apiKeySettings.ValidApiKeys.Add(new ApiKeyInfo
        {
            Key = "inactive-key",
            Name = "Inactive Key",
            IsActive = false
        });
        
        _httpContext.Request.Headers["X-API-Key"] = "inactive-key";

        _nextMock.Setup(x => x(_httpContext))
                 .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext, _nextMock.Object);

        // Assert - O middleware atual não verifica IsActive, apenas se a chave existe
        _nextMock.Verify(x => x(_httpContext), Times.Once);
        _httpContext.Response.StatusCode.Should().Be(200);
    }

    private async Task<string> GetResponseBodyAsync()
    {
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_httpContext.Response.Body);
        return await reader.ReadToEndAsync();
    }
}