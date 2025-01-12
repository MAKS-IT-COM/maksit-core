namespace MaksIT.Core.Abstractions.Query;

public abstract class QueryResultBase<T> {
  public required T Id { get; set; }
}