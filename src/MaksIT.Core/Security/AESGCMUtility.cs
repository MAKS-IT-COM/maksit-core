using System.Security.Cryptography;

namespace MaksIT.Core.Security;

/// <summary>
/// 
/// </summary>
public static class AESGCMUtility {
  private const int IvLength = 12; // 12 bytes for AES-GCM IV
  private const int TagLength = 16; // 16 bytes for AES-GCM Tag

  public static bool TryEncryptData(byte[] data, string base64Key, out byte[]? result, out string? errorMessage) {
    try {
      var key = Convert.FromBase64String(base64Key);
      using (AesGcm aesGcm = new AesGcm(key, AesGcm.TagByteSizes.MaxSize)) {
        var iv = new byte[IvLength];
        RandomNumberGenerator.Fill(iv);

        var cipherText = new byte[data.Length];
        var tag = new byte[TagLength];

        aesGcm.Encrypt(iv, data, cipherText, tag);

        // Concatenate cipherText, tag, and iv
        result = new byte[cipherText.Length + tag.Length + iv.Length];
        Buffer.BlockCopy(cipherText, 0, result, 0, cipherText.Length);
        Buffer.BlockCopy(tag, 0, result, cipherText.Length, tag.Length);
        Buffer.BlockCopy(iv, 0, result, cipherText.Length + tag.Length, iv.Length);

        errorMessage = null;
        return true;
      }
    }
    catch (Exception ex) when (ex is FormatException || ex is CryptographicException) {
      result = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static bool TryDecryptData(byte[] data, string base64Key, out byte[]? decryptedData, out string? errorMessage) {
    try {
      var key = Convert.FromBase64String(base64Key);

      // Extract cipherText, tag, and iv
      var cipherTextLength = data.Length - IvLength - TagLength;

      var cipherText = new byte[cipherTextLength];
      var tag = new byte[TagLength];
      var iv = new byte[IvLength];

      Buffer.BlockCopy(data, 0, cipherText, 0, cipherTextLength);
      Buffer.BlockCopy(data, cipherTextLength, tag, 0, TagLength);
      Buffer.BlockCopy(data, cipherTextLength + TagLength, iv, 0, IvLength);

      using (AesGcm aesGcm = new AesGcm(key, AesGcm.TagByteSizes.MaxSize)) {
        decryptedData = new byte[cipherText.Length];
        aesGcm.Decrypt(iv, cipherText, tag, decryptedData);
        errorMessage = null;
        return true;
      }
    }
    catch (Exception ex) when (ex is FormatException || ex is CryptographicException) {
      decryptedData = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static string GenerateKeyBase64() {
    var key = new byte[32]; // 256-bit key for AES-256
    RandomNumberGenerator.Fill(key);
    return Convert.ToBase64String(key);
  }
}
