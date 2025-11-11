using System.Text;
using System.Security.Cryptography;
using System.Diagnostics.CodeAnalysis;
using MaksIT.Core.Security.JWK;
using MaksIT.Core.Extensions;


namespace MaksIT.Core.Security.JWS;

public static class JwsGenerator {
  public static bool TryEncode(RSA rsa, Jwk jwk, JwsHeader protectedHeader, string? payload, [NotNullWhen(true)] out JwsMessage? jwsMessage, [NotNullWhen(false)] out string? errorMessage, string? keyId = null) {
    try {
      jwsMessage = Encode(rsa, jwk, protectedHeader, payload, keyId);
      errorMessage = null;
      return true;
    } catch (Exception ex) {
      jwsMessage = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static bool TryEncode(RSA rsa, Jwk jwk, JwsHeader protectedHeader, byte[] payload, [NotNullWhen(true)] out JwsMessage? jwsMessage, [NotNullWhen(false)] out string? errorMessage, string? keyId = null) {
    try {
      jwsMessage = Encode(rsa, jwk, protectedHeader, payload, keyId);
      errorMessage = null;
      return true;
    } catch (Exception ex) {
      jwsMessage = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static bool TryEncode<T>(RSA rsa, Jwk jwk, JwsHeader protectedHeader, T payload, [NotNullWhen(true)] out JwsMessage? jwsMessage, [NotNullWhen(false)] out string? errorMessage, string? keyId = null) {
    try {
      jwsMessage = Encode(rsa, jwk, protectedHeader, payload, keyId);
      errorMessage = null;
      return true;
    } catch (Exception ex) {
      jwsMessage = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static bool TryEncode(RSA rsa, Jwk jwk, JwsHeader protectedHeader, [NotNullWhen(true)] out JwsMessage? jwsMessage, [NotNullWhen(false)] out string? errorMessage, string? keyId = null) {
    try {
      jwsMessage = Encode(rsa, jwk, protectedHeader, keyId);
      errorMessage = null;
      return true;
    } catch (Exception ex) {
      jwsMessage = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static bool TryGetKeyAuthorization(Jwk jwk, string token, [NotNullWhen(true)] out string? keyAuthorization, [NotNullWhen(false)] out string? errorMessage) {
    keyAuthorization = null;
    errorMessage = null;
    if (!JwkGenerator.TryComputeThumbprint(jwk, out var thumb, out errorMessage)) {
      return false;
    }
    keyAuthorization = $"{token}.{thumb}";
    return true;
  }

  private static JwsMessage Encode(RSA rsa, Jwk jwk, JwsHeader protectedHeader, string? payload, string? keyId = null) {
    return EncodeInternal(rsa, jwk, protectedHeader, payload, keyId);
  }

  private static JwsMessage Encode(RSA rsa, Jwk jwk, JwsHeader protectedHeader, byte[] payload, string? keyId = null) {
    string encodedPayload = Base64UrlUtility.Encode(payload);
    return EncodeInternal(rsa, jwk, protectedHeader, encodedPayload, keyId, isPayloadEncoded: true);
  }

  private static JwsMessage Encode<T>(RSA rsa, Jwk jwk, JwsHeader protectedHeader, T payload, string? keyId = null) {
    string encodedPayload = Base64UrlUtility.Encode(payload.ToJson());
    return EncodeInternal(rsa, jwk, protectedHeader, encodedPayload, keyId, isPayloadEncoded: true);
  }

  private static JwsMessage Encode(RSA rsa, Jwk jwk, JwsHeader protectedHeader, string? keyId = null) {
    // POST-as-GET: empty payload
    return EncodeInternal(rsa, jwk, protectedHeader, null, keyId);
  }

  private static JwsMessage EncodeInternal(
      RSA rsa,
      Jwk jwk,
      JwsHeader protectedHeader,
      string? payload,
      string? keyId,
      bool isPayloadEncoded = false) {
    protectedHeader.Algorithm = JwkAlgorithm.Rs256.Name;

    if (!string.IsNullOrEmpty(keyId))
      protectedHeader.KeyId = keyId;
    else
      protectedHeader.Key = jwk;

    var message = new JwsMessage {
      Payload = string.Empty,
      Protected = Base64UrlUtility.Encode(protectedHeader.ToJson())
    };

    if (payload != null) {
      message.Payload = isPayloadEncoded ? payload : Base64UrlUtility.Encode(payload);
    }

    var signingInput = Encoding.ASCII.GetBytes($"{message.Protected}.{message.Payload}");
    message.Signature = Base64UrlUtility.Encode(
        rsa.SignData(signingInput, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));

    return message;
  }
}
