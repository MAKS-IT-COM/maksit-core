namespace MaksIT.Core.Security.JWT;

public class JWTTokenClaims {

  /// <summary>
  /// Gets or sets the unique identifier for the user.
  /// </summary>
  public string? UserId { get; set; }

  /// <summary>
  /// Gets or sets the username associated with the user.
  /// </summary>
  public string? Username { get; set; }

  /// <summary>
  /// Gets or sets the list of roles associated with the current entity.
  /// </summary>
  public List<string>? Roles { get; set; }

  public List<string>? AclEntries { get; set; }

  /// <summary>
  /// Gets or sets the date and time when the token was issued.
  /// </summary>
  public DateTime? IssuedAt { get; set; }

  /// <summary>
  /// Gets or sets the date and time when the token expires.
  /// </summary>
  public DateTime? ExpiresAt { get; set; }
}