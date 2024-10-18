
using MaksIT.Core.Webapi.Models;

namespace MaksIT.Core.Tests.Webapi.Models;

public class PagedRequestTests {

  [Fact]
  public void BuildFilterExpression_ShouldHandleEqualsOperator() {
    // Arrange
    var request = new PagedRequest {
      Filters = "Name='John'"
    };

    // Act
    var expression = request.BuildFilterExpression<TestEntity>(request.Filters);
    var compiled = expression!.Compile();

    // Assert
    var testEntity = new TestEntity { Name = "John" };
    Assert.True(compiled(testEntity));
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleNotEqualsOperator() {
    // Arrange
    var request = new PagedRequest {
      Filters = "Name!='John'"
    };

    // Act
    var expression = request.BuildFilterExpression<TestEntity>(request.Filters);
    var compiled = expression!.Compile();

    // Assert
    var testEntity = new TestEntity { Name = "John" };
    Assert.False(compiled(testEntity));
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleGreaterThanOperator() {
    // Arrange
    var request = new PagedRequest {
      Filters = "Age>30"
    };

    // Act
    var expression = request.BuildFilterExpression<TestEntity>(request.Filters);
    var compiled = expression!.Compile();

    // Assert
    var testEntity = new TestEntity { Age = 31 };
    Assert.True(compiled(testEntity));
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleLessThanOperator() {
    // Arrange
    var request = new PagedRequest {
      Filters = "Age<30"
    };

    // Act
    var expression = request.BuildFilterExpression<TestEntity>(request.Filters);
    var compiled = expression!.Compile();

    // Assert
    var testEntity = new TestEntity { Age = 29 };
    Assert.True(compiled(testEntity));
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleAndOperator() {
    // Arrange
    var request = new PagedRequest {
      Filters = "Name='John' && Age>30"
    };

    // Act
    var expression = request.BuildFilterExpression<TestEntity>(request.Filters);
    var compiled = expression!.Compile();

    // Assert
    var testEntity = new TestEntity { Name = "John", Age = 31 };
    Assert.True(compiled(testEntity));
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleOrOperator() {
    // Arrange
    var request = new PagedRequest {
      Filters = "Name='John' || Age>30"
    };

    // Act
    var expression = request.BuildFilterExpression<TestEntity>(request.Filters);
    var compiled = expression!.Compile();

    // Assert
    var testEntity = new TestEntity { Name = "Doe", Age = 31 };
    Assert.True(compiled(testEntity));
  }

  [Fact]
  public void BuildFilterExpression_ShouldHandleNegation() {
    // Arrange
    var request = new PagedRequest {
      Filters = "!Name='John'"
    };

    // Act
    var expression = request.BuildFilterExpression<TestEntity>(request.Filters);
    var compiled = expression!.Compile();

    // Assert
    var testEntity = new TestEntity { Name = "Doe" };
    Assert.True(compiled(testEntity));
  }
}

// Helper class for testing purposes
public class TestEntity {
  public string? Name { get; set; }
  public int Age { get; set; }
}