

namespace MaksIT.Core.Extensions.Tests;

  public class FromatsExtensionsTests {
    [Fact]
    public void TryCreateTarFromDirectory_InvalidSourceDirectory_ReturnsFalse() {
      // Arrange
      string invalidSourceDirectory = "NonExistentDirectory";
      string outputTarPath = "output.tar";

      // Act
      bool result = FormatsExtensions.TryCreateTarFromDirectory(invalidSourceDirectory, outputTarPath);

      // Assert
      Assert.False(result);
    }

    [Fact]
    public void TryCreateTarFromDirectory_InvalidOutputPath_ReturnsFalse() {
      // Arrange
      string sourceDirectory = Path.GetTempPath();
      string invalidOutputPath = "";

      // Act
      bool result = FormatsExtensions.TryCreateTarFromDirectory(sourceDirectory, invalidOutputPath);

      // Assert
      Assert.False(result);
    }

    [Fact]
    public void TryCreateTarFromDirectory_EmptySourceDirectory_ReturnsFalse() {
      // Arrange
      string sourceDirectory = Path.Combine(Path.GetTempPath(), "EmptyDirectory");
      Directory.CreateDirectory(sourceDirectory); // Ensure the directory exists but is empty
      string outputTarPath = Path.Combine(Path.GetTempPath(), "output.tar");

      // Act
      bool result = FormatsExtensions.TryCreateTarFromDirectory(sourceDirectory, outputTarPath);

      // Assert
      Assert.False(result);

      // Cleanup
      Directory.Delete(sourceDirectory);
    }

    [Fact]
    public void TryCreateTarFromDirectory_ValidInput_CreatesTarFile() {
      // Arrange
      string sourceDirectory = Path.Combine(Path.GetTempPath(), "TestDirectory");
      Directory.CreateDirectory(sourceDirectory);
      string testFilePath = Path.Combine(sourceDirectory, "test.txt");
      File.WriteAllText(testFilePath, "Test content");

      string outputTarPath = Path.Combine(Path.GetTempPath(), "output.tar");

      // Act
      bool result = FormatsExtensions.TryCreateTarFromDirectory(sourceDirectory, outputTarPath);

      // Assert
      Assert.True(result);
      Assert.True(File.Exists(outputTarPath));

      // Cleanup
      File.Delete(testFilePath);
      Directory.Delete(sourceDirectory);
      File.Delete(outputTarPath);
    }

    [Fact]
    public void TryCreateTarFromDirectory_CannotCreateOutputFile_ReturnsFalse() {
      // Arrange
      string sourceDirectory = Path.Combine(Path.GetTempPath(), "TestDirectory");
      Directory.CreateDirectory(sourceDirectory);
      string testFilePath = Path.Combine(sourceDirectory, "test.txt");
      File.WriteAllText(testFilePath, "Test content");

      string outputTarPath = Path.Combine(Path.GetTempPath(), "output.tar");

      // Lock the file to simulate inability to create it
      using (FileStream lockedFile = File.Create(outputTarPath)) {
        // Act
        bool result = FormatsExtensions.TryCreateTarFromDirectory(sourceDirectory, outputTarPath);

        // Assert
        Assert.False(result);
      }

      // Cleanup
      File.Delete(testFilePath);
      Directory.Delete(sourceDirectory);
      if (File.Exists(outputTarPath)) {
        File.Delete(outputTarPath);
      }
    }
  }