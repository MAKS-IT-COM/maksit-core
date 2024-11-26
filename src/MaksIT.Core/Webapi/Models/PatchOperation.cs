namespace MaksIT.Core.Webapi.Models;

public enum PatchOperation {

  /// <summary>
  /// When you need to replace some field, or relpace item in collection
  /// </summary>
  Replace,

  /// <summary>
  /// When you need to set some field, or add item to collection
  /// </summary>
  Add,

  /// <summary>
  /// When you need to set some field to null, or remove item from collection
  /// </summary>
  Remove,

  /// <summary>
  /// When you need to clear collection
  /// </summary>
  Clear
}
