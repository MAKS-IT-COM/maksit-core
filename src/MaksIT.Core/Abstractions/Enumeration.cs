using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MaksIT.Core.Abstractions {
  public abstract class Enumeration : IComparable {
    public string Name { get; }

    public int Id { get; }

    protected Enumeration(int id, string name) => (Id, Name) = (id, name);

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration =>
        typeof(T).GetFields(BindingFlags.Public |
                            BindingFlags.Static |
                            BindingFlags.DeclaredOnly)
                 .Select(f => f.GetValue(null))
                 .Cast<T>();

    public override bool Equals(object? obj) =>
        obj is Enumeration otherValue &&
        GetType() == obj.GetType() &&
        Id == otherValue.Id;

    public override int GetHashCode() => Id.GetHashCode();

    public static int AbsoluteDifference(Enumeration firstValue, Enumeration secondValue) =>
        Math.Abs(firstValue.Id - secondValue.Id);

    public static T FromValue<T>(int value) where T : Enumeration =>
        Parse<T, int>(value, nameof(value), item => item.Id == value);

    public static T FromDisplayName<T>(string displayName) where T : Enumeration =>
        Parse<T, string>(displayName, nameof(displayName), item => item.Name == displayName);

    private static T Parse<T, TK>(TK value, string description, Func<T, bool> predicate) where T : Enumeration =>
        GetAll<T>().FirstOrDefault(predicate) ??
        throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");

    public int CompareTo(object? other) {
      if (other is Enumeration otherEnumeration)
        return Id.CompareTo(otherEnumeration.Id);
      else
        throw new ArgumentException($"Object is not of type {nameof(Enumeration)}");
    }
  }
}
