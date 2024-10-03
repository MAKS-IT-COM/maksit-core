using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaksIT.Core.Security;

public static class Base32Encoder {
  private static readonly char[] Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();
  private const string PaddingChar = "=";

  public static string Encode(byte[] data) {
    if (data == null || data.Length == 0) {
      throw new ArgumentNullException(nameof(data));
    }

    var result = new StringBuilder();
    int buffer = data[0];
    int next = 1;
    int bitsLeft = 8;
    while (bitsLeft > 0 || next < data.Length) {
      if (bitsLeft < 5) {
        if (next < data.Length) {
          buffer <<= 8;
          buffer |= (data[next++] & 0xFF);
          bitsLeft += 8;
        }
        else {
          int pad = 5 - bitsLeft;
          buffer <<= pad;
          bitsLeft += pad;
        }
      }

      int index = (buffer >> (bitsLeft - 5)) & 0x1F;
      bitsLeft -= 5;
      result.Append(Base32Alphabet[index]);
    }

    // Padding for a complete block
    int padding = result.Length % 8;
    if (padding > 0) {
      for (int i = padding; i < 8; i++) {
        result.Append(PaddingChar);
      }
    }

    return result.ToString();
  }

  public static byte[] Decode(string base32) {
    if (string.IsNullOrEmpty(base32)) {
      throw new ArgumentNullException(nameof(base32));
    }

    base32 = base32.TrimEnd(PaddingChar.ToCharArray());
    int byteCount = base32.Length * 5 / 8;
    byte[] result = new byte[byteCount];

    int buffer = 0;
    int bitsLeft = 0;
    int index = 0;

    foreach (char c in base32) {
      int charValue = CharToValue(c);
      if (charValue < 0) {
        throw new FormatException("Invalid base32 character.");
      }

      buffer <<= 5;
      buffer |= charValue & 0x1F;
      bitsLeft += 5;

      if (bitsLeft >= 8) {
        result[index++] = (byte)(buffer >> (bitsLeft - 8));
        bitsLeft -= 8;
      }
    }

    return result;
  }

  private static int CharToValue(char c) {
    if (c >= 'A' && c <= 'Z') {
      return c - 'A';
    }

    if (c >= '2' && c <= '7') {
      return c - '2' + 26;
    }

    return -1; // Invalid character
  }
}

