using System.Text.Json.Serialization;

namespace MaksIT.Core.Security.JWS;

public class JwsMessage {
  [JsonPropertyName("protected")]
  public string Protected { get; set; } = string.Empty;

  [JsonPropertyName("payload")]
  public string Payload { get; set; } = string.Empty;

  [JsonPropertyName("signature")]
  public string Signature { get; set; } = string.Empty;
}