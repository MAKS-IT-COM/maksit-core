namespace MaksIT.Core.Sagas;

/// <summary>
/// Shared context to pass values between steps without tight coupling.
/// </summary>
public sealed class LocalSagaContext {
  private readonly Dictionary<string, object?> _bag = new(StringComparer.Ordinal);

  public T? Get<T>(string key) {
    return _bag.TryGetValue(key, out var v) && v is T t ? t : default;
  }

  public LocalSagaContext Set<T>(string key, T value) {
    _bag[key] = value;
    return this;
  }

  public bool Contains(string key) => _bag.ContainsKey(key);
}