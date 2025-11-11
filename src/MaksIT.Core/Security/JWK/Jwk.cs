using System.Text.Json.Serialization;


namespace MaksIT.Core.Security.JWK;

/// <summary>
/// Standard JWK class supporting RSA, EC, and octet keys.
/// </summary>
public class Jwk {
    // Common fields
    [JsonPropertyName("kty")]
    public string? Kty { get; set; }

    [JsonPropertyName("kid")]
    public string? Kid { get; set; }

    [JsonPropertyName("alg")]
    public string? Alg { get; set; }

    [JsonPropertyName("use")]
    public string? Use { get; set; }

    [JsonPropertyName("key_ops")]
    public string[]? KeyOps { get; set; }

    // RSA fields
    [JsonPropertyName("n")]
    public string? N { get; set; } // Modulus

    [JsonPropertyName("e")]
    public string? E { get; set; } // Exponent

    [JsonPropertyName("d")]
    public string? D { get; set; } // Private exponent

    [JsonPropertyName("p")]
    public string? P { get; set; }

    [JsonPropertyName("q")]
    public string? Q { get; set; }

    [JsonPropertyName("dp")]
    public string? DP { get; set; }

    [JsonPropertyName("dq")]
    public string? DQ { get; set; }

    [JsonPropertyName("qi")]
    public string? QI { get; set; }

    // EC fields
    [JsonPropertyName("crv")]
    public string? Crv { get; set; }

    [JsonPropertyName("x")]
    public string? X { get; set; }

    [JsonPropertyName("y")]
    public string? Y { get; set; }

    [JsonPropertyName("d_ec")]
    public string? D_EC { get; set; } // EC private key

    // Symmetric (octet) fields
    [JsonPropertyName("k")]
    public string? K { get; set; }

    // Backward compatibility for old code
    [JsonIgnore]
    public string? Exponent { get => E; set => E = value; }
    [JsonIgnore]
    public string? Modulus { get => N; set => N = value; }
}