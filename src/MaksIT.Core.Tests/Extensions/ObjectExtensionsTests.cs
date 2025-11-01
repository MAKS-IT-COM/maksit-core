using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MaksIT.Core.Extensions;
using Xunit;

namespace MaksIT.Core.Tests.Extensions {
  public class ObjectExtensionsTests {
    private class TestObject {
      public required string Name { get; set; }
      public int Age { get; set; }
      public string? Address { get; set; }
    }

    private class CustomDateTimeConverter : JsonConverter<DateTime> {
      private readonly string _format;

      public CustomDateTimeConverter(string format) {
        _format = format;
      }

      public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var dateString = reader.GetString();
        if (dateString is null) {
          throw new JsonException("Expected a date string but got null.");
        }

        return DateTime.ParseExact(dateString, _format, null);
      }

      public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString(_format));
      }
    }

    [Fact]
    public void ToJson_WithNullObject_ShouldReturnEmptyJson() {
      // Arrange
      TestObject? obj = null;

      // Act
      var result = obj.ToJson();

      // Assert
      Assert.Equal("{}", result);
    }

    [Fact]
    public void ToJson_WithSimpleObject_ShouldReturnCorrectJson() {
      // Arrange
      var obj = new TestObject {
        Name = "John Doe",
        Age = 30
      };

      // Act
      var result = obj.ToJson();

      // Assert
      Assert.Equal("{\"name\":\"John Doe\",\"age\":30}", result);
    }

    [Fact]
    public void ToJson_WithObjectAndNullValues_ShouldIgnoreNullProperties() {
      // Arrange
      var obj = new TestObject {
        Name = "John Doe",
        Age = 30,
        Address = null
      };

      // Act
      var result = obj.ToJson();

      // Assert
      Assert.Equal("{\"name\":\"John Doe\",\"age\":30}", result);
    }

    [Fact]
    public void ToJson_WithCustomJsonConverter_ShouldApplyConverter() {
      // Arrange
      var obj = new DateTime(2023, 08, 30);
      var converters = new List<JsonConverter> { new CustomDateTimeConverter("yyyy-MM-dd") };

      // Act
      var result = obj.ToJson(converters);

      // Assert
      Assert.Equal("\"2023-08-30\"", result);
    }

    [Fact]
    public void ToJson_WithComplexObjectAndConverters_ShouldSerializeCorrectly() {
      // Arrange
      var obj = new {
        Name = "Jane Doe",
        BirthDate = new DateTime(1990, 12, 25)
      };

      var converters = new List<JsonConverter> { new CustomDateTimeConverter("yyyy/MM/dd") };

      // Act
      var result = obj.ToJson(converters);

      // Assert
      Assert.Equal("{\"name\":\"Jane Doe\",\"birthDate\":\"1990/12/25\"}", result);
    }

    [Fact]
    public void ToJson_WithEmptyObject_ShouldReturnEmptyJsonObject() {
      // Arrange
      var obj = new { };

      // Act
      var result = obj.ToJson();

      // Assert
      Assert.Equal("{}", result);
    }

    // ------- DeepClone / DeepEqual / RevertFrom tests below -------

    private class Person {
      public string Name = "";
      public int Age;
      private string _secret = "xyz";
      public string Secret => _secret;
      public void SetSecret(string s) { _secret = s; }
      public Address? Addr;
    }

    private class Address {
      public string City = "";
      public Person? Owner; // cycle back to person
    }

    private struct Score {
      public int A;
      public List<Person>? People; // ref-type field inside struct
    }

    [Fact]
    public void DeepClone_WithSimpleGraph_ShouldProduceIndependentCopy() {
      // Arrange
      var p = new Person { Name = "Alice", Age = 25, Addr = new Address { City = "Rome" } };

      // Act
      var clone = p.DeepClone();

      // Assert
      Assert.NotSame(p, clone);
      Assert.Equal("Alice", clone.Name);
      Assert.Equal(25, clone.Age);
      Assert.NotSame(p.Addr, clone.Addr);
      Assert.Equal("Rome", clone.Addr!.City);

      // Mutate clone should not affect original
      clone.Name = "Bob";
      clone.Addr.City = "Milan";
      clone.SetSecret("new");
      Assert.Equal("Alice", p.Name);
      Assert.Equal("Rome", p.Addr!.City);
      Assert.Equal("xyz", p.Secret);
    }

    [Fact]
    public void DeepClone_ShouldPreserveCyclesAndReferenceIdentity() {
      // Arrange
      var p = new Person { Name = "Root" };
      var a = new Address { City = "Naples" };
      p.Addr = a;
      a.Owner = p; // create cycle

      // Act
      var clone = p.DeepClone();

      // Assert
      Assert.NotSame(p, clone);
      Assert.NotSame(p.Addr, clone.Addr);
      Assert.Same(clone, clone.Addr!.Owner); // cycle preserved in clone
    }

    [Fact]
    public void DeepClone_ShouldHandleStructsWithReferenceFields() {
      // Arrange
      var s = new Score {
        A = 7,
        People = new List<Person> { new Person { Name = "P1" } }
      };

      // Act
      var sClone = s.DeepClone();

      // Assert
      Assert.Equal(7, sClone.A);
      Assert.NotSame(s.People, sClone.People);
      Assert.NotSame(s.People![0], sClone.People![0]);
      Assert.Equal("P1", sClone.People[0].Name);
    }

    [Fact]
    public void DeepClone_ShouldHandleArraysAndMultiDimensional() {
      // Arrange
      var arr = new[] { new Person { Name = "A" }, new Person { Name = "B" } };
      var md = (Person[,])Array.CreateInstance(typeof(Person), new[] { 1, 2 }, new[] { 1, 1 });
      md[1, 1] = arr[0];
      md[1, 2] = arr[1];

      // Act
      var arrClone = arr.DeepClone();
      var mdClone = md.DeepClone();

      // Assert
      Assert.NotSame(arr, arrClone);
      Assert.NotSame(arr[0], arrClone[0]);
      Assert.Equal("A", arrClone[0].Name);

      Assert.NotSame(md, mdClone);
      Assert.Equal(md.GetLowerBound(0), mdClone.GetLowerBound(0));
      Assert.Equal(md.GetLowerBound(1), mdClone.GetLowerBound(1));
      Assert.NotSame(md[1, 1], mdClone[1, 1]);
      Assert.Equal("A", mdClone[1, 1].Name);
    }

    [Fact]
    public void DeepClone_ShouldReturnSameReferenceForImmutable() {
      // Arrange
      var s = "hello";

      // Act
      var s2 = s.DeepClone();

      // Assert
      Assert.Same(s, s2);
    }

    [Fact]
    public void DeepEqual_ShouldReturnTrue_ForEqualGraphs() {
      // Arrange
      var p1 = new Person { Name = "Alice", Age = 30, Addr = new Address { City = "Turin" } };
      var p2 = new Person { Name = "Alice", Age = 30, Addr = new Address { City = "Turin" } };

      // Act
      var equal = p1.DeepEqual(p2);

      // Assert
      Assert.True(equal);
    }

    [Fact]
    public void DeepEqual_ShouldReturnFalse_WhenAnyFieldDiffers() {
      // Arrange
      var p1 = new Person { Name = "Alice", Age = 30, Addr = new Address { City = "Turin" } };
      var p2 = new Person { Name = "Alice", Age = 31, Addr = new Address { City = "Turin" } };

      // Act
      var equal = p1.DeepEqual(p2);

      // Assert
      Assert.False(equal);
    }

    [Fact]
    public void DeepEqual_ShouldHandleCycles() {
      // Arrange
      var p1 = new Person { Name = "R", Addr = new Address { City = "X" } };
      p1.Addr!.Owner = p1;
      var p2 = new Person { Name = "R", Addr = new Address { City = "X" } };
      p2.Addr!.Owner = p2;

      // Act
      var equal = p1.DeepEqual(p2);

      // Assert
      Assert.True(equal);
    }

    [Fact]
    public void RevertFrom_ShouldCopyStateBackOntoExistingInstance() {
      // Arrange
      var original = new Person { Name = "Alice", Age = 20, Addr = new Address { City = "Parma" } };
      var snapshot = original.DeepClone();

      // Mutate original
      original.Name = "Changed";
      original.Age = 99;
      original.Addr!.City = "ChangedCity";
      original.SetSecret("changed-secret");

      // Act
      original.RevertFrom(snapshot);

      // Assert
      Assert.Equal("Alice", original.Name);
      Assert.Equal(20, original.Age);
      Assert.Equal("Parma", original.Addr!.City);
      Assert.Equal("xyz", original.Secret);
    }

    [Fact]
    public void DeepEqual_NullsAndTypeMismatch_ShouldBehaveCorrectly() {
      // Arrange
      Person? a = null;
      Person? b = null;
      var c = new Person();
      var d = new TestObject { Name = "x", Age = 1 };

      // Act / Assert
      Assert.True(a.DeepEqual(b));
      Assert.False(a.DeepEqual(c));
      // Different runtime types must be false
      Assert.False(c.DeepEqual((object)d));
    }
  }
}
