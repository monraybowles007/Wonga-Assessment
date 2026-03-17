using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WongaAuth.Api.Data;
using WongaAuth.Api.Models.Dtos;
using WongaAuth.Api.Services;
using Xunit;

namespace WongaAuth.UnitTests;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);

        var configData = new Dictionary<string, string?>
        {
            { "JwtSettings:Secret", "ThisIsASecretKeyForWongaAssessment2024!@#$%^&*" },
            { "JwtSettings:Issuer", "WongaAuth" },
            { "JwtSettings:Audience", "WongaAuthClient" },
            { "JwtSettings:ExpirationInMinutes", "60" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var logger = Mock.Of<ILogger<AuthService>>();

        _authService = new AuthService(_dbContext, configuration, logger);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "Password123"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);

        // Verify user was persisted
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "john@example.com");
        Assert.NotNull(user);
        Assert.NotEqual(request.Password, user.PasswordHash); // Password should be hashed
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "duplicate@example.com",
            Password = "Password123"
        };

        await _authService.RegisterAsync(request);

        var duplicateRequest = new RegisterRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "duplicate@example.com",
            Password = "Password456"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.RegisterAsync(duplicateRequest));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "login@example.com",
            Password = "Password123"
        };

        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "login@example.com",
            Password = "Password123"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("login@example.com", result.Email);
        Assert.Equal("John", result.FirstName);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "wrongpass@example.com",
            Password = "Password123"
        };

        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "wrongpass@example.com",
            Password = "WrongPassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(loginRequest));
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(loginRequest));
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingUser_ReturnsUserDetails()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "getuser@example.com",
            Password = "Password123"
        };

        await _authService.RegisterAsync(registerRequest);

        var user = await _dbContext.Users.FirstAsync(u => u.Email == "getuser@example.com");

        // Act
        var result = await _authService.GetUserByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("getuser@example.com", result.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Act
        var result = await _authService.GetUserByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
