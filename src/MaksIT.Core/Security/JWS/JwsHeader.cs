using System.Text.Json.Serialization;


namespace MaksIT.Core.Security.JWS;

public class JwsHeader {
  [JsonPropertyName("alg")]
  public string? Algorithm { get; set; }

  [JsonPropertyName("kid")]
  public string? KeyId { get; set; }

  [JsonPropertyName("jwk")]
  public object? Key { get; set; }
}