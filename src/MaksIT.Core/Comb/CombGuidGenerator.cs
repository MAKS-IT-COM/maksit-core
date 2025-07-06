using System.Buffers.Binary;


namespace MaksIT.Core.Comb;

/// <summary>
/// Specifies the layout strategy used to embed a timestamp in a COMB GUID.
/// </summary>
public enum CombGuidType {
  /// <summary>
  /// COMB GUID format compatible with SQL Server (timestamp in bytes 8–15).
  /// </summary>
  SqlServer,

  /// <summary>
  /// COMB GUID format compatible with PostgreSQL (timestamp in bytes 0–7).
  /// </summary>
  PostgreSql
}

/// <summary>
/// Provides methods to generate and extract COMB GUIDs with embedded timestamps.
/// COMB GUIDs improve index locality by combining randomness with a sortable timestamp.
/// </summary>
public static class CombGuidGenerator {
  private const int TimestampByteLength = 8;
  private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

  /// <summary>
  /// Generates a COMB GUID using the specified base GUID, timestamp, and format type.
  /// </summary>
  /// <param name="baseGuid">The base GUID to embed the timestamp into.</param>
  /// <param name="timestamp">The UTC timestamp to embed in the GUID.</param>
  /// <param name="type">The COMB GUID format to use.</param>
  /// <returns>The generated COMB GUID.</returns>
  public static Guid CreateCombGuid(Guid baseGuid, DateTime timestamp, CombGuidType type) {
    return type switch {
      CombGuidType.SqlServer => CreateSqlServerCombGuid(baseGuid, timestamp),
      CombGuidType.PostgreSql => CreatePostgreSqlCombGuid(baseGuid, timestamp),
      _ => throw new ArgumentOutOfRangeException(nameof(type), "Unsupported COMB GUID type.")
    };
  }

  /// <summary>
  /// Generates a COMB GUID using a random GUID and a specified UTC timestamp.
  /// </summary>
  /// <param name="timestamp">The UTC timestamp to embed in the GUID.</param>
  /// <param name="type">The COMB GUID format to use.</param>
  /// <returns>The generated COMB GUID.</returns>
  public static Guid CreateCombGuid(DateTime timestamp, CombGuidType type) =>
      CreateCombGuid(Guid.NewGuid(), timestamp, type);

  /// <summary>
  /// Generates a COMB GUID using a specified base GUID and the current UTC timestamp.
  /// </summary>
  /// <param name="baseGuid">The base GUID to embed the timestamp into.</param>
  /// <param name="type">The COMB GUID format to use.</param>
  /// <returns>The generated COMB GUID.</returns>
  public static Guid CreateCombGuid(Guid baseGuid, CombGuidType type) =>
      CreateCombGuid(baseGuid, DateTime.UtcNow, type);

  /// <summary>
  /// Extracts the embedded UTC timestamp from a COMB GUID using the specified format.
  /// </summary>
  /// <param name="combGuid">The COMB GUID containing the timestamp.</param>
  /// <param name="type">The COMB GUID format used during creation.</param>
  /// <returns>The extracted UTC timestamp.</returns>
  public static DateTime ExtractTimestamp(Guid combGuid, CombGuidType type) {
    Span<byte> guidBytes = stackalloc byte[16];
    combGuid.TryWriteBytes(guidBytes);

    return type switch {
      CombGuidType.SqlServer => ReadTimestampFromBytes(guidBytes.Slice(8, TimestampByteLength)),
      CombGuidType.PostgreSql => ReadTimestampFromBytes(guidBytes.Slice(0, TimestampByteLength)),
      _ => throw new ArgumentOutOfRangeException(nameof(type), "Unsupported COMB GUID type.")
    };
  }

  /// <summary>
  /// Creates a COMB GUID compatible with SQL Server by embedding the timestamp in bytes 8–15.
  /// </summary>
  /// <param name="baseGuid">The base GUID.</param>
  /// <param name="timestamp">The UTC timestamp.</param>
  /// <returns>The resulting COMB GUID.</returns>
  private static Guid CreateSqlServerCombGuid(Guid baseGuid, DateTime timestamp) {
    Span<byte> guidBytes = stackalloc byte[16];
    baseGuid.TryWriteBytes(guidBytes);
    WriteTimestampBytes(guidBytes.Slice(8, TimestampByteLength), timestamp);
    return new Guid(guidBytes);
  }

  /// <summary>
  /// Creates a COMB GUID compatible with PostgreSQL by embedding the timestamp in bytes 0–7.
  /// </summary>
  /// <param name="baseGuid">The base GUID.</param>
  /// <param name="timestamp">The UTC timestamp.</param>
  /// <returns>The resulting COMB GUID.</returns>
  private static Guid CreatePostgreSqlCombGuid(Guid baseGuid, DateTime timestamp) {
    Span<byte> baseBytes = stackalloc byte[16];
    baseGuid.TryWriteBytes(baseBytes);

    Span<byte> finalBytes = stackalloc byte[16];
    // first 8 bytes = timestamp
    WriteTimestampBytes(finalBytes.Slice(0, TimestampByteLength), timestamp);
    // remaining 8 bytes = random tail
    baseBytes.Slice(TimestampByteLength, 16 - TimestampByteLength)
             .CopyTo(finalBytes.Slice(TimestampByteLength, 16 - TimestampByteLength));

    return new Guid(finalBytes);
  }

  /// <summary>
  /// Converts a DateTime into an 8-byte timestamp and writes it into the specified span.
  /// </summary>
  /// <param name="destination">The span where the timestamp will be written.</param>
  /// <param name="timestamp">The UTC timestamp to convert.</param>
  private static void WriteTimestampBytes(Span<byte> destination, DateTime timestamp) {
    long ticks = timestamp.ToUniversalTime().Ticks; // full 64-bit precision
    Span<byte> fullBytes = stackalloc byte[8];
    BinaryPrimitives.WriteInt64BigEndian(fullBytes, ticks);
    fullBytes.CopyTo(destination);
  }

  /// <summary>
  /// Reads an 8-byte timestamp from the given span and converts it to a DateTime.
  /// </summary>
  /// <param name="source">The span containing the timestamp bytes.</param>
  /// <returns>The corresponding UTC DateTime.</returns>
  private static DateTime ReadTimestampFromBytes(ReadOnlySpan<byte> source) {
    long ticks = BinaryPrimitives.ReadInt64BigEndian(source);
    return new DateTime(ticks, DateTimeKind.Utc);
  }

  /// <summary>
  /// Converts a DateTime to Unix time in milliseconds.
  /// </summary>
  /// <param name="timestamp">The UTC DateTime to convert.</param>
  /// <returns>Unix time in milliseconds.</returns>
  private static long ConvertToUnixTimeMilliseconds(DateTime timestamp) =>
      (long)(timestamp.ToUniversalTime() - UnixEpoch).TotalMilliseconds;

  /// <summary>
  /// Converts Unix time in milliseconds to a UTC DateTime.
  /// </summary>
  /// <param name="milliseconds">Unix time in milliseconds.</param>
  /// <returns>The corresponding UTC DateTime.</returns>
  private static DateTime ConvertFromUnixTimeMilliseconds(long milliseconds) =>
      UnixEpoch.AddMilliseconds(milliseconds);
}
