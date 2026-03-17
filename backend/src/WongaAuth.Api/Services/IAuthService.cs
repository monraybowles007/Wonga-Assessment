using WongaAuth.Api.Models.Dtos;

namespace WongaAuth.Api.Services;

/// <summary>
/// Defines authentication operations for user registration, login, and profile retrieval.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">The registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An authentication response containing the JWT token and user info.</returns>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates an existing user with email and password.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An authentication response containing the JWT token and user info.</returns>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves user details by their unique identifier.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's profile details if found; otherwise, null.</returns>
    Task<UserDetailsResponse?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
