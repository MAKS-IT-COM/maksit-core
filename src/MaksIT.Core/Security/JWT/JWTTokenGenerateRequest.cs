namespace MaksIT.Core.Security.JWT;

public class JWTTokenGenerateRequest {

  /// <summary>
  /// Secret key used for signing the JWT token. Must be kept secure and not shared publicly.
  /// </summary>
  public required string Secret { get; set; }

  /// <summary>
  /// Issuer of the JWT token, typically the application or service that generates the token.
  /// </summary>
  public required string Issuer { get; set; }

  /// <summary>
  /// Gets or sets the audience for the application.
  /// </summary>
  public required string Audience { get; set; }

  /// <summary>
  /// Expiration time in minutes.
  /// </summary>
  public double Expiration { get; set; }

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

}