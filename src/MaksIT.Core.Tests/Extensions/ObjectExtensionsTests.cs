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
  }
}
