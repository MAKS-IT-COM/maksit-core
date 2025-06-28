using System.Collections.Generic;
using Xunit;
using MaksIT.Core.Security.JWT;

namespace MaksIT.Core.Tests.Security {
  public class JwtGeneratorTests {


    private JWTTokenGenerateRequest jWTTokenGenerateRequest = new JWTTokenGenerateRequest {
    Secret = "supersecretkey12345678901234567890",
    Issuer = "testIssuer",
    Audience = "testAudience",
    Expiration = 30, // 30 minutes
    Username = "testUser",
    Roles = new List<string> { "Admin", "User" },
  };

  [Fact]
    public void GenerateToken_ShouldReturnValidToken() {
      // Act
      var result = JwtGenerator.TryGenerateToken(jWTTokenGenerateRequest, out var tokenData, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.NotNull(tokenData);
      Assert.False(string.IsNullOrEmpty(tokenData?.Item1));
      Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateToken_ShouldReturnClaimsPrincipal_WhenTokenIsValid() {
      // Arrange
      JwtGenerator.TryGenerateToken(jWTTokenGenerateRequest, out var tokenData, out var generateErrorMessage);

      // Act
      var result = JwtGenerator.TryValidateToken(jWTTokenGenerateRequest.Secret, jWTTokenGenerateRequest.Issuer, jWTTokenGenerateRequest.Audience, tokenData?.Item1, out var jwtTokenClaims, out var validateErrorMessage);

      // Assert
      Assert.True(result);
      Assert.NotNull(jwtTokenClaims);
      Assert.Equal(jWTTokenGenerateRequest.Username, jwtTokenClaims?.Username);
      Assert.Contains(jwtTokenClaims?.Roles ?? new List<string>(), c => c == "Admin");
      Assert.Contains(jwtTokenClaims?.Roles ?? new List<string>(), c => c == "User");
      Assert.Null(validateErrorMessage);
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenTokenIsInvalid() {
      // Arrange
      var invalidToken = "invalidToken";

      // Act
      var result = JwtGenerator.TryValidateToken(jWTTokenGenerateRequest.Secret, jWTTokenGenerateRequest.Issuer, jWTTokenGenerateRequest.Audience, invalidToken, out var jwtTokenClaims, out var errorMessage);

      // Assert
      Assert.False(result);
      Assert.Null(jwtTokenClaims);
      Assert.NotNull(errorMessage);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyString() {
      // Act
      var refreshToken = JwtGenerator.GenerateRefreshToken();

      // Assert
      Assert.False(string.IsNullOrEmpty(refreshToken));
    }

    [Fact]
    public void GenerateSecret_ShouldReturnDifferentValuesOnSubsequentCalls() {
      // Act
      string secret1 = JwtGenerator.GenerateSecret();
      string secret2 = JwtGenerator.GenerateSecret();

      // Assert
      Assert.False(string.IsNullOrEmpty(secret1));
      Assert.False(string.IsNullOrEmpty(secret2));
      Assert.NotEqual(secret1, secret2); // Ensure the secrets are unique
    }
  }
}
