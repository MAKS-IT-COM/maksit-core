namespace MaksIT.Core.Tests.Extensions;

using MaksIT.Core.Extensions;

public class FormatsExtensionsTests : IDisposable {
  private readonly string _testDirectory;
  private readonly List<string> _createdFiles = new();

  public FormatsExtensionsTests() {
    _testDirectory = Path.Combine(Path.GetTempPath(), $"MaksIT_Test_{Guid.NewGuid()}");
    Directory.CreateDirectory(_testDirectory);
  }

  public void Dispose() {
    // Cleanup
    try {
      if (Directory.Exists(_testDirectory)) {
        Directory.Delete(_testDirectory, true);
      }
      foreach (var file in _createdFiles) {
        if (File.Exists(file)) {
          File.Delete(file);
        }
      }
    }
    catch {
      // Ignore cleanup errors
    }
  }

  [Fact]
  public void TryCreateTarFromDirectory_ValidDirectory_ReturnsTrue() {
    // Arrange
    var sourceDir = Path.Combine(_testDirectory, "source");
    Directory.CreateDirectory(sourceDir);
    File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "Hello, World!");

    var outputTar = Path.Combine(_testDirectory, "output.tar");
    _createdFiles.Add(outputTar);

    // Act
    var result = FormatsExtensions.TryCreateTarFromDirectory(sourceDir, outputTar);

    // Assert
    Assert.True(result);
    Assert.True(File.Exists(outputTar));
    Assert.True(new FileInfo(outputTar).Length > 0);
  }

  [Fact]
  public void TryCreateTarFromDirectory_MultipleFiles_ReturnsTrue() {
    // Arrange
    var sourceDir = Path.Combine(_testDirectory, "multi_source");
    Directory.CreateDirectory(sourceDir);
    File.WriteAllText(Path.Combine(sourceDir, "file1.txt"), "Content 1");
    File.WriteAllText(Path.Combine(sourceDir, "file2.txt"), "Content 2");
    File.WriteAllText(Path.Combine(sourceDir, "file3.txt"), "Content 3");

    var outputTar = Path.Combine(_testDirectory, "multi_output.tar");
    _createdFiles.Add(outputTar);

    // Act
    var result = FormatsExtensions.TryCreateTarFromDirectory(sourceDir, outputTar);

    // Assert
    Assert.True(result);
    Assert.True(File.Exists(outputTar));
  }

  [Fact]
  public void TryCreateTarFromDirectory_NestedDirectories_ReturnsTrue() {
    // Arrange
    var sourceDir = Path.Combine(_testDirectory, "nested_source");
    var subDir = Path.Combine(sourceDir, "subdir");
    Directory.CreateDirectory(subDir);
    File.WriteAllText(Path.Combine(sourceDir, "root.txt"), "Root content");
    File.WriteAllText(Path.Combine(subDir, "nested.txt"), "Nested content");

    var outputTar = Path.Combine(_testDirectory, "nested_output.tar");
    _createdFiles.Add(outputTar);

    // Act
    var result = FormatsExtensions.TryCreateTarFromDirectory(sourceDir, outputTar);

    // Assert
    Assert.True(result);
    Assert.True(File.Exists(outputTar));
  }

  [Fact]
  public void TryCreateTarFromDirectory_EmptyDirectory_ReturnsFalse() {
    // Arrange
    var sourceDir = Path.Combine(_testDirectory, "empty_source");
    Directory.CreateDirectory(sourceDir);

    var outputTar = Path.Combine(_testDirectory, "empty_output.tar");

    // Act
    var result = FormatsExtensions.TryCreateTarFromDirectory(sourceDir, outputTar);

    // Assert
    Assert.False(result);
    Assert.False(File.Exists(outputTar));
  }

  [Fact]
  public void TryCreateTarFromDirectory_NonExistentDirectory_ReturnsFalse() {
    // Arrange
    var sourceDir = Path.Combine(_testDirectory, "non_existent");
    var outputTar = Path.Combine(_testDirectory, "non_existent_output.tar");

    // Act
    var result = FormatsExtensions.TryCreateTarFromDirectory(sourceDir, outputTar);

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void TryCreateTarFromDirectory_NullSourceDirectory_ReturnsFalse() {
    // Arrange
    var outputTar = Path.Combine(_testDirectory, "null_source_output.tar");

    // Act
    var result = FormatsExtensions.TryCreateTarFromDirectory(null!, outputTar);

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void TryCreateTarFromDirectory_EmptySourceDirectory_ReturnsFalse() {
    // Arrange
    var outputTar = Path.Combine(_testDirectory, "empty_path_output.tar");

    // Act
    var result = FormatsExtensions.TryCreateTarFromDirectory("", outputTar);

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void TryCreateTarFromDirectory_WhitespaceSourceDirectory_ReturnsFalse() {
    // Arrange
    var outputTar = Path.Combine(_testDirectory, "whitespace_output.tar");

    // Act
    var result = FormatsExtensions.TryCreateTarFromDirectory("   ", outputTar);

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void TryCreateTarFromDirectory_NullOutputPath_ReturnsFalse() {
    // Arrange
    var sourceDir = Path.Combine(_testDirectory, "valid_source");
    Directory.CreateDirectory(sourceDir);
    File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "Content");

    // Act
    var result = FormatsExtensions.TryCreateTarFromDirectory(sourceDir, null!);

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void TryCreateTarFromDirectory_EmptyOutputPath_ReturnsFalse() {
    // Arrange
    var sourceDir = Path.Combine(_testDirectory, "valid_source2");
    Directory.CreateDirectory(sourceDir);
    File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "Content");

    // Act
    var result = FormatsExtensions.TryCreateTarFromDirectory(sourceDir, "");

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void TryCreateTarFromDirectory_CreatesOutputDirectory_WhenNotExists() {
    // Arrange
    var sourceDir = Path.Combine(_testDirectory, "source_for_new_dir");
    Directory.CreateDirectory(sourceDir);
    File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "Content");

    var outputDir = Path.Combine(_testDirectory, "new_output_dir");
    var outputTar = Path.Combine(outputDir, "output.tar");
    _createdFiles.Add(outputTar);

    // Act
    var result = FormatsExtensions.TryCreateTarFromDirectory(sourceDir, outputTar);

    // Assert
    Assert.True(result);
    Assert.True(Directory.Exists(outputDir));
    Assert.True(File.Exists(outputTar));
  }
}
