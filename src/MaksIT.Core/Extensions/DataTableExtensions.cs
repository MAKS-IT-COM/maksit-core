using System.Data;


namespace MaksIT.Core.Extensions;
public static class DataTableExtensions {

  public static int DuplicatesCount(this DataTable dt1, DataTable dt2) {
    if (dt1 == null)
      throw new ArgumentNullException(nameof(dt1));
    if (dt2 == null)
      throw new ArgumentNullException(nameof(dt2));

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
    if (dt == null)
      throw new ArgumentNullException(nameof(dt));
    if (columns == null)
      throw new ArgumentNullException(nameof(columns));

    return dt.DefaultView.ToTable(true, columns);
  }
}
