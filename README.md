# MaksIT.Core

MaksIT.Core is a collection of helper methods and extensions for .NET projects, designed to simplify common tasks and improve code readability. The library includes extensions for `Guid`, `string`, `Object`, and a base class for creating enumeration types.

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
  - [Enumeration](#enumeration)
  - [Guid Extensions](#guid-extensions)
  - [Object Extensions](#object-extensions)
  - [String Extensions](#string-extensions)
- [Available Methods](#available-methods)
  - [Enumeration Methods](#enumeration-methods)
  - [Guid Methods](#guid-methods)
  - [Object Methods](#object-methods)
  - [String Methods](#string-methods)
- [Contributing](#contributing)
- [License](#license)

## Installation

To install MaksIT.Core, add the package to your project via NuGet:

```sh
dotnet add package MaksIT.Core
```

Or manually add it to your `.csproj` file:

```xml
<PackageReference Include="MaksIT.Core" Version="1.0.0" />
```

## Usage

### Enumeration

The `Enumeration` base class provides a way to create strongly-typed enums in C#. This is useful for scenarios where you need more functionality than the default `enum` type.

**Example:**

```csharp
public class Status : Enumeration
{
    public static readonly Status Active = new Status(1, "Active");
    public static readonly Status Inactive = new Status(2, "Inactive");

    private Status(int id, string name) : base(id, name) { }
}

// Usage
var activeStatus = Status.FromValue<Status>(1);
Console.WriteLine(activeStatus.Name); // Output: Active
```

### Guid Extensions

The `GuidExtensions` class contains extensions for working with `Guid` types.

**Example:**

```csharp
Guid guid = Guid.NewGuid();
Guid? nullableGuid = guid.ToNullable();
```

### Object Extensions

The `ObjectExtensions` class provides extensions for working with objects.

**Example:**

```csharp
var person = new { Name = "John", Age = 30 };
string json = person.ToJson();
Console.WriteLine(json); // Output: {"name":"John","age":30}
```

### String Extensions

The `StringExtensions` class provides a variety of useful string manipulation methods.

**Example:**

```csharp
string text = "Hello World";
bool isLike = text.Like("Hello*"); // SQL-like matching
Console.WriteLine(isLike); // Output: True
```

## Available Methods

### Enumeration Methods

- **`GetAll<T>()`**: Retrieves all static fields of a given type `T` that derive from `Enumeration`.
- **`Equals(object? obj)`**: Determines whether the specified object is equal to the current object.
- **`GetHashCode()`**: Returns the hash code for the current object.
- **`AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)`**: Computes the absolute difference between two enumeration values.
- **`FromValue<T>(int value)`**: Retrieves an instance of type `T` from its integer value.
- **`FromDisplayName<T>(string displayName)`**: Retrieves an instance of type `T` from its display name.
- **`CompareTo(object? other)`**: Compares the current instance with another object of the same type.

### Guid Methods

- **`ToNullable(this Guid id)`**: Converts a `Guid` to a nullable `Guid?`. Returns `null` if the `Guid` is `Guid.Empty`.

### Object Methods

- **`ToJson<T>(this T? obj)`**: Converts an object to a JSON string using default serialization options.
- **`ToJson<T>(this T? obj, List<JsonConverter>? converters)`**: Converts an object to a JSON string using custom converters.

### String Methods

- **`Like(this string? text, string? wildcardedText)`**: Determines if a string matches a given wildcard pattern (SQL LIKE).
- **`Left(this string s, int count)`**: Returns the left substring of the specified length.
- **`Right(this string s, int count)`**: Returns the right substring of the specified length.
- **`Mid(this string s, int index, int count)`**: Returns a substring starting from the specified index with the specified length.
- **`ToInteger(this string s)`**: Converts a string to an integer, returning zero if conversion fails.
- **`IsInteger(this string s)`**: Determines whether the string represents an integer.
- **`Prepend(this StringBuilder sb, string content)`**: Prepends content to the beginning of a `StringBuilder`.
- **`ToEnum<T>(this string input)`**: Converts a string to an enum value of type `T`.
- **`ToNullableEnum<T>(this string input)`**: Converts a string to a nullable enum value of type `T`.
- **`ToNull(this string s)`**: Returns `null` if the string is empty or whitespace.
- **`NullIfEmptyString(this string s)`**: Returns `null` if the string is empty or whitespace, otherwise returns the original string.
- **`ToLong(this string s)`**: Converts a string to a long, returning a hash code if conversion fails.
- **`ToNullableLong(this string s)`**: Converts a string to a nullable long, returning `null` if conversion fails.
- **`ToInt(this string s)`**: Converts a string to an int, returning a hash code if conversion fails.
- **`ToNullableInt(this string s)`**: Converts a string to a nullable int, returning `null` if conversion fails.
- **`ToUint(this string s)`**: Converts a string to a uint, returning a hash code if conversion fails.
- **`ToNullableUint(this string s)`**: Converts a string to a nullable uint, returning `null` if conversion fails.
- **`ToDecimal(this string s)`**: Converts a string to a decimal, returning a hash code if conversion fails.
- **`ToNullableDecimal(this string s)`**: Converts a string to a nullable decimal, returning `null` if conversion fails.
- **`ToDouble(this string s)`**: Converts a string to a double, returning a hash code if conversion fails.
- **`ToNullableDouble(this string s)`**: Converts a string to a nullable double, returning `null` if conversion fails.
- **`ToDate(this string s, string[] formats)`**: Converts a string to a `DateTime` object using a specified format.
- **`ToDate(this string s)`**: Converts a string to a `DateTime` object using the default format.
- **`ToNullableDate(this string s)`**: Converts a string to a nullable `DateTime` object using the default format.
- **`ToNullableDate(this string s, string[] formats)`**: Converts a string to a nullable `DateTime` object using specified formats.
- **`ToDateTime(this string s, string[] formats)`**: Converts a string to a `DateTime` object using specified formats.
- **`ToDateTime(this string s)`**: Converts a string to a `DateTime` object using the default formats.
- **`ToNullableDateTime(this string s)`**: Converts a string to a nullable `DateTime` object using the default formats.
- **`ToNullableDateTime(this string s, string[] formats)`**: Converts a string to a nullable `DateTime` object using specified formats.
- **`ToBool(this string s)`**: Converts a string to a boolean.
- **`ToNullableBool(this string s)`**: Converts a string to a nullable boolean.
- **`ToGuid(this string text)`**: Converts a string to a `Guid`.
- **`ToNullableGuid(this string s)`**: Converts a string to a nullable `Guid`.
- **`StringSplit(this string s, char c)`**: Splits a string by a specified character and trims each resulting element.
- **`ToTitle(this string s)`**: Converts the first character of the string to uppercase.
- **`ExtractUrls(this string s)`**: Extracts all URLs from a string.
- **`Format(this string s, params object[] args)`**: Formats a string using specified arguments.
- **`Excerpt(this string s, int length = 60)`**: Truncates a string to a specified length, adding ellipses if necessary.
- **`ToObject<T>(this string s)`**: Deserializes a JSON string into an object of type `T`.
- **`ToObject<T>(this string s, List<JsonConverter> converters)`**: Deserializes a JSON string into an object of type `T` using custom converters.
- **`IsValidEmail(this string? s)`**: Validates whether the string is a valid email format.
- **`HtmlToPlainText(this string htmlCode)`**: Converts HTML content to plain text.
- **`ToCamelCase(this string input)`**: Converts a string to camel case.

## Contribution

Contributions to this project are welcome! Please fork the repository and submit a pull request with your changes. If you encounter any issues or have feature requests, feel free to open an issue on GitHub.

## License

This project is licensed under the MIT License. See the full license text below.

---

### MIT License

```
MIT License

Copyright (c) 2024 Maksym Sadovnychyy (MAKS-IT)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## Contact

For any questions or inquiries, please reach out via GitHub or [email](mailto:maksym.sadovnychyy@gmail.com).