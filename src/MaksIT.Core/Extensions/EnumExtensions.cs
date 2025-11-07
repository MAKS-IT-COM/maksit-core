using System.Reflection;
using System.ComponentModel.DataAnnotations;


namespace MaksIT.Core.Extensions;

public static class EnumExtensions {
  public static string GetDisplayName<TEnum>(this TEnum value) where TEnum : Enum {
    var type = typeof(TEnum);
    var memInfo = type.GetMember(value.ToString());
    var attributes = memInfo[0].GetCustomAttribute<DisplayAttribute>();
    return attributes?.Name ?? value.ToString();
  }
}
