using System.Text;
using System.Security.Cryptography;
using System.Diagnostics.CodeAnalysis;
using MaksIT.Core.Security.JWK;
using MaksIT.Core.Extensions;


namespace MaksIT.Core.Security.JWS;

public static class JwsGenerator {
  public static bool TryEncode(
    RSA rsa,
    Jwk jwk,
    JwsHeader protectedHeader,
    out JwsMessage? message,
    [NotNullWhen(false)] out string? errorMessage
  ) => TryEncode<string>(rsa, jwk, protectedHeader, null, out message, out errorMessage);


  public static bool TryEncode<T>(
    RSA rsa,
    Jwk jwk,
    JwsHeader protectedHeader,
    T? payload,
    out JwsMessage? message,
    [NotNullWhen(false)] out string? errorMessage
  ) {
    try {
      protectedHeader.Algorithm = JwkAlgorithm.Rs256.Name;

      if (jwk.KeyId != null)
        protectedHeader.KeyId = jwk.KeyId;
      else
        protectedHeader.Key = jwk;

      var msg = new JwsMessage {
        Payload = "",
        Protected = Base64UrlUtility.Encode(protectedHeader.ToJson())
      };

      if (payload != null) {
        if (payload is string stringPayload)
          msg.Payload = Base64UrlUtility.Encode(stringPayload);
        else
          msg.Payload = Base64UrlUtility.Encode(payload.ToJson());
      }

      var signature = rsa.SignData(
          Encoding.ASCII.GetBytes($"{msg.Protected}.{msg.Payload}"),
          HashAlgorithmName.SHA256,
          RSASignaturePadding.Pkcs1
      );

      msg.Signature = Base64UrlUtility.Encode(signature);

      message = msg;
      errorMessage = null;

      return true;
    }
    catch (Exception ex) {
      message = null;
      errorMessage = ex.Message;
      return false;
    }
  }
}
