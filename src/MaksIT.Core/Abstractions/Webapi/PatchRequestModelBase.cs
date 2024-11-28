using System.ComponentModel.DataAnnotations;
using System.Reflection;
using MaksIT.Core.Webapi.Models;


namespace MaksIT.Core.Abstractions.Webapi;

public abstract class PatchRequestModelBase : RequestModelBase, IValidatableObject {
  private bool HasNonNullPatchField => GetType()
    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
    .Where(prop => prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(PatchField<>))
    .Any(prop => prop.GetValue(this) != null);

  public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
    if (!HasNonNullPatchField) {
      yield return new ValidationResult("At least one patch field must be provided", new string[] { "PatchField" });
    }
  }
}
