namespace MaksIT.Core.Tests.Webapi.Models;

public class PagedRequestTests {

  public class TestEntity {
    public string? Name { get; set; }
    public int Age { get; set; }
  }

  // Setup a mock IQueryable to test against
  private IQueryable<TestEntity> GetTestQueryable() {
    return new List<TestEntity> {
            new TestEntity { Name = "John", Age = 31 },
            new TestEntity { Name = "Jane", Age = 29 },
            new TestEntity { Name = "Doe", Age = 35 }
        }.AsQueryable();
  }

  [Fact]
  public void ApplyFilters_ShouldHandleEqualsOperator() {
    // Arrange
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Name == \"John\""
    };

    // Act
    var filtered = request.ApplyFilters(queryable);

    // Assert
    Assert.Contains(filtered, t => t.Name == "John");
    Assert.Single(filtered);
  }

  [Fact]
  public void ApplyFilters_ShouldHandleNotEqualsOperator() {
    // Arrange
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Name != \"John\""
    };

    // Act
    var filtered = request.ApplyFilters(queryable);

    // Assert
    Assert.DoesNotContain(filtered, t => t.Name == "John");
  }

  [Fact]
  public void ApplyFilters_ShouldHandleGreaterThanOperator() {
    // Arrange
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Age > 30"
    };

    // Act
    var filtered = request.ApplyFilters(queryable);

    // Assert
    Assert.All(filtered, t => Assert.True(t.Age > 30));
  }

  [Fact]
  public void ApplyFilters_ShouldHandleLessThanOperator() {
    // Arrange
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Age < 30"
    };

    // Act
    var filtered = request.ApplyFilters(queryable);

    // Assert
    Assert.All(filtered, t => Assert.True(t.Age < 30));
  }

  [Fact]
  public void ApplyFilters_ShouldHandleAndOperator() {
    // Arrange
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Name == \"John\" && Age > 30"
    };

    // Act
    var filtered = request.ApplyFilters(queryable);

    // Assert
    Assert.Contains(filtered, t => t.Name == "John" && t.Age > 30);
    Assert.Single(filtered);
  }

  [Fact]
  public void ApplyFilters_ShouldHandleOrOperator() {
    // Arrange
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Name == \"John\" || Age > 30"
    };

    // Act
    var filtered = request.ApplyFilters(queryable);

    // Assert
    Assert.Contains(filtered, t => t.Name == "John" || t.Age > 30);
  }

  [Fact]
  public void ApplyFilters_ShouldHandleNegation() {
    // Arrange
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "!(Name == \"John\")"
    };

    // Act
    var filtered = request.ApplyFilters(queryable);

    // Assert
    Assert.DoesNotContain(filtered, t => t.Name == "John");
  }

  [Fact]
  public void ApplyFilters_ShouldHandleContainsOperator() {
    // Arrange
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Name.Contains(\"oh\")"
    };

    // Act
    var filtered = request.ApplyFilters(queryable);

    // Assert
    Assert.Contains(filtered, t => t.Name.Contains("oh"));
  }

  [Fact]
  public void ApplyFilters_ShouldHandleStartsWithOperator() {
    // Arrange
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Name.StartsWith(\"Jo\")"
    };

    // Act
    var filtered = request.ApplyFilters(queryable);

    // Assert
    Assert.Contains(filtered, t => t.Name.StartsWith("John"));
    Assert.Single(filtered); // Assuming only "Johnny" starts with "John"
  }

  [Fact]
  public void ApplyFilters_ShouldHandleEndsWithOperator() {
    // Arrange
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Name.EndsWith(\"hn\")"
    };

    // Act
    var filtered = request.ApplyFilters(queryable);

    // Assert
    Assert.Contains(filtered, t => t.Name.EndsWith("hn"));
  }
}
