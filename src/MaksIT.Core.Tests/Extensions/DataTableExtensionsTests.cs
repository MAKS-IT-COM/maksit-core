using System.Data;
using MaksIT.Core.Extensions;


namespace MaksIT.Core.Tests.Extensions;

public class DataTableExtensionsTests {
  [Fact]
  public void DuplicatesCount_WithDuplicates_ReturnsCorrectCount() {
    // Arrange
    var dt1 = new DataTable();
    dt1.Columns.Add("Id");
    dt1.Columns.Add("Name");
    dt1.Rows.Add("1", "Alice");
    dt1.Rows.Add("2", "Bob");

    var dt2 = new DataTable();
    dt2.Columns.Add("Id");
    dt2.Columns.Add("Name");
    dt2.Rows.Add("1", "Alice");
    dt2.Rows.Add("3", "Charlie");

    // Act
    var duplicateCount = dt1.DuplicatesCount(dt2);

    // Assert
    Assert.Equal(1, duplicateCount);
  }

  [Fact]
  public void DuplicatesCount_WithNoDuplicates_ReturnsZero() {
    // Arrange
    var dt1 = new DataTable();
    dt1.Columns.Add("Id");
    dt1.Columns.Add("Name");
    dt1.Rows.Add("1", "Alice");

    var dt2 = new DataTable();
    dt2.Columns.Add("Id");
    dt2.Columns.Add("Name");
    dt2.Rows.Add("2", "Bob");

    // Act
    var duplicateCount = dt1.DuplicatesCount(dt2);

    // Assert
    Assert.Equal(0, duplicateCount);
  }

  [Fact]
  public void DistinctRecords_ReturnsDistinctRows() {
    // Arrange
    var dt = new DataTable();
    dt.Columns.Add("Id");
    dt.Columns.Add("Name");
    dt.Rows.Add("1", "Alice");
    dt.Rows.Add("1", "Alice");
    dt.Rows.Add("2", "Bob");

    // Act
    var distinctDt = dt.DistinctRecords(new[] { "Id", "Name" });

    // Assert
    Assert.Equal(2, distinctDt.Rows.Count);
  }
}
