using System.Reflection;
using MaksIT.Core.Webapi.Models;


namespace MaksIT.Core.Abstractions.Webapi;

public abstract class PatchRequestModelBase : RequestModelBase {
  public bool HasNonNullPatchField => GetType()
    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
    .Where(prop => prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(PatchField<>))
    .Any(prop => prop.GetValue(this) != null);
}
