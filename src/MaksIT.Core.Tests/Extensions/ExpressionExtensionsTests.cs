using System.Linq.Expressions;

using MaksIT.Core.Extensions;


namespace MaksIT.Core.Tests.Extensions;

public class ExpressionExtensionsTests {

  [Fact]
  public void AndAlso_ShouldCombineTwoPredicatesWithAndCondition() {
    // Arrange
    Expression<Func<TestEntity, bool>> firstPredicate = x => x.Age > 18;
    Expression<Func<TestEntity, bool>> secondPredicate = x => (x.Name ?? "").StartsWith("A");

    // Act
    var combinedPredicate = firstPredicate.AndAlso(secondPredicate);
    var compiledPredicate = combinedPredicate.Compile();

    // Assert
    Assert.True(compiledPredicate(new TestEntity { Age = 20, Name = "Alice" }));
    Assert.False(compiledPredicate(new TestEntity { Age = 17, Name = "Alice" }));
    Assert.False(compiledPredicate(new TestEntity { Age = 20, Name = "Bob" }));
  }

  [Fact]
  public void OrElse_ShouldCombineTwoPredicatesWithOrCondition() {
    // Arrange
    Expression<Func<TestEntity, bool>> firstPredicate = x => x.Age > 18;
    Expression<Func<TestEntity, bool>> secondPredicate = x => (x.Name ?? "").StartsWith("A");

    // Act
    var combinedPredicate = firstPredicate.OrElse(secondPredicate);
    var compiledPredicate = combinedPredicate.Compile();

    // Assert
    Assert.True(compiledPredicate(new TestEntity { Age = 20, Name = "Alice" }));
    Assert.True(compiledPredicate(new TestEntity { Age = 17, Name = "Alice" }));
    Assert.True(compiledPredicate(new TestEntity { Age = 20, Name = "Bob" }));
    Assert.False(compiledPredicate(new TestEntity { Age = 17, Name = "Bob" }));
  }

  [Fact]
  public void Not_ShouldNegatePredicate() {
    // Arrange
    Expression<Func<TestEntity, bool>> predicate = x => x.Age > 18;

    // Act
    var negatedPredicate = predicate.Not();
    var compiledPredicate = negatedPredicate.Compile();

    // Assert
    Assert.False(compiledPredicate(new TestEntity { Age = 20 }));
    Assert.True(compiledPredicate(new TestEntity { Age = 17 }));
  }

  [Fact]
  public void AndAlso_ShouldHandleNullValues() {
    // Arrange
    Expression<Func<TestEntity, bool>> firstPredicate = x => x.Name != null;
    Expression<Func<TestEntity, bool>> secondPredicate = x => (x.Name ?? "").Length > 3;

    // Act
    var combinedPredicate = firstPredicate.AndAlso(secondPredicate);
    var compiledPredicate = combinedPredicate.Compile();

    // Assert
    Assert.False(compiledPredicate(new TestEntity { Name = null }));
    Assert.True(compiledPredicate(new TestEntity { Name = "John" }));
  }

  [Fact]
  public void Not_ShouldThrowExceptionForNullExpression() {
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => ExpressionExtensions.Not<TestEntity>(null!));
  }

  [Fact]
  public void AndAlso_ShouldThrowExceptionForNullExpression() {
    // Arrange
    Expression<Func<TestEntity, bool>> firstPredicate = null!;
    Expression<Func<TestEntity, bool>> secondPredicate = x => x.Age > 18;

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => firstPredicate.AndAlso(secondPredicate));
  }

  [Fact]
  public void OrElse_ShouldThrowExceptionForNullExpression() {
    // Arrange
    Expression<Func<TestEntity, bool>> firstPredicate = x => x.Age > 18;
    Expression<Func<TestEntity, bool>> secondPredicate = null!;

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => firstPredicate.OrElse(secondPredicate));
  }

  [Fact]
  public void Batch_ShouldDivideCollectionIntoBatchesOfGivenSize() {
    // Arrange
    var source = Enumerable.Range(1, 10);
    int batchSize = 3;

    // Act
    var batches = source.Batch(batchSize).ToList();

    // Assert
    Assert.Equal(4, batches.Count);
    Assert.Equal(new List<int> { 1, 2, 3 }, batches[0]);
    Assert.Equal(new List<int> { 4, 5, 6 }, batches[1]);
    Assert.Equal(new List<int> { 7, 8, 9 }, batches[2]);
    Assert.Equal(new List<int> { 10 }, batches[3]);
  }

  private class TestEntity {
    public Guid Id { get; set; }
    public int Age { get; set; }
    public string? Name { get; set; }
  }
}