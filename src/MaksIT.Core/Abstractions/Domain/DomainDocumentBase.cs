namespace MaksIT.Core.Abstractions.Domain;

public abstract class DomainDocumentBase<T> : DomainObjectBase {
  public T Id { get; set; }

  public DomainDocumentBase(T id) {
    Id = id;
  }

}
