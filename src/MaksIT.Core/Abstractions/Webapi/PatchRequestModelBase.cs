using System.Reflection;
using System.ComponentModel.DataAnnotations;

using MaksIT.Core.Webapi.Models;
using System.Diagnostics.CodeAnalysis;


namespace MaksIT.Core.Abstractions.Webapi;

public abstract class PatchRequestModelBase : RequestModelBase {

  public Dictionary<string, PatchOperation>? Operations { get; set; }

  private bool HasNonNullPatchField => GetType()
    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
    .Where(prop => prop.Name != nameof(Operations))
    .Any(prop => prop.GetValue(this) != null);

  /// <summary>
  /// Attempts to retrieve the patch operation associated with the specified property name (case insensitive).
  /// </summary>
  /// <param name="propertyName">The name of the property for which to retrieve the patch operation.</param>
  /// <param name="operation">When this method returns, contains the patch operation associated with the specified property name, if the key is found; otherwise, null.</param>
  /// <returns>true if the patch operation is found; otherwise, false.</returns>
  public bool TryGetOperation(string propertyName, [NotNullWhen(true)] out PatchOperation? operation) {
    if (Operations == null) {
      operation = null;
      return false;
    }

    var entry = Operations.FirstOrDefault(op => op.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
    if (!entry.Equals(default(KeyValuePair<string, PatchOperation>))) {
      operation = entry.Value;
      return true;
    }
    else {
      operation = null;
      return false;
    }
  }

  public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
    if (!HasNonNullPatchField) {
      yield return new ValidationResult("At least one patch field must be provided", ["PatchField"]);
    }

    if (Operations != null) {
      foreach (var operation in Operations) {
        if (!Enum.IsDefined(typeof(PatchOperation), operation.Value)) {
          yield return new ValidationResult($"Invalid patch operation '{operation.Value}' for property '{operation.Key}'", [operation.Key]);
        }
      }
    }
  }
}
