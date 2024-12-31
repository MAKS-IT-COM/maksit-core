using System.Linq;
using Xunit;
using MaksIT.Core.Webapi.Models;  // Ensure namespace matches the actual namespace of PagedRequest

namespace MaksIT.Core.Tests.Webapi.Models;

public class PagedRequestTests {

  public class TestEntity {
    public required string Name { get; set; }
    public required int Age { get; set; }
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
  public void BuildFilterExpression_ShouldHandleEqualsOperator() {
    // Arrange
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Name == \"John\""
    };

    // Act
    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    // Assert
    Assert.Contains(filtered, t => t.Name == "John");
    Assert.Single(filtered);
  }

  // Add similar changes for other tests
  [Fact]
  public void BuildFilterExpression_ShouldHandleNotEqualsOperator() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Name != \"John\""
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.DoesNotContain(filtered, t => t.Name == "John");
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleGreaterThanOperator() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Age > 30"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.All(filtered, t => Assert.True(t.Age > 30));
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleLessThanOperator() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Age < 30"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.All(filtered, t => Assert.True(t.Age < 30));
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleAndOperator() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Name == \"John\" && Age > 30"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.Contains(filtered, t => t.Name == "John" && t.Age > 30);
    Assert.Single(filtered);
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleOrOperator() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Name == \"John\" || Age > 30"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.Contains(filtered, t => t.Name == "John" || t.Age > 30);
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleNegation() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "!(Name == \"John\")"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.DoesNotContain(filtered, t => t.Name == "John");
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleContainsOperator() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Name.Contains(\"oh\")"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.Contains(filtered, t => t.Name.Contains("oh"));
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleStartsWithOperator() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Name.StartsWith(\"Jo\")"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.Contains(filtered, t => t.Name.StartsWith("Jo"));
    Assert.Single(filtered); // Assuming only "John" starts with "Jo"
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleEndsWithOperator() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "Name.EndsWith(\"hn\")"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.Contains(filtered, t => t.Name.EndsWith("hn"));
  }
}
