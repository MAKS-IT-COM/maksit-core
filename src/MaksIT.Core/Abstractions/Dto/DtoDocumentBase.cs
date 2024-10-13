namespace MaksIT.Core.Abstractions.Dto;

public abstract class DtoDocumentBase<T> : DtoObjectBase {
  public required T Id { get; set; }
}
