using MaksIT.Core.Webapi.Models;


namespace MaksIT.Core.Tests.Webapi.Models;

public class PagedResponseTests {
  [Fact]
  public void TotalPages_ShouldCalculateCorrectly() {
    // Arrange
    var items = new List<string> { "Item1", "Item2" };
    var response = new PagedResponse<string>(items, 10, 1, 2);

    // Act
    var totalPages = response.TotalPages;

    // Assert
    Assert.Equal(5, totalPages);
  }

  [Fact]
  public void HasPreviousPage_ShouldReturnTrue_WhenPageNumberGreaterThan1() {
    // Arrange
    var items = new List<string> { "Item1", "Item2" };
    var response = new PagedResponse<string>(items, 10, 2, 2);

    // Act
    var hasPreviousPage = response.HasPreviousPage;

    // Assert
    Assert.True(hasPreviousPage);
  }

  [Fact]
  public void HasPreviousPage_ShouldReturnFalse_WhenPageNumberIs1() {
    // Arrange
    var items = new List<string> { "Item1", "Item2" };
    var response = new PagedResponse<string>(items, 10, 1, 2);

    // Act
    var hasPreviousPage = response.HasPreviousPage;

    // Assert
    Assert.False(hasPreviousPage);
  }

  [Fact]
  public void HasNextPage_ShouldReturnTrue_WhenPageNumberLessThanTotalPages() {
    // Arrange
    var items = new List<string> { "Item1", "Item2" };
    var response = new PagedResponse<string>(items, 10, 1, 2);

    // Act
    var hasNextPage = response.HasNextPage;

    // Assert
    Assert.True(hasNextPage);
  }

  [Fact]
  public void HasNextPage_ShouldReturnFalse_WhenPageNumberEqualsTotalPages() {
    // Arrange
    var items = new List<string> { "Item1", "Item2" };
    var response = new PagedResponse<string>(items, 10, 5, 2);

    // Act
    var hasNextPage = response.HasNextPage;

    // Assert
    Assert.False(hasNextPage);
  }

  [Fact]
  public void Constructor_ShouldInitializePropertiesCorrectly() {
    // Arrange
    var items = new List<string> { "Item1", "Item2" };

    // Act
    var response = new PagedResponse<string>(items, 10, 1, 2);

    // Assert
    Assert.Equal(items, response.Items);
    Assert.Equal(10, response.TotalCount);
    Assert.Equal(1, response.PageNumber);
    Assert.Equal(2, response.PageSize);
  }
}