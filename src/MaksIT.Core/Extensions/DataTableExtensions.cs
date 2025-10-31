using System.Data;


namespace MaksIT.Core.Extensions;
public static class DataTableExtensions {

  /// <summary>
  /// Counts duplicate records between two DataTables.
  /// </summary>
  /// <param name="dt1"></param>
  /// <param name="dt2"></param>
  /// <returns></returns>
  public static int DuplicatesCount(this DataTable dt1, DataTable dt2) {
    var duplicates = 0;
    foreach (DataRow dtRow1 in dt1.Rows) {
      var dt1Items = dtRow1.ItemArray.Select(item => item?.ToString() ?? string.Empty);
      var dt1Comp = string.Join("", dt1Items);
      foreach (DataRow dtRow2 in dt2.Rows) {
        var dt2Items = dtRow2.ItemArray.Select(item => item?.ToString() ?? string.Empty);
        var dt2Comp = string.Join("", dt2Items);

        if (dt1Comp == dt2Comp) {
          duplicates++;
        }
      }
    }

    return duplicates;
  }

  /// <summary>
  /// Returns distinct records based on specified columns.
  /// </summary>
  /// <param name="dt"></param>
  /// <param name="columns"></param>
  /// <returns></returns>
  public static DataTable DistinctRecords(this DataTable dt, string[] columns) {
    return dt.DefaultView.ToTable(true, columns);
  }
}
