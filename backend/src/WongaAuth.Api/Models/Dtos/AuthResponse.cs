namespace WongaAuth.Api.Models.Dtos;

/// <summary>
/// Response returned after successful authentication (register or login).
/// </summary>
public record AuthResponse(string Token, string Email, string FirstName, string LastName);
