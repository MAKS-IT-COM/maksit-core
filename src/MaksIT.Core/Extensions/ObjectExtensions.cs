using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace MaksIT.Core.Extensions;

public static class ObjectExtensions {

  /// <summary>
  /// Converts object to json string
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="obj"></param>
  /// <returns></returns>
  public static string ToJson<T>(this T? obj) => obj.ToJson(null);

  /// <summary>
  /// Converts object to json string
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="obj"></param>
  /// <param name="converters"></param>
  /// <returns></returns>
  public static string ToJson<T>(this T? obj, List<JsonConverter>? converters) {
    if (obj == null)
      return "{}";

    var options = new JsonSerializerOptions {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    converters?.ForEach(x => options.Converters.Add(x));

    return JsonSerializer.Serialize(obj, options);
  }

  /// <summary>
  /// Creates a deep clone of the object, preserving reference identity and supporting cycles.
  /// </summary>
  public static T DeepClone<T>(this T source) {
    return (T)DeepCloneInternal(source, new Dictionary<object, object>(ReferenceEqualityComparer.Instance));
  }

  /// <summary>
  /// Deeply compares two objects for structural equality (fields, including private ones).
  /// </summary>
  public static bool DeepEqual<T>(this T a, T b) {
    return DeepEqualInternal(a, b, new HashSet<(object, object)>(ReferencePairComparer.Instance));
  }

  /// <summary>
  /// Copies all fields from the snapshot into the current target object (useful with tracked entities).
  /// </summary>
  public static void RevertFrom<T>(this T target, T snapshot) {
    if (ReferenceEquals(target, snapshot) || target == null || snapshot == null) return;
    var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
    CopyAllFields(snapshot!, target!, snapshot!.GetType(), visited);
  }

  #region Internal Cloner

  private static object DeepCloneInternal(object source, Dictionary<object, object> visited) {
    if (source == null) return null!;

    var type = source.GetType();

    // Fast-path for immutable/primitive-ish types
    if (IsImmutable(type)) return source;

    // Already cloned?
    if (!type.IsValueType && visited.TryGetValue(source, out var existing))
      return existing;

    // Arrays
    if (type.IsArray)
      return CloneArray((Array)source, visited);

    // Value types (structs): shallow copy via boxing + clone ref-type fields
    if (type.IsValueType)
      return CloneStruct(source, type, visited);

    // Reference type: allocate uninitialized object, then copy fields
    var clone = FormatterServices.GetUninitializedObject(type);
    visited[source] = clone;
    CopyAllFields(source, clone, type, visited);
    return clone;
  }

  private static bool IsImmutable(Type t) {
    if (t.IsPrimitive || t.IsEnum) return true;

    // Common immutable BCL types
    if (t == typeof(string) ||
        t == typeof(decimal) ||
        t == typeof(DateTime) ||
        t == typeof(DateTimeOffset) ||
        t == typeof(TimeSpan) ||
        t == typeof(Guid) ||
        t == typeof(Uri))
      return true;

    // Nullable<T> of immutable underlying
    if (Nullable.GetUnderlyingType(t) is Type nt)
      return IsImmutable(nt);

    return false;
  }

  private static Array CloneArray(Array source, Dictionary<object, object> visited) {
    var elemType = source.GetType().GetElementType()!;
    var rank = source.Rank;

    var lengths = new int[rank];
    var lowers = new int[rank];
    for (int d = 0; d < rank; d++) {
      lengths[d] = source.GetLength(d);
      lowers[d] = source.GetLowerBound(d);
    }

    var clone = Array.CreateInstance(elemType, lengths, lowers);
    visited[source] = clone;

    var indices = new int[rank];
    CopyArrayRecursive(source, clone, 0, indices, lowers, lengths, visited);
    return clone;
  }

  private static void CopyArrayRecursive(
    Array source,
    Array target,
    int dim,
    int[] indices,
    int[] lowers,
    int[] lengths,
    Dictionary<object, object> visited) {
    if (dim == source.Rank) {
      var value = source.GetValue(indices);
      var cloned = DeepCloneInternal(value!, visited);
      target.SetValue(cloned, indices);
      return;
    }

    int start = lowers[dim];
    int end = lowers[dim] + lengths[dim];
    for (int i = start; i < end; i++) {
      indices[dim] = i;
      CopyArrayRecursive(source, target, dim + 1, indices, lowers, lengths, visited);
    }
  }

  private static object CloneStruct(object source, Type type, Dictionary<object, object> visited) {
    // Boxed copy is already a shallow copy of the struct
    object boxed = source;
    CopyAllFields(boxed, boxed, type, visited, skipVisitedRegistration: true);
    return boxed;
  }

  private static void CopyAllFields(object source, object target, Type type, Dictionary<object, object> visited, bool skipVisitedRegistration = false) {
    for (Type t = type; t != null; t = t.BaseType!) {
      var fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
      foreach (var f in fields) {
        var value = f.GetValue(source);
        var cloned = DeepCloneInternal(value!, visited);
        f.SetValue(target, cloned);
      }
    }
  }

  #endregion

  #region Internal Deep-Equal

  private static bool DeepEqualInternal(object a, object b, HashSet<(object, object)> visited) {
    if (ReferenceEquals(a, b))
      return true;
    if (a == null || b == null)
      return false;

    var type = a.GetType();
    if (type != b.GetType())
      return false;

    // Fast path for immutables
    if (IsImmutable(type))
      return a.Equals(b);

    // Prevent infinite recursion on cycles
    var pair = (a, b);
    if (visited.Contains(pair))
      return true;
    visited.Add(pair);

    // Arrays
    if (type.IsArray)
      return ArraysEqual((Array)a, (Array)b, visited);

    // Value or reference types: compare fields recursively
    return FieldsEqual(a, b, type, visited);
  }

  private static bool ArraysEqual(Array a, Array b, HashSet<(object, object)> visited) {
    if (a.Rank != b.Rank) return false;
    for (int d = 0; d < a.Rank; d++) {
      if (a.GetLength(d) != b.GetLength(d) || a.GetLowerBound(d) != b.GetLowerBound(d))
        return false;
    }

    var indices = new int[a.Rank];
    return CompareArrayRecursive(a, b, 0, indices, visited);
  }

  private static bool CompareArrayRecursive(Array a, Array b, int dim, int[] indices, HashSet<(object, object)> visited) {
    if (dim == a.Rank) {
      var va = a.GetValue(indices);
      var vb = b.GetValue(indices);
      return DeepEqualInternal(va!, vb!, visited);
    }

    int start = a.GetLowerBound(dim);
    int end = start + a.GetLength(dim);
    for (int i = start; i < end; i++) {
      indices[dim] = i;
      if (!CompareArrayRecursive(a, b, dim + 1, indices, visited))
        return false;
    }
    return true;
  }

  private static bool FieldsEqual(object a, object b, Type type, HashSet<(object, object)> visited) {
    for (Type t = type; t != null; t = t.BaseType!) {
      var fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
      foreach (var f in fields) {
        var va = f.GetValue(a);
        var vb = f.GetValue(b);
        if (!DeepEqualInternal(va!, vb!, visited))
          return false;
      }
    }
    return true;
  }

  #endregion

  #region Helpers

  private sealed class ReferenceEqualityComparer : IEqualityComparer<object> {
    public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
    public new bool Equals(object x, object y) => ReferenceEquals(x, y);
    public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
  }

  private sealed class ReferencePairComparer : IEqualityComparer<(object, object)> {
    public static readonly ReferencePairComparer Instance = new ReferencePairComparer();
    public bool Equals((object, object) x, (object, object) y)
      => ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2);
    public int GetHashCode((object, object) obj) {
      unchecked {
        return (RuntimeHelpers.GetHashCode(obj.Item1) * 397) ^ RuntimeHelpers.GetHashCode(obj.Item2);
      }
    }
  }

  #endregion
}