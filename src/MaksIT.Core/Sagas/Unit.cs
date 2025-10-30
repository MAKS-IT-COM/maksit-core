using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaksIT.Core.Sagas;
/// <summary>
/// A simple unit type for steps that do not return a value.
/// </summary>
public readonly struct Unit {
  public static readonly Unit Value = new Unit();
}
