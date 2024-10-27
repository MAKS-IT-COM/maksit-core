
namespace MaksIT.Core.Webapi.Models;

public class PatchField<T> {
  public PatchOperation Operation { get; set; }
  public T? Value { get; set; }

  public void Deconstruct(out PatchOperation operation, out T? value) {
    operation = Operation;
    value = Value;
  }
}
