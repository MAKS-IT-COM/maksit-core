using System.Formats.Tar;


namespace MaksIT.Core.Extensions;
public static class FromatsExtensions {
  public static bool TryCreateTarFromDirectory(string sourceDirectory, string outputTarPath) {
    // Validate source directory
    if (string.IsNullOrWhiteSpace(sourceDirectory) || !Directory.Exists(sourceDirectory)) {
      return false;
    }

    // Validate output path
    if (string.IsNullOrWhiteSpace(outputTarPath)) {
      return false;
    }

    // Ensure the parent directory of the output file exists
    string? outputDirectory = Path.GetDirectoryName(outputTarPath);
    if (outputDirectory is not null && !Directory.Exists(outputDirectory)) {
      try {
        Directory.CreateDirectory(outputDirectory);
      }
      catch {
        return false; // Return false if directory creation fails
      }
    }

    // Validate if the source directory contains files
    var files = Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories);
    if (!files.Any()) {
      return false;
    }

    // Create the TAR file
    if (!CanCreateFile(outputTarPath)) {
      return false; // Ensure the output file can be created
    }

    using FileStream fs = File.Create(outputTarPath);
    using TarWriter writer = new TarWriter(fs, TarEntryFormat.Pax, leaveOpen: false);

    foreach (string filePath in files) {
      string relativePath = Path.GetRelativePath(sourceDirectory, filePath).Replace(Path.DirectorySeparatorChar, '/');
      writer.WriteEntry(filePath, relativePath);
    }

    return true;
  }

  private static bool CanCreateFile(string filePath) {
    try {
      using (FileStream fs = File.Create(filePath, 1, FileOptions.DeleteOnClose)) { }
      return true;
    }
    catch {
      return false; // Return false if the file cannot be created
    }
  }
}
