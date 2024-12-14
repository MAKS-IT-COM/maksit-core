﻿using System.Reflection;
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

  public bool TryGetOperation(string propertyName, [NotNullWhen(true)] out PatchOperation? operation) {
    if (Operations == null) {
      operation = null;
      return false;
    }

    if (Operations.TryGetValue(propertyName, out var tempOperation)) {
      operation = tempOperation;
      return true;
    }
    else {
      operation = null;
      return false;
    }
  }

  public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
    if (!HasNonNullPatchField) {
      yield return new ValidationResult("At least one patch field must be provided", new string[] { "PatchField" });
    }

    if (Operations != null) {
      foreach (var operation in Operations) {
        if (!Enum.IsDefined(typeof(PatchOperation), operation.Value)) {
          yield return new ValidationResult($"Invalid patch operation '{operation.Value}' for property '{operation.Key}'", new string[] { operation.Key });
        }
      }
    }
  }
}
