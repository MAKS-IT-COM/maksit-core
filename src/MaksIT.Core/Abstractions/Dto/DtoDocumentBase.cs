namespace MaksIT.Core.Abstractions.Dto;

public abstract class DtoDocumentBase<T> : DtoObjectBase {
  public required T Id { get; set; }

  // Override Equals to compare based on Id
  public override bool Equals(object? obj) {
    if (obj is DtoDocumentBase<T> other) // Compare with the same base type
      return EqualityComparer<T>.Default.Equals(Id, other.Id); // Use EqualityComparer for generic type comparison

    return false;
  }

  // Override GetHashCode to use Id
  public override int GetHashCode() {
    if (Id is null)
      throw new InvalidOperationException("Id cannot be null when generating hash code.");

    return EqualityComparer<T>.Default.GetHashCode(Id); // Use EqualityComparer for hash code generation
  }
}
