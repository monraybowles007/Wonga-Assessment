using System.ComponentModel.DataAnnotations;

namespace WongaAuth.Api.Models.Dtos;

/// <summary>
/// Request payload for user login.
/// </summary>
public record LoginRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}
