using System.Reflection;
using System.ComponentModel.DataAnnotations;

using MaksIT.Core.Webapi.Models;


namespace MaksIT.Core.Abstractions.Webapi;

public abstract class PatchRequestModelBase : RequestModelBase {

  public Dictionary<string, PatchOperation> Operations = new Dictionary<string, PatchOperation>();

  private bool HasNonNullPatchField => GetType()
    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
    .Where(prop => prop.Name != nameof(Operations))
    .Any(prop => prop.GetValue(this) != null);

  public PatchOperation GetOperation(string propertyName) {
    return Operations[propertyName];
  }

  public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
    if (!HasNonNullPatchField) {
      yield return new ValidationResult("At least one patch field must be provided", new string[] { "PatchField" });
    }
  }
}
