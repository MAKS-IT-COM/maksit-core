using MaksIT.Core.Logging;

namespace MaksIT.Core.Tests.Logging;

public class LoggerPrefixTests {
  [Fact]
  public void WithValue_ShouldCreateCorrectCategoryString() {
    // Arrange & Act
    var folderCategory = LoggerPrefix.Folder.WithValue("Audit");
    var categoryCategory = LoggerPrefix.Category.WithValue("Orders");
    var tagCategory = LoggerPrefix.Tag.WithValue("Critical");

    // Assert
    Assert.Equal("Folder:Audit", folderCategory);
    Assert.Equal("Category:Orders", categoryCategory);
    Assert.Equal("Tag:Critical", tagCategory);
  }

  [Fact]
  public void WithValue_ShouldHandleSpacesInValue() {
    // Arrange & Act
    var result = LoggerPrefix.Folder.WithValue("My Custom Folder");

    // Assert
    Assert.Equal("Folder:My Custom Folder", result);
  }

  [Fact]
  public void WithValue_ShouldHandleEmptyValue() {
    // Arrange & Act
    var result = LoggerPrefix.Folder.WithValue("");

    // Assert
    Assert.Equal("Folder:", result);
  }

  [Fact]
  public void Parse_ShouldExtractFolderPrefix() {
    // Arrange
    var categoryName = "Folder:Audit";

    // Act
    var (prefix, value) = LoggerPrefix.Parse(categoryName);

    // Assert
    Assert.Equal(LoggerPrefix.Folder, prefix);
    Assert.Equal("Audit", value);
  }

  [Fact]
  public void Parse_ShouldExtractCategoryPrefix() {
    // Arrange
    var categoryName = "Category:Orders";

    // Act
    var (prefix, value) = LoggerPrefix.Parse(categoryName);

    // Assert
    Assert.Equal(LoggerPrefix.Category, prefix);
    Assert.Equal("Orders", value);
  }

  [Fact]
  public void Parse_ShouldExtractTagPrefix() {
    // Arrange
    var categoryName = "Tag:Critical";

    // Act
    var (prefix, value) = LoggerPrefix.Parse(categoryName);

    // Assert
    Assert.Equal(LoggerPrefix.Tag, prefix);
    Assert.Equal("Critical", value);
  }

  [Fact]
  public void Parse_ShouldHandleValueWithSpaces() {
    // Arrange
    var categoryName = "Folder:My Custom Folder";

    // Act
    var (prefix, value) = LoggerPrefix.Parse(categoryName);

    // Assert
    Assert.Equal(LoggerPrefix.Folder, prefix);
    Assert.Equal("My Custom Folder", value);
  }

  [Fact]
  public void Parse_ShouldReturnNullForUnrecognizedPrefix() {
    // Arrange
    var categoryName = "MyApp.Services.OrderService";

    // Act
    var (prefix, value) = LoggerPrefix.Parse(categoryName);

    // Assert
    Assert.Null(prefix);
    Assert.Null(value);
  }

  [Fact]
  public void Parse_ShouldReturnNullForEmptyString() {
    // Arrange
    var categoryName = "";

    // Act
    var (prefix, value) = LoggerPrefix.Parse(categoryName);

    // Assert
    Assert.Null(prefix);
    Assert.Null(value);
  }

  [Fact]
  public void Parse_ShouldHandleEmptyValueAfterPrefix() {
    // Arrange
    var categoryName = "Folder:";

    // Act
    var (prefix, value) = LoggerPrefix.Parse(categoryName);

    // Assert
    Assert.Equal(LoggerPrefix.Folder, prefix);
    Assert.Equal("", value);
  }

  [Fact]
  public void Parse_ShouldBeCaseSensitive() {
    // Arrange
    var categoryName = "folder:Audit"; // lowercase 'f'

    // Act
    var (prefix, value) = LoggerPrefix.Parse(categoryName);

    // Assert
    Assert.Null(prefix);
    Assert.Null(value);
  }

  [Fact]
  public void GetAll_ShouldReturnAllPrefixes() {
    // Arrange & Act
    var allPrefixes = MaksIT.Core.Abstractions.Enumeration.GetAll<LoggerPrefix>().ToList();

    // Assert
    Assert.Equal(3, allPrefixes.Count);
    Assert.Contains(LoggerPrefix.Folder, allPrefixes);
    Assert.Contains(LoggerPrefix.Category, allPrefixes);
    Assert.Contains(LoggerPrefix.Tag, allPrefixes);
  }

  [Fact]
  public void ToString_ShouldReturnPrefixName() {
    // Arrange & Act & Assert
    Assert.Equal("Folder:", LoggerPrefix.Folder.ToString());
    Assert.Equal("Category:", LoggerPrefix.Category.ToString());
    Assert.Equal("Tag:", LoggerPrefix.Tag.ToString());
  }
}
