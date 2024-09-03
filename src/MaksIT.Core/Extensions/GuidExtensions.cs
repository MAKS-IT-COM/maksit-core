using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaksIT.Core.Extensions {
  public static class GuidExtensions {
    public static Guid? ToNullable(this Guid id) {
      // Return null if the Guid is the default value (Guid.Empty)
      return id == default ? null : id;
    }
  }

}
