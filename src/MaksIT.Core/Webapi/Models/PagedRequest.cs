using System.Linq.Dynamic.Core;

using MaksIT.Core.Abstractions.Webapi;

public class PagedRequest : RequestModelBase {
  public int PageSize { get; set; } = 100;
  public int PageNumber { get; set; } = 1;
  public string? Filters { get; set; }

  public string? SortBy { get; set; }
  public bool IsAscending { get; set; } = true;

  public IQueryable<T> ApplyFilters<T>(IQueryable<T> query) {
    if (!string.IsNullOrWhiteSpace(Filters)) {
      query = query.Where(Filters); // Filters interpreted directly
    }

    if (!string.IsNullOrWhiteSpace(SortBy)) {
      var direction = IsAscending ? "ascending" : "descending";
      query = query.OrderBy($"{SortBy} {direction}");
    }

    return query.Skip((PageNumber - 1) * PageSize).Take(PageSize);
  }
}
