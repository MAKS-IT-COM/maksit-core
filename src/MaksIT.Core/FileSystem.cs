using System.Runtime.InteropServices;
using MaksIT.Core.Extensions;

namespace MaksIT.Core;

/// <summary>
/// Main <c>FileSystem</c> class.
/// Provides basic helper methods to work with the file system.
/// </summary>
public static class FileSystem {
  private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

  /// <summary>
  /// Copies the file or folder's content to the specified folder.
  /// </summary>
  /// <param name="sourcePath">File or directory path.</param>
  /// <param name="destDirPath">Destination directory.</param>
  /// <param name="overwrite">Whether to overwrite existing files.</param>
  /// <param name="errorMessage">The error message if the operation fails.</param>
  /// <returns>True if the copy operation was successful; otherwise, false.</returns>
  public static bool TryCopyToFolder(string sourcePath, string destDirPath, bool overwrite, out string? errorMessage) {
    try {
      if (!Directory.Exists(destDirPath)) {
        Directory.CreateDirectory(destDirPath);
      }

      FileAttributes attr = File.GetAttributes(sourcePath);

      if (attr.HasFlag(FileAttributes.Directory)) {
        foreach (var filePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)) {
          var destFilePath = Path.Combine(destDirPath, filePath.Substring(sourcePath.Length).TrimStart(Path.DirectorySeparatorChar));
          var destDirectoryPath = Path.GetDirectoryName(destFilePath);

          if (destDirectoryPath != null && !Directory.Exists(destDirectoryPath)) {
            Directory.CreateDirectory(destDirectoryPath);
          }

          File.Copy(filePath, destFilePath, overwrite);
        }
      }
      else {
        // It's a file
        File.Copy(sourcePath, Path.Combine(destDirPath, Path.GetFileName(sourcePath)), overwrite);
      }

      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      errorMessage = ex.Message;
      return false;
    }
  }

  /// <summary>
  /// Deletes a file or directory at the specified path.
  /// </summary>
  /// <param name="itemPath">File or directory path.</param>
  /// <param name="errorMessage">The error message if the operation fails.</param>
  /// <returns>True if the delete operation was successful; otherwise, false.</returns>
  public static bool TryDeleteFileOrDirectory(string itemPath, out string? errorMessage) {
    try {
      if (File.Exists(itemPath)) {
        File.Delete(itemPath);
      }
      else if (Directory.Exists(itemPath)) {
        Directory.Delete(itemPath, true);
      }

      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      errorMessage = ex.Message;
      return false;
    }
  }

  /// <summary>
  /// Resolves a path with wildcards and returns all possible variants found.
  /// </summary>
  /// <param name="wildcardedPath">Example - @"?:\Users\*\AppData\Roa*\"</param>
  /// <returns>Returns all possible, but existing path variants found.</returns>
  public static List<string> ResolveWildcardedPath(string wildcardedPath) {
    var response = new List<string>();

    wildcardedPath = wildcardedPath.TrimEnd(Path.DirectorySeparatorChar);

    if (!wildcardedPath.Contains('*') && !wildcardedPath.Contains('?')) {
      response.Add(wildcardedPath);
      return response;
    }

    var pathsCollection = new List<string> { "" };

    foreach (string item in wildcardedPath.Split(Path.DirectorySeparatorChar)) {
      if (item == "?:") {
        pathsCollection = DriveInfo.GetDrives()
            .Where(drive => drive.Name.Like(item + Path.DirectorySeparatorChar))
            .Select(drive => drive.Name)
            .ToList();
      }
      else if (item.Contains('*') || item.Contains('?')) {
        var temp = new List<string>();

        foreach (var path in pathsCollection) {
          if (Directory.Exists(path)) {
            try {
              temp.AddRange(Directory.GetFiles(path).Where(file => Path.GetFileName(file).Like(item)).Select(file => file + Path.DirectorySeparatorChar));
              temp.AddRange(Directory.GetDirectories(path).Where(dir => Path.GetFileName(dir).Like(item)).Select(dir => dir + Path.DirectorySeparatorChar));
            }
            catch {
              // Handle exceptions if necessary
            }
          }
        }

        pathsCollection = temp;
      }
      else {
        if (pathsCollection.Count == 0) {
          pathsCollection.Add(item + Path.DirectorySeparatorChar);
        }
        else {
          for (var i = 0; i < pathsCollection.Count; i++) {
            pathsCollection[i] += item + Path.DirectorySeparatorChar;
          }
        }
      }
    }

    pathsCollection = pathsCollection.Select(s => s.Trim(Path.DirectorySeparatorChar)).ToList();

    var tempWildcardedPath = wildcardedPath.Split(Path.DirectorySeparatorChar);

    response = pathsCollection
        .Where(path => {
          var tempPath = path.Split(Path.DirectorySeparatorChar);
          if (tempWildcardedPath.Length != tempPath.Length) return false;

          for (int i = 0; i < tempWildcardedPath.Length; i++) {
            if (!tempWildcardedPath[i].Contains('*') && !tempWildcardedPath[i].Contains('?') && tempWildcardedPath[i] != tempPath[i]) {
              return false;
            }
          }

          return Directory.Exists(path) || File.Exists(path);
        })
        .ToList();

    return response;
  }

  /// <summary>
  /// Tests a file name for duplicates, and if it is a duplicate, assigns a new name.
  /// </summary>
  /// <param name="fullPath">File path to test for duplicates.</param>
  /// <returns>Returns the updated file name.</returns>
  public static string DuplicateFileNameCheck(string fullPath) {
    var fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
    var extension = Path.GetExtension(fullPath);
    var path = Path.GetDirectoryName(fullPath);
    var newFullPath = fullPath;

    if (path == null) {
      throw new ArgumentException("Invalid file path", nameof(fullPath));
    }

    var count = 1;
    while (File.Exists(newFullPath)) {
      var tempFileName = $"{fileNameOnly}({count++})";
      newFullPath = Path.Combine(path, tempFileName + extension);
    }

    return newFullPath;
  }
}
