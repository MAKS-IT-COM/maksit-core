using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaksIT.Core.Abstractions.Webapi;
public abstract class RequestModelBase : IValidatableObject {
  public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
    return Enumerable.Empty<ValidationResult>();
  }
}
