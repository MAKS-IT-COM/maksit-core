using System.Linq;
using Xunit;
using MaksIT.Core.Webapi.Models;
using MaksIT.Core.Extensions;  // Ensure namespace matches the actual namespace of PagedRequest

namespace MaksIT.Core.Tests.Webapi.Models;

public class PagedRequestTests {

  public class TestEntity {
    public required string Name { get; set; }
    public required int Age { get; set; }
  }

  public class Application {
    public required string Name { get; set; }
    public Guid OrganizationId { get; set; }
  }

  // Setup a mock IQueryable to test against
  private IQueryable<TestEntity> GetTestQueryable() {
    return new List<TestEntity> {
            new TestEntity { Name = "John", Age = 31 },
            new TestEntity { Name = "Jane", Age = 29 },
            new TestEntity { Name = "Doe", Age = 35 }
        }.AsQueryable();
  }

  private IQueryable<Application> GetApplicationsTestQueryable() {
    return new List<Application> {
      new Application { Name = "NuGet", OrganizationId = "e1a92b8e-9f3f-4b3f-80c3-01941360e1fe".ToGuid()}
    }.AsQueryable();
  
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleEqualsOperator() {
    // Arrange
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "name == \"John\""
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
      Filters = "name != \"John\""
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.DoesNotContain(filtered, t => t.Name == "john");
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleGreaterThanOperator() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "age > 30"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.All(filtered, t => Assert.True(t.Age > 30));
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleLessThanOperator() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "age < 30"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.All(filtered, t => Assert.True(t.Age < 30));
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleAndOperator() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "name == \"john\" && age > 30"
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
      Filters = "name == \"john\" || age > 30"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.Contains(filtered, t => t.Name == "John" || t.Age > 30);
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleNegation() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "!(name == \"john\")"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.DoesNotContain(filtered, t => t.Name == "John");
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleContainsOperator() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "name.Contains(\"oh\")"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.Contains(filtered, t => t.Name.Contains("oh"));
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleStartsWithOperator() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "name.StartsWith(\"jo\")"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.Contains(filtered, t => t.Name.StartsWith("Jo"));
    Assert.Single(filtered); // Assuming only "John" starts with "Jo"
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleEqualsAndContainsOperators() {
    var queryable = GetApplicationsTestQueryable();
    var request = new PagedRequest {
      Filters = "organizationId == \"e1a92b8e-9f3f-4b3f-80c3-01941360e1fe\" && (name.Contains(\"nuge\"))"
    };

    var predicate = request.BuildFilterExpression<Application>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.Contains(filtered, t => t.Name.ToLower().Contains("nuge"));
    Assert.Single(filtered); // Assuming only one entity matches the criteria
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleEndsWithOperator() {
    var queryable = GetTestQueryable();
    var request = new PagedRequest {
      Filters = "name.EndsWith(\"hn\")"
    };

    var predicate = request.BuildFilterExpression<TestEntity>();
    var filtered = queryable.Where(predicate).ToList();

    Assert.Contains(filtered, t => t.Name.EndsWith("hn"));
  }


}
