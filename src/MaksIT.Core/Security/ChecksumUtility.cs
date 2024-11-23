using System.Diagnostics.CodeAnalysis;

namespace MaksIT.Core.Security;


public static class ChecksumUtility {
  public static bool TryCalculateCRC32Checksum(
    byte[] data,
    [NotNullWhen(true)] out string? checksum,
    [NotNullWhen(false)] out string? errorMessage
  ) {
    if (Crc32.TryCompute(data, out var result, out errorMessage)) {
      checksum = BitConverter.ToString(BitConverter.GetBytes(result)).Replace("-", "").ToLower();
      return true;
    }
    checksum = null;
    return false;
  }

  public static bool TryCalculateCRC32ChecksumFromFile(string filePath, out string? checksum, out string? errorMessage) {
    try {
      using var crc32 = new Crc32();
      using var stream = File.OpenRead(filePath);
      var hashBytes = crc32.ComputeHash(stream);
      checksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      checksum = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static bool TryCalculateCRC32ChecksumFromFileInChunks(
    string filePath,
    [NotNullWhen(true)] out string? checksum,
    [NotNullWhen(false)] out string? errorMessage,
    int chunkSize = 8192
  ) {
    try {
      using var crc32 = new Crc32();
      using var stream = File.OpenRead(filePath);
      var buffer = new byte[chunkSize];
      int bytesRead;
      while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0) {
        crc32.TransformBlock(buffer, 0, bytesRead, null, 0);
      }
      crc32.TransformFinalBlock(buffer, 0, 0);
      var hashBytes = crc32.Hash;
      checksum = BitConverter.ToString(hashBytes ?? Array.Empty<byte>()).Replace("-", "").ToLower();
      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      checksum = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static bool VerifyCRC32Checksum(byte[] data, string expectedChecksum) {
    return TryCalculateCRC32Checksum(data, out var calculatedChecksum, out _) &&
           string.Equals(calculatedChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase);
  }

  public static bool VerifyCRC32ChecksumFromFile(string filePath, string expectedChecksum) {
    return TryCalculateCRC32ChecksumFromFile(filePath, out var calculatedChecksum, out _) &&
           string.Equals(calculatedChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase);
  }

  public static bool VerifyCRC32ChecksumFromFileInChunks(string filePath, string expectedChecksum, int chunkSize = 8192) {
    return TryCalculateCRC32ChecksumFromFileInChunks(filePath, out var calculatedChecksum, out _, chunkSize) &&
           string.Equals(calculatedChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase);
  }
}
