using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WongaAuth.Api.Data;
using WongaAuth.Api.Models.Dtos;
using Xunit;

namespace WongaAuth.IntegrationTests;

public class AuthControllerTests : IClassFixture<AuthControllerTests.CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "Integration",
            LastName = "Test",
            Email = $"register-{Guid.NewGuid()}@example.com",
            Password = "Password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
        Assert.NotNull(content);
        Assert.NotEmpty(content.Token);
        Assert.Equal(request.Email.ToLowerInvariant(), content.Email);
        Assert.Equal("Integration", content.FirstName);
        Assert.Equal("Test", content.LastName);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange - first register a user
        var email = $"login-{Guid.NewGuid()}@example.com";
        var registerRequest = new RegisterRequest
        {
            FirstName = "Login",
            LastName = "Test",
            Email = email,
            Password = "Password123"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = "Password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
        Assert.NotNull(content);
        Assert.NotEmpty(content.Token);
        Assert.Equal(email.ToLowerInvariant(), content.Email);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsUserDetails()
    {
        // Arrange - register to get a token
        var email = $"me-{Guid.NewGuid()}@example.com";
        var registerRequest = new RegisterRequest
        {
            FirstName = "Me",
            LastName = "Endpoint",
            Email = email,
            Password = "Password123"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);

        Assert.NotNull(authResult);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<UserDetailsResponse>(_jsonOptions);
        Assert.NotNull(content);
        Assert.Equal("Me", content.FirstName);
        Assert.Equal("Endpoint", content.LastName);
        Assert.Equal(email.ToLowerInvariant(), content.Email);
        Assert.NotEqual(Guid.Empty, content.Id);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var email = $"dup-{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest
        {
            FirstName = "Dup",
            LastName = "Test",
            Email = email,
            Password = "Password123"
        };

        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Act - try registering again with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        var email = $"wrongpw-{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FirstName = "Wrong",
            LastName = "Password",
            Email = email,
            Password = "Password123"
        });

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Custom web application factory that replaces PostgreSQL with an in-memory database.
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("WongaAuthTestDb");
                });
            });
        }
    }
}
