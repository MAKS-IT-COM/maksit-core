using System;
using System.Buffers;
using System.Text;

namespace MaksIT.Core.Security.JWK;

/// <summary>
/// Provides RFC 4648-compliant Base64Url encoding and decoding utilities.
/// </summary>
public static class Base64UrlUtility
{
    /// <summary>
    /// Encodes a byte array to a Base64Url string (RFC 4648 §5).
    /// </summary>
    public static string Encode(byte[] data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        string base64 = Convert.ToBase64String(data);
        return base64.TrimEnd('=')
                     .Replace('+', '-')
                     .Replace('/', '_');
    }

    /// <summary>
    /// Encodes a UTF-8 string to a Base64Url string (RFC 4648 §5).
    /// </summary>
    public static string Encode(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        return Encode(Encoding.UTF8.GetBytes(value));
    }

    /// <summary>
    /// Decodes a Base64Url string to a byte array.
    /// </summary>
    public static byte[] Decode(string base64Url)
    {
        if (base64Url == null) throw new ArgumentNullException(nameof(base64Url));
        string padded = base64Url.Replace('-', '+').Replace('_', '/');
        switch (base64Url.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }

    /// <summary>
    /// Decodes a Base64Url string to a UTF-8 string.
    /// </summary>
    public static string DecodeToString(string base64Url)
    {
        return Encoding.UTF8.GetString(Decode(base64Url));
    }
}
