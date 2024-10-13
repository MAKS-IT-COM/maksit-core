using MaksIT.Core.Abstractions.Webapi;

namespace MaksIT.Core.Webapi.Models;

public class PagedResponse<T> : ResponseModelBase {
  public IEnumerable<T> Items { get; set; }
  public int PageNumber { get; set; }
  public int PageSize { get; set; }
  public int TotalCount { get; set; }
  public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
  public bool HasPreviousPage => PageNumber > 1;
  public bool HasNextPage => PageNumber < TotalPages;

  public PagedResponse(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize) {
    Items = items;
    TotalCount = totalCount;
    PageNumber = pageNumber;
    PageSize = pageSize;
  }
}