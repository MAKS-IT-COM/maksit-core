using MaksIT.Core.Comb;


namespace MaksIT.Core.Tests.Comb;

public class CombGuidGeneratorTests {
  [Theory]
  [InlineData(CombGuidType.SqlServer)]
  [InlineData(CombGuidType.PostgreSql)]
  public void CreateCombGuid_WithBaseGuidAndTimestamp_EmbedsTimestampCorrectly(CombGuidType type) {
    // Arrange
    var baseGuid = Guid.NewGuid();
    var timestamp = DateTime.UtcNow;

    // Act
    var combGuid = CombGuidGenerator.CreateCombGuid(baseGuid, timestamp, type);
    var extractedTimestamp = CombGuidGenerator.ExtractTimestamp(combGuid, type);

    // Assert
    Assert.Equal(timestamp, extractedTimestamp);
  }

  [Theory]
  [InlineData(CombGuidType.SqlServer)]
  [InlineData(CombGuidType.PostgreSql)]
  public void CreateCombGuid_WithTimestampOnly_GeneratesValidCombGuid(CombGuidType type) {
    // Arrange
    var timestamp = DateTime.UtcNow;

    // Act
    var combGuid = CombGuidGenerator.CreateCombGuid(timestamp, type);
    var extractedTimestamp = CombGuidGenerator.ExtractTimestamp(combGuid, type);

    // Assert
    Assert.Equal(timestamp, extractedTimestamp);
  }

  [Theory]
  [InlineData(CombGuidType.SqlServer)]
  [InlineData(CombGuidType.PostgreSql)]
  public void CreateCombGuid_WithBaseGuidOnly_UsesCurrentUtcTimestamp(CombGuidType type) {
    // Arrange
    var baseGuid = Guid.NewGuid();
    var beforeCreation = DateTime.UtcNow;

    // Act
    var combGuid = CombGuidGenerator.CreateCombGuid(baseGuid, type);
    var extractedTimestamp = CombGuidGenerator.ExtractTimestamp(combGuid, type);

    // Assert
    Assert.True(extractedTimestamp >= beforeCreation);
    Assert.True(extractedTimestamp <= DateTime.UtcNow);
  }

  [Theory]
  [InlineData(CombGuidType.SqlServer)]
  [InlineData(CombGuidType.PostgreSql)]
  public void ExtractTimestamp_ReturnsCorrectTimestamp(CombGuidType type) {
    // Arrange
    var baseGuid = Guid.NewGuid();
    var timestamp = DateTime.UtcNow;
    var combGuid = CombGuidGenerator.CreateCombGuid(baseGuid, timestamp, type);

    // Act
    var extractedTimestamp = CombGuidGenerator.ExtractTimestamp(combGuid, type);

    // Assert
    Assert.Equal(timestamp, extractedTimestamp);
  }
}