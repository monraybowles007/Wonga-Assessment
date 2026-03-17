namespace WongaAuth.Api.Models.Dtos;

/// <summary>
/// Response containing the authenticated user's profile details.
/// </summary>
public record UserDetailsResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt);
