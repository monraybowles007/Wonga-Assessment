using System.ComponentModel.DataAnnotations;

namespace WongaAuth.Api.Models.Dtos;

/// <summary>
/// Request payload for user registration.
/// </summary>
public record RegisterRequest
{
    [Required]
    [StringLength(100)]
    public required string FirstName { get; init; }

    [Required]
    [StringLength(100)]
    public required string LastName { get; init; }

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public required string Email { get; init; }

    [Required]
    [MinLength(6)]
    public required string Password { get; init; }
}
