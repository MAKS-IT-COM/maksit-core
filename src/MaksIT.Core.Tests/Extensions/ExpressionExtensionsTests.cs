using System.Linq.Expressions;

using MaksIT.Core.Extensions;


namespace MaksIT.Core.Tests.Extensions;

public class ExpressionExtensionsTests {
  [Fact]
  public void CreateContainsPredicate_ShouldReturnTrue_WhenIdIsInList() {
    // Arrange
    var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    var targetId = ids[1];
    var predicate = ExpressionExtensions.CreateContainsPredicate<TestEntity>(ids, nameof(TestEntity.Id));

    // Act
    var result = predicate.Compile()(new TestEntity { Id = targetId });

    // Assert
    Assert.True(result);
  }

  [Fact]
  public void CreateContainsPredicate_ShouldReturnFalse_WhenIdIsNotInList() {
    // Arrange
    var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    var targetId = Guid.NewGuid();
    var predicate = ExpressionExtensions.CreateContainsPredicate<TestEntity>(ids, nameof(TestEntity.Id));

    // Act
    var result = predicate.Compile()(new TestEntity { Id = targetId });

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void CombineWith_ShouldCombineTwoPredicates() {
    // Arrange
    Expression<Func<TestEntity, bool>> firstPredicate = x => x.Age > 18;
    Expression<Func<TestEntity, bool>> secondPredicate = x => x.Name.StartsWith("A");

    // Act
    var combinedPredicate = firstPredicate.CombineWith(secondPredicate);
    var compiledPredicate = combinedPredicate.Compile();

    // Assert
    Assert.True(compiledPredicate(new TestEntity { Age = 20, Name = "Alice" }));
    Assert.False(compiledPredicate(new TestEntity { Age = 17, Name = "Alice" }));
    Assert.False(compiledPredicate(new TestEntity { Age = 20, Name = "Bob" }));
  }

  private class TestEntity {
    public Guid Id { get; set; }
    public int Age { get; set; }
    public string Name { get; set; }
  }
}

