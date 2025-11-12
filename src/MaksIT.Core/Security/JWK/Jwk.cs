using System.Text.Json.Serialization;


namespace MaksIT.Core.Security.JWK;


public class Jwk {
  #region Common fields
  /// <summary>
  /// "kty" (Key Type) Parameter
  /// <para>
  /// The "kty" (key type) parameter identifies the cryptographic algorithm
  /// family used with the key, such as "RSA" or "EC".
  /// </para>
  /// </summary>
  [JsonPropertyName("kty")]
  public string? KeyType { get; set; }

  /// <summary>
  /// "kid" (Key ID) Parameter
  /// <para>
  /// The "kid" (key ID) parameter is used to match a specific key. This
  /// is used, for instance, to choose among a set of keys within a JWK Set
  /// during key rollover. The structure of the "kid" value is
  /// unspecified.
  /// </para>
  /// </summary>
  [JsonPropertyName("kid")]
  public string? KeyId { get; set; }

  /// <summary>
  /// "alg" (Algorithm) Parameter
  /// <para>
  /// The "alg" (algorithm) parameter identifies the algorithm intended for
  /// use with the key.
  /// </para>
  /// </summary>
  [JsonPropertyName("alg")]
  public string? Algorithm { get; set; }

  /// <summary>
  /// "use" (Public Key Use) Parameter
  /// <para>
  /// The "use" (public key use) parameter identifies the intended use of
  /// the public key. The "use" parameter is employed to indicate whether
  /// a public key is used for encrypting data or verifying the signature
  /// on data.
  /// </para>
  /// </summary>
  [JsonPropertyName("use")]
  public string? KeyUse { get; set; }

  /// <summary>
  /// "key_ops" (Key Operations) Parameter
  /// <para>
  /// The "key_ops" (key operations) parameter identifies the operation(s) for which the key is intended to be used.
  /// </para>
  /// </summary>
  [JsonPropertyName("key_ops")]
  public string[]? KeyOperations { get; set; }
  #endregion

  #region RSA fields
  /// <summary>
  /// The modulus value for the public RSA key. It is represented as the Base64URL encoding of value's big endian representation.
  /// </summary>
  [JsonPropertyName("n")]
  public string? RsaModulus { get; set; }

  /// <summary>
  /// The exponent value for the public RSA key. It is represented as the Base64URL encoding of value's big endian representation.
  /// </summary>
  [JsonPropertyName("e")]
  public string? RsaExponent { get; set; }

  /// <summary>
  /// The first prime factor. It is represented as the Base64URL encoding of the value's big endian representation.
  /// </summary>
  [JsonPropertyName("p")]
  public string? RsaFirstPrimeFactor { get; set; }

  /// <summary>
  /// The second prime factor. It is represented as the Base64URL encoding of the value's big endian representation.
  /// </summary>
  [JsonPropertyName("q")]
  public string? RsaSecondPrimeFactor { get; set; }

  /// <summary>
  /// The first factor Chinese Remainder Theorem exponent. It is represented as the Base64URL encoding of the value's big endian representation.
  /// </summary>
  [JsonPropertyName("dp")]
  public string? RsaFirstFactorCRTExponent { get; set; }

  /// <summary>
  /// The second factor Chinese Remainder Theorem exponent. It is represented as the Base64URL encoding of the value's big endian representation.
  /// </summary>
  [JsonPropertyName("dq")]
  public string? RsaSecondFactorCRTExponent { get; set; }

  /// <summary>
  /// The first Chinese Remainder Theorem coefficient. It is represented as the Base64URL encoding of the value's big endian representation.
  /// </summary>
  [JsonPropertyName("qi")]
  public string? RsaFirstCRTCoefficient { get; set; }

  /// <summary>
  /// The other primes information, should they exist, null or an empty list if not specified.
  /// </summary>
  [JsonPropertyName("oth")]
  public List<OtherPrimeInfo>? RsaOtherPrimesInfo { get; set; }
  #endregion

  #region EC fields
  /// <summary>
  /// The "crv" (Curve) parameter identifies the cryptographic curve used with the key.
  /// </summary>
  [JsonPropertyName("crv")]
  public string? EcCurve { get; set; }

  /// <summary>
  /// The "x" coordinate for the EC public key. It is represented as the Base64URL encoding of the coordinate's big endian representation.
  /// </summary>
  [JsonPropertyName("x")]
  public string? EcX { get; set; }

  /// <summary>
  /// The "y" coordinate for the EC public key. It is represented as the Base64URL encoding of the coordinate's big endian representation.
  /// </summary>
  [JsonPropertyName("y")]
  public string? EcY { get; set; }
  #endregion

  #region Private Key field
  /// <summary>
  /// The private key value ("d"). Used for RSA (private exponent) and EC (private key).
  /// RFC 7518 uses "d" for both.
  /// </summary>
  [JsonPropertyName("d")]
  public string? PrivateKey { get; set; }
  #endregion

  #region Symmetric (octet) fields
  /// <summary>
  /// The symmetric (octet) key value. It is represented as the Base64URL encoding of the value's big endian representation.
  /// </summary>
  [JsonPropertyName("k")]
  public string? SymmetricKey { get; set; }
  #endregion
}

/// <summary>
/// Represents an entry in the 'oth' (Other Primes Info) parameter for multi-prime RSA keys.
/// </summary>
public class OtherPrimeInfo {
  #region OtherPrimeInfo fields
  /// <summary>
  /// The value of the other prime factor.
  /// </summary>
  [JsonPropertyName("r")]
  public string? PrimeFactor { get; set; }

  /// <summary>
  /// The CRT exponent of the other prime factor.
  /// </summary>
  [JsonPropertyName("d")]
  public string? FactorCRTExponent { get; set; }

  /// <summary>
  /// The CRT coefficient of the other prime factor.
  /// </summary>
  [JsonPropertyName("t")]
  public string? FactorCRTCoefficient { get; set; }
  #endregion
}