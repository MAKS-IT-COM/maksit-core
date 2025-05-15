# MaksIT.Core Library Documentation

## Table of Contents

- [Abstractions](#abstractions)
  - [Base Classes](#base-classes)
  - [Eunumeration](#eunumeration)
- [Extesnions](#extensions)
- [Logging](#logging)
- [Networking](#networking)
- [Security](#security)
- [Web API Models](#web-api-models)
- [Others](#others)


## Abstractions

### Base Classes

The following base classes in the `MaksIT.Core.Abstractions` namespace provide a foundation for implementing domain, DTO, and Web API models, ensuring consistency and maintainability in application design.

---

##### 1. **`DomainObjectBase`**

###### Summary
Represents the base class for all domain objects in the application.

###### Purpose
- Serves as the foundation for all domain objects.
- Provides a place to include shared logic or properties for domain-level entities in the future.

---

##### 2. **`DomainDocumentBase<T>`**

###### Summary
Represents a base class for domain documents with a unique identifier.

###### Purpose
- Extends `DomainObjectBase` to include an identifier.
- Provides a common structure for domain entities that need unique IDs.

###### Example Usage
```csharp
public class UserDomainDocument : DomainDocumentBase<Guid> {
    public UserDomainDocument(Guid id) : base(id) {
    }
}
```

---

##### 3. **`DtoObjectBase`**

###### Summary
Represents the base class for all Data Transfer Objects (DTOs).

###### Purpose
- Serves as the foundation for all DTOs.
- Provides a place to include shared logic or properties for DTOs in the future.

---

##### 4. **`DtoDocumentBase<T>`**

###### Summary
Represents a base class for DTOs with a unique identifier.

###### Purpose
- Extends `DtoObjectBase` to include an identifier.
- Provides a common structure for DTOs that need unique IDs.

###### Example Usage
```csharp
public class UserDto : DtoDocumentBase<Guid> {
    public required string Name { get; set; }
}
```

---

##### 5. **`RequestModelBase`**

###### Summary
Represents the base class for Web API request models.

###### Purpose
- Serves as a foundation for request models used in Web API endpoints.
- Provides a common structure for request validation or shared properties.

###### Example Usage
```csharp
public class CreateUserRequest : RequestModelBase {
    public required string Name { get; set; }
}
```

---

##### 6. **`ResponseModelBase`**

###### Summary
Represents the base class for Web API response models.

###### Purpose
- Serves as a foundation for response models returned by Web API endpoints.
- Provides a common structure for standardizing API responses.

###### Example Usage
```csharp
public class UserResponse : ResponseModelBase {
    public required Guid Id { get; set; }
    public required string Name { get; set; }
}
```

---

#### Features and Benefits

1. **Consistency**:
   - Ensures a uniform structure for domain, DTO, and Web API models.

2. **Extensibility**:
   - Base classes can be extended to include shared properties or methods as needed.

3. **Type Safety**:
   - Generic identifiers (`T`) ensure type safety for domain documents and DTOs.

4. **Reusability**:
   - Common logic or properties can be added to base classes and reused across the application.

---

#### Example End-to-End Usage

```csharp
// Domain Class
public class ProductDomain : DomainDocumentBase<int> {
    public ProductDomain(int id) : base(id) { }
    public string Name { get; set; } = string.Empty;
}

// DTO Class
public class ProductDto : DtoDocumentBase<int> {
    public required string Name { get; set; }
}

// Web API Request Model
public class CreateProductRequest : RequestModelBase {
    public required string Name { get; set; }
}

// Web API Response Model
public class ProductResponse : ResponseModelBase {
    public required int Id { get; set; }
    public required string Name { get; set; }
}
```

---

#### Best Practices

1. **Keep Base Classes Lightweight**:
   - Avoid adding unnecessary properties or methods to base classes.

2. **Encapsulation**:
   - Use base classes to enforce encapsulation and shared behavior across entities.

3. **Validation**:
   - Extend `RequestModelBase` or `ResponseModelBase` to include validation logic if needed.

---

This structure promotes clean code principles, reducing redundancy and improving maintainability across the application layers.

---

### Eunumeration

The `Enumeration` class in the `MaksIT.Core.Abstractions` namespace provides a base class for creating strongly-typed enumerations. It enables you to define enumerable constants with additional functionality, such as methods for querying, comparing, and parsing enumerations.

---

#### Features and Benefits

1. **Strongly-Typed Enumerations**:
   - Combines the clarity of enums with the extensibility of classes.
   - Supports additional fields, methods, or logic as needed.

2. **Reflection Support**:
   - Dynamically retrieve all enumeration values with `GetAll`.

3. **Parsing Capabilities**:
   - Retrieve enumeration values by ID or display name.

4. **Comparison and Equality**:
   - Fully implements equality and comparison operators for use in collections and sorting.

---

#### Example Usage

#### Defining an Enumeration
```csharp
public class MyEnumeration : Enumeration {
    public static readonly MyEnumeration Value1 = new(1, "Value One");
    public static readonly MyEnumeration Value2 = new(2, "Value Two");

    private MyEnumeration(int id, string name) : base(id, name) { }
}
```

#### Retrieving All Values
```csharp
var allValues = Enumeration.GetAll<MyEnumeration>();
allValues.ToList().ForEach(Console.WriteLine);
```

#### Parsing by ID or Name
```csharp
var valueById = Enumeration.FromValue<MyEnumeration>(1);
var valueByName = Enumeration.FromDisplayName<MyEnumeration>("Value One");

Console.WriteLine(valueById); // Output: Value One
Console.WriteLine(valueByName); // Output: Value One
```

#### Comparing Enumeration Values
```csharp
var difference = Enumeration.AbsoluteDifference(MyEnumeration.Value1, MyEnumeration.Value2);
Console.WriteLine($"Absolute Difference: {difference}"); // Output: 1
```

#### Using in Collections
```csharp
var values = new List<MyEnumeration> { MyEnumeration.Value2, MyEnumeration.Value1 };
values.Sort(); // Orders by ID
```

---

#### Best Practices

1. **Extend for Specific Enums**:
   - Create specific subclasses for each enumeration type.

2. **Avoid Duplicates**:
   - Ensure unique IDs and names for each enumeration value.

3. **Use Reflection Sparingly**:
   - Avoid calling `GetAll` in performance-critical paths.

---

The `Enumeration` class provides a powerful alternative to traditional enums, offering flexibility and functionality for scenarios requiring additional metadata or logic.

---

## Extensions

### DataTableExtensions

The `DataTableExtensions` class is a static utility class in the `MaksIT.Core.Extensions` namespace. It provides extension methods for working with `DataTable` objects, enabling functionality such as counting duplicate rows between two `DataTable` instances and retrieving distinct records based on specified columns.

---

#### Methods

##### 1. **`DuplicatesCount`**

###### Summary
Compares two `DataTable` instances (`dt1` and `dt2`) row by row and counts the number of duplicate rows.

###### Usage
```csharp
DataTable table1 = new DataTable();
DataTable table2 = new DataTable();
// Populate table1 and table2...

int duplicateCount = table1.DuplicatesCount(table2);
Console.WriteLine($"Number of duplicate rows: {duplicateCount}");
```

---

##### 2. **`DistinctRecords`**

###### Summary
Filters a `DataTable` to return distinct rows based on specified columns.

###### Usage
```csharp
DataTable table = new DataTable();
table.Columns.Add("Name");
table.Columns.Add("Age");
// Populate the table...

string[] columns = { "Name", "Age" };
DataTable distinctTable = table.DistinctRecords(columns);

Console.WriteLine("Distinct rows based on Name and Age:");
foreach (DataRow row in distinctTable.Rows)
{
    Console.WriteLine($"{row["Name"]}, {row["Age"]}");
}
```

---

#### Notes
- **Performance Considerations**:
  - The `DuplicatesCount` method uses nested loops, which may impact performance on large data sets. Consider optimization for large tables.
  - The `DistinctRecords` method leverages `DataView.ToTable`, which is efficient for retrieving distinct records but requires valid column names.

- **Edge Cases**:
  - Ensure both `DataTable` instances in `DuplicatesCount` have compatible schemas for meaningful results.
  - For `DistinctRecords`, columns that do not exist in the source `DataTable` will throw an exception.

- **Null Handling**:
  - Both methods ensure that `null` inputs result in a clear exception message.

--- 

This class adds flexibility when working with `DataTable` objects, making it easier to handle duplicate detection and record filtration tasks programmatically.

---

### DateTimeExtensions

The `DateTimeExtensions` class in the `MaksIT.Core.Extensions` namespace provides a set of extension methods for the `DateTime` type, enabling enhanced date manipulation capabilities. These methods simplify common date-related tasks, such as adding workdays, finding specific weekdays, or determining month/year boundaries.

---

#### Methods

##### 1. **`AddWorkdays`**

###### Summary
Adds a specified number of workdays (excluding weekends and holidays) to the given date.

###### Usage
```csharp
DateTime today = DateTime.Today;
IHolidayCalendar holidayCalendar = new CustomHolidayCalendar();
DateTime futureWorkday = today.AddWorkdays(5, holidayCalendar);
```

---

##### 2. **`NextWeekday`**

###### Summary
Finds the next occurrence of a specified weekday starting from a given date.

###### Usage
```csharp
DateTime today = DateTime.Today;
DateTime nextMonday = today.NextWeekday(DayOfWeek.Monday);
```

---

##### 3. **`ToNextWeekday`**

###### Summary
Calculates the time span until the next occurrence of a specified weekday.

###### Usage
```csharp
DateTime today = DateTime.Today;
TimeSpan timeToNextFriday = today.ToNextWeekday(DayOfWeek.Friday);
```

---

##### 4. **`NextEndOfMonth`**

###### Summary
Gets the date of the last day of the next month, preserving the time of the given date.

###### Usage
```csharp
DateTime today = DateTime.Today;
DateTime nextMonthEnd = today.NextEndOfMonth();
```

---

##### 5. **`EndOfMonth`**

###### Summary
Gets the date of the last day of the current month, preserving the time of the given date.

---

##### 6. **`BeginOfMonth`**

###### Summary
Gets the first day of the current month, preserving the time of the given date.

---

##### 7. **`StartOfYear` / `EndOfYear`**

###### Summary
Gets the first or last day of the current year, preserving the time of the given date.

---

##### 8. **`IsEndOfMonth` / `IsBeginOfMonth` / `IsEndOfYear`**

###### Summary
Checks whether the given date is at the end of the month, the beginning of the month, or the end of the year.

---

##### 9. **`IsSameMonth`**

###### Summary
Checks whether two dates belong to the same month and year.

---

##### 10. **`GetDifferenceInYears`**

###### Summary
Calculates the number of complete calendar years between two dates.

###### Usage
```csharp
DateTime birthDate = new DateTime(1990, 5, 15);
DateTime today = DateTime.Today;
int age = birthDate.GetDifferenceInYears(today);
```

---

#### Interface: `IHolidayCalendar`

The `IHolidayCalendar` interface defines a method to determine if a specific date is a holiday.

#### Example Implementation
```csharp
public class CustomHolidayCalendar : IHolidayCalendar {
  private readonly HashSet<DateTime> holidays = new() { new DateTime(2024, 1, 1) };

  public bool Contains(DateTime date) => holidays.Contains(date.Date);
}
``` 

This class, with its utility methods, simplifies date calculations for workday management, boundaries, and comparisons. It is especially useful for business and scheduling applications.

---

### ExpressionExtensions

The `ExpressionExtensions` class in the `MaksIT.Core.Extensions` namespace provides extension methods for working with LINQ expressions. These methods simplify tasks like combining multiple expressions into a single logical predicate, which is especially useful in building dynamic queries.

---

#### Methods

##### 1. **`CombineWith`**

###### How It Works
1. The method reuses the parameter from the `first` expression.
2. It replaces the parameter in the `second` expression with the same parameter from the `first` expression using the `SubstituteParameterVisitor` class.
3. Combines the bodies of `first` and `second` using `Expression.AndAlso`.

###### Usage
```csharp
Expression<Func<int, bool>> isEven = x => x % 2 == 0;
Expression<Func<int, bool>> isPositive = x => x > 0;

var combinedExpression = isEven.CombineWith(isPositive);

var numbers = new[] { -2, -1, 0, 1, 2, 3, 4 };
var filteredNumbers = numbers.AsQueryable().Where(combinedExpression).ToList();

Console.WriteLine(string.Join(", ", filteredNumbers)); // Output: 2, 4
```

---

#### Notes

- **Use Cases**:
  - Dynamically building LINQ queries with multiple predicates.
  - Simplifying expression tree manipulation in repository patterns or query builders.

- **Performance**:
  - The method uses expression visitors, which is efficient for modifying expression trees without evaluating them.

- **Limitations**:
  - Works only with `Expression<Func<T, bool>>` predicates.
  - Both `first` and `second` must target the same type `T`.

---

This utility makes it easy to compose complex logical expressions dynamically, a common need in advanced querying scenarios like filtering or search functionalities.

---

### FormatsExtensions

The `FormatsExtensions` class in the `MaksIT.Core.Extensions` namespace provides utility methods for working with TAR file creation from directories. These methods simplify the process of compressing directories into TAR files while handling various edge cases, such as invalid paths or empty directories.

---

#### Methods

##### 1. **`TryCreateTarFromDirectory`**

###### Summary
Attempts to create a TAR file from the contents of a specified directory. Returns `true` if the operation succeeds, or `false` if it fails due to invalid input or other errors.

###### Parameters
- `string sourceDirectory`: The path to the source directory to be compressed.
- `string outputTarPath`: The path where the TAR file will be created.

###### Usage

```csharp
string sourceDirectory = @"C:\MyFolder";
string outputTarPath = @"C:\MyFolder.tar";

if (FormatsExtensions.TryCreateTarFromDirectory(sourceDirectory, outputTarPath)) {
  Console.WriteLine("TAR file created successfully.");
}
else {
  Console.WriteLine("Failed to create TAR file.");
}
```

---

#### Features

1. **Error Handling**:
   - Returns `false` for invalid source directories, empty directories, or inaccessible output paths.

2. **Cross-Platform Compatibility**:
   - Designed to work on platforms supported by .NET 8.

3. **Ease of Use**:
   - Simplifies the process of creating TAR files with minimal code.

---

#### Notes

- **Use Cases**:
  - Archiving directories for backup or distribution.
  - Automating file compression tasks in applications.

- **Edge Cases**:
  - If the source directory does not exist, is empty, or the output path is invalid, the method will return `false`.
  - If the output file is locked or cannot be created, the method will also return `false`.

---

#### Example End-to-End Usage

```csharp
// Valid input
string sourceDirectory = @"C:\MyFolder";
string outputTarPath = @"C:\MyFolder.tar";

if (FormatsExtensions.TryCreateTarFromDirectory(sourceDirectory, outputTarPath)) {
  Console.WriteLine("TAR file created successfully.");
}
else {
  Console.WriteLine("Failed to create TAR file.");
}

// Invalid source directory
string invalidSourceDirectory = @"C:\NonExistentFolder";

if (!FormatsExtensions.TryCreateTarFromDirectory(invalidSourceDirectory, outputTarPath)) {
  Console.WriteLine("Source directory does not exist.");
}
```

---

### GuidExtensions

The `GuidExtensions` class in the `MaksIT.Core.Extensions` namespace provides an extension method for converting a `Guid` to a nullable `Guid?`. This is useful in scenarios where the default value of `Guid.Empty` needs to be treated as `null`.

---

#### Methods

##### 1. **`ToNullable`**

###### Summary
Converts a `Guid` to a nullable `Guid?`. If the `Guid` is `Guid.Empty`, the method returns `null`; otherwise, it returns the `Guid`.

###### Usage
```csharp
Guid id1 = Guid.NewGuid();
Guid id2 = Guid.Empty;

Guid? nullableId1 = id1.ToNullable(); // Returns the value of id1
Guid? nullableId2 = id2.ToNullable(); // Returns null

Console.WriteLine(nullableId1); // Outputs the GUID value
Console.WriteLine(nullableId2); // Outputs nothing (null)
```

---

#### Notes

- **Use Cases**:
  - Simplifies handling of `Guid` values in APIs, databases, or validation logic where `Guid.Empty` is treated as `null`.
  - Useful in scenarios where nullable values improve data handling or reduce ambiguity in business logic.

- **Edge Cases**:
  - If the input `Guid` is `Guid.Empty`, this method always returns `null`, regardless of context.
  - Any valid `Guid` other than `Guid.Empty` is returned unchanged.

- **Performance**:
  - This method performs a simple comparison and is highly efficient.

---

This extension adds clarity and reduces boilerplate code when working with `Guid` values, particularly in data validation or transformation workflows.

---

### ObjectExtensions

The `ObjectExtensions` class in the `MaksIT.Core.Extensions` namespace provides extension methods for serializing objects to JSON strings using `System.Text.Json`. These methods allow flexible JSON serialization with optional custom converters.

#### Methods

##### 1. **`ToJson<T>()`**

###### Summary
Converts an object of type `T` to a JSON string using default serialization options.

###### Usage
```csharp
var person = new { Name = "John", Age = 30 };
string json = person.ToJson();
Console.WriteLine(json); // Output: {"name":"John","age":30}

object? nullObject = null;
string nullJson = nullObject.ToJson();
Console.WriteLine(nullJson); // Output: {}
```

---

##### 2. **`ToJson<T>(List<JsonConverter>?)`**

###### Summary
Converts an object of type `T` to a JSON string using default serialization options and additional custom converters.

###### Usage
```csharp
var person = new { Name = "Alice", BirthDate = DateTime.UtcNow };

var converters = new List<JsonConverter> {
    new JsonStringEnumConverter(), // Example custom converter
};

string jsonWithConverters = person.ToJson(converters);
Console.WriteLine(jsonWithConverters);

// Output with converters may vary depending on implementation.
```

---

#### Notes

- **Use Cases**:
  - Simplifies JSON serialization for objects with optional custom converters.
  - Handles `null` objects gracefully by returning `"{}"`.

- **Edge Cases**:
  - When `obj` is `null`, the method always returns `"{}"`, even if custom converters are provided.
  - If `converters` is `null` or empty, serialization uses only the default options.

- **Performance**:
  - Serialization leverages `System.Text.Json`, which is optimized for .NET applications.

---

These extension methods simplify object-to-JSON serialization while offering flexibility with custom serialization options, making them ideal for use in APIs and other data exchange scenarios.

---

### StringExtensions

The `StringExtensions` class in the `MaksIT.Core.Extensions` namespace is an extensive library of utility methods for string manipulation, type conversions, and various transformations. It includes functionality for handling substrings, parsing, validations, and formatting, among other capabilities.

---

#### Methods Overview

##### 1. **Wildcard and Pattern Matching**

###### **`Like`**
Checks if a string matches a pattern with SQL-like wildcard syntax (`*` and `?`).

**Usage**:
```csharp
bool result = "example".Like("exa*e");
```

---

##### 2. **Substring Methods**

###### **`Left`**, **`Right`**, **`Mid`**
Extract portions of a string based on character count or position.

**Usage**:
```csharp
string result = "example".Left(3); // "exa"
```

---

##### 3. **Conversion Methods**

###### **`ToInteger`**, **`ToLong`**, **`ToDecimal`**, etc.
Convert strings to numeric types or nullable equivalents.

**Usage**:
```csharp
int value = "123".ToInteger(); // 123
int? nullableValue = "".ToNullableInt(); // null
```

---

##### 4. **Date Parsing**

###### **`ToDate`**, **`ToDateTime`**
Parse strings into `DateTime` or `DateTime?` with optional formats.

**Usage**:
```csharp
DateTime date = "01/01/2023".ToDate();
DateTime? nullableDate = "".ToNullableDateTime();
```

---

##### 5. **Boolean Conversion**

###### **`ToBool`**, **`ToNullableBool`**
Converts strings like `"true"`, `"yes"`, or `"1"` to `bool`.

**Usage**:
```csharp
bool result = "yes".ToBool(); // true
bool? nullableResult = "".ToNullableBool(); // null
```

---

##### 6. **Enums**

###### **`ToEnum`**, **`ToNullableEnum`**
Parse strings to enums, with support for `[Display(Name = "...")]` attributes.

**Usage**:
```csharp
DayOfWeek day = "Monday".ToEnum<DayOfWeek>();
DayOfWeek? nullableDay = "".ToNullableEnum<DayOfWeek>();
```

---

##### 7. **String Utilities**

###### **`ToTitle`**, **`ToCamelCase`**
Transforms the casing of strings.

**Usage**:
```csharp
string result = "hello world".ToTitle(); // "Hello world"
```

---

##### 8. **CSV Parsing**

###### **`CSVToDataTable`**
Reads a CSV file and converts it into a `DataTable`.

**Usage**:
```csharp
DataTable table = "path/to/file.csv".CSVToDataTable();
```

---

##### 9. **Validation**

###### **`IsInteger`**, **`IsValidEmail`**, **`IsBase32String`**
Checks whether a string matches specific formats or conditions.

**Usage**:
```csharp
bool isEmail = "test@example.com".IsValidEmail(); // true
```

---

##### 10. **HTML Utilities**

###### **`HtmlToPlainText`**
Converts HTML content to plain text by removing tags and entities.

**Usage**:
```csharp
string plainText = "<p>Hello</p>".HtmlToPlainText(); // "Hello"
```

---

##### 11. **URL Extraction**

###### **`ExtractUrls`**
Extracts distinct URLs from a string.

**Usage**:
```csharp
var urls = "Visit http://example.com".ExtractUrls();
```

---

##### 12. **Formatting**

###### **`Format`**
Formats a string with specified arguments.

```csharp
public static string Format(this string s, params object[] args);
```

**Usage**:
```csharp
string formatted = "Hello {0}".Format("World"); // "Hello World"
```

---

##### 13. **Serialization**

###### **`ToObject<T>`**
Deserializes a JSON string into an object of type `T`.

**Usage**:
```csharp
var obj = "{\"Name\":\"John\"}".ToObject<MyClass>();
```

---

##### 14. **Hashing**

###### **`ToGuid`**
Generates a `Guid` from a string using MD5 hashing.

**Usage**:
```csharp
Guid guid = "example".ToGuid();
```

---

##### 15. **String Splitting**

###### **`StringSplit`**
Splits a string by a character and trims each segment.

**Usage**:
```csharp
string[] parts = "a, b, c".StringSplit(',');
```

---

#### Notes

- **Performance**: Methods are optimized for common scenarios.
- **Error Handling**: Includes graceful null handling in many cases.
- **Versatility**: Suitable for APIs, data processing, and utility libraries.

These methods enhance string handling capabilities and simplify common operations.

---

### Logging

The file logging system in the `MaksIT.Core.Logging` namespace provides a custom implementation of the `ILogger` interface. It includes a `FileLogger` to log messages to a file, a `FileLoggerProvider` to create logger instances, and an extension method to integrate the file logger into the `Microsoft.Extensions.Logging` framework.

---

#### Example Usage
```csharp
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddFile("logs.txt"));

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<FileLogger>>();
logger.LogInformation("Logging to file!");
```

---

#### Features

- **Thread Safety**: Ensures thread-safe writes using a lock mechanism.
- **Customizable File Path**: Specify the log file location during configuration.
- **Integration**: Easily integrates with `Microsoft.Extensions.Logging`.
- **Log Levels**: Supports all standard log levels.

---

#### Notes

- **Performance**: The file operations use `File.AppendAllText`, which may have overhead for high-frequency logging. For high-throughput scenarios, consider using a buffered approach.
- **File Management**: Ensure proper file rotation and cleanup to avoid large log files.
- **Scoping**: Scoping is not supported in this implementation.

---

This logging system is ideal for lightweight, file-based logging needs and integrates seamlessly into existing .NET applications using the `Microsoft.Extensions.Logging` framework.

---

## Networking

### NetworkConnection

The `NetworkConnection` class in the `MaksIT.Core.Networking.Windows` namespace provides a Windows-only implementation for managing connections to network shares. It uses the Windows `mpr.dll` to create and manage connections.

---

#### Namespace
`MaksIT.Core.Networking.Windows`

#### Class
`public class NetworkConnection : IDisposable`

---

#### Overview

This class is specifically designed for Windows platforms and allows connecting to and disconnecting from network resources such as shared drives or directories.

---

#### Usage
```csharp
ILogger<NetworkConnection> logger = new LoggerFactory().CreateLogger<NetworkConnection>();
NetworkCredential credentials = new NetworkCredential("username", "password");

if (NetworkConnection.TryCreate(logger, @"\\server\share", credentials, out var connection, out var errorMessage)) {
    Console.WriteLine("Connected successfully.");
    connection.Dispose(); // Always dispose after use.
} else {
    Console.WriteLine($"Failed to connect: {errorMessage}");
}
```

---

#### Features

1. **Windows-Only**:
   - This class is supported only on Windows platforms. A check ensures it is not used on other operating systems.

2. **Thread-Safe**:
   - Proper thread-safety measures are in place for connection and disconnection.

3. **Error Handling**:
   - Detailed error messages are provided for connection failures.

---

#### Notes

- **Performance**:
  - The implementation directly interacts with the Windows API, making it lightweight but platform-dependent.
  
- **Usage**:
  - Always dispose of the connection using `Dispose` to ensure proper resource cleanup.
  
- **Limitations**:
  - The class does not support non-Windows platforms and will throw a `PlatformNotSupportedException` if used elsewhere.

#### Overview

This class is ideal for managing secure connections to shared network resources in Windows environments.

---

### PingPort

The `PingPort` class in the `MaksIT.Core.Networking` namespace provides utility methods to check the reachability of a host on specified TCP or UDP ports. These methods help in network diagnostics by attempting to establish a connection to the target host and port.

---

#### Methods

##### 1. **`TryHostPort`**

###### Summary
Tries to establish a connection to a host on a specified TCP port.

###### Usage
```csharp
if (PingPort.TryHostPort("example.com", 80, out var errorMessage)) {
    Console.WriteLine("TCP port is reachable.");
} else {
    Console.WriteLine($"Failed to reach TCP port: {errorMessage}");
}
```

---

##### 2. **`TryUDPPort`**

###### Summary
Tries to send and receive a message to/from a host on a specified UDP port.

###### Usage
```csharp
if (PingPort.TryUDPPort("example.com", 123, out var errorMessage)) {
    Console.WriteLine("UDP port is reachable.");
} else {
    Console.WriteLine($"Failed to reach UDP port: {errorMessage}");
}
```

---

###### Features

1. **Protocol Support**:
   - `TryHostPort`: For TCP ports.
   - `TryUDPPort`: For UDP ports.
   
2. **Timeout Handling**:
   - `TryHostPort`: Waits for up to 5 seconds for a connection.
   - `TryUDPPort`: Sets a receive timeout of 5 seconds.

3. **Error Reporting**:
   - Returns a clear error message if the operation fails.

4. **Lightweight**:
   - Both methods are simple and do not require additional dependencies.

---

###### Notes

- **Performance**:
  - Designed for quick diagnostics and has a default timeout of 5 seconds to avoid blocking indefinitely.
  
- **Error Handling**:
  - Gracefully handles exceptions and provides detailed error messages for debugging.

- **Limitations**:
  - The reachability of a UDP port depends on whether the target host sends a response to the initial message.
  - Cannot guarantee service availability, only the ability to establish a connection.

---

This class is ideal for quick and lightweight network port checks, particularly in diagnostic or troubleshooting tools.

---

## Security

### AESGCMUtility

The `AESGCMUtility` class in the `MaksIT.Core.Security` namespace provides utility methods for encryption and decryption using AES-GCM (Advanced Encryption Standard in Galois/Counter Mode). It includes functionality to securely encrypt and decrypt data and generate encryption keys.

---

#### Methods

##### 1. **`TryEncryptData`**

###### Summary
Encrypts data using AES-GCM with a base64-encoded key. The encrypted data includes the ciphertext, authentication tag, and IV.

###### Usage
```csharp
var key = AESGCMUtility.GenerateKeyBase64();
var data = Encoding.UTF8.GetBytes("Sensitive data");

if (AESGCMUtility.TryEncryptData(data, key, out var encryptedData, out var error)) {
    Console.WriteLine("Encryption successful.");
} else {
    Console.WriteLine($"Encryption failed: {error}");
}
```

##### 2. **`TryDecryptData`**

###### Summary
Decrypts data encrypted with AES-GCM using a base64-encoded key. The data must include the ciphertext, authentication tag, and IV.

###### Usage
```csharp
if (AESGCMUtility.TryDecryptData(encryptedData, key, out var decryptedData, out var error)) {
    Console.WriteLine($"Decryption successful: {Encoding.UTF8.GetString(decryptedData)}");
} else {
    Console.WriteLine($"Decryption failed: {error}");
}
```


##### 3. **`GenerateKeyBase64`**

###### Summary
Generates a secure, random 256-bit AES key and returns it as a base64-encoded string.

###### Usage
```csharp
var key = AESGCMUtility.GenerateKeyBase64();
Console.WriteLine($"Generated Key: {key}");
```

---

#### Features

1. **Secure Encryption**:
   - Uses AES-GCM, a secure encryption mode with built-in authentication.

2. **Data Integrity**:
   - Includes an authentication tag to ensure the ciphertext has not been tampered with.

3. **Key Generation**:
   - Supports generating random, secure AES keys.

4. **Error Handling**:
   - Provides descriptive error messages for failed encryption or decryption.

5. **Flexible Output**:
   - Encrypted data includes the IV, tag, and ciphertext, ensuring portability and compatibility.

---

#### Notes

1. **Security**:
   - Always store keys securely, e.g., in a hardware security module (HSM) or a secure configuration service.
   - Avoid reusing IVs with the same key, as this compromises security.

2. **Performance**:
   - AES-GCM is optimized for modern hardware and offers efficient encryption and decryption.

3. **Error Handling**:
   - The methods return `false` and provide detailed error messages if any issue occurs.

---

#### Example End-to-End Usage

```csharp
// Generate a key
var key = AESGCMUtility.GenerateKeyBase64();

// Data to encrypt
var plaintext = Encoding.UTF8.GetBytes("Confidential data");

// Encrypt
if (AESGCMUtility.TryEncryptData(plaintext, key, out var encryptedData, out var encryptError)) {
    Console.WriteLine("Encryption successful.");

    // Decrypt
    if (AESGCMUtility.TryDecryptData(encryptedData, key, out var decryptedData, out var decryptError)) {
        Console.WriteLine($"Decryption successful: {Encoding.UTF8.GetString(decryptedData)}");
    } else {
        Console.WriteLine($"Decryption failed: {decryptError}");
    }
} else {
    Console.WriteLine($"Encryption failed: {encryptError}");
}
```

---

This utility simplifies secure encryption and decryption with AES-GCM and is suitable for sensitive data in .NET applications.

---

### Base32Encoder

The `Base32Encoder` class in the `MaksIT.Core.Security` namespace provides methods to encode binary data into Base32 and decode Base32 strings back into binary data. Base32 encoding is commonly used in scenarios where binary data needs to be represented in a textual format.

---

#### Methods

##### 1. **`TryEncode`**

###### Summary
Encodes binary data into a Base32 string.

###### How It Works
1. Converts binary data into Base32 using 5-bit blocks.
2. Adds padding (`=`) to ensure the encoded string length is a multiple of 8.

###### Usage
```csharp
byte[] data = Encoding.UTF8.GetBytes("Hello World");
if (Base32Encoder.TryEncode(data, out var encoded, out var errorMessage)) {
    Console.WriteLine($"Encoded: {encoded}");
} else {
    Console.WriteLine($"Encoding failed: {errorMessage}");
}
```

---

##### 2. **`TryDecode`**

###### Summary
Decodes a Base32 string back into binary data.

###### How It Works
1. Removes any padding characters (`=`) from the input string.
2. Converts the Base32 string back into binary data using 5-bit blocks.

###### Usage
```csharp
string base32 = "JBSWY3DPEBLW64TMMQ======";
if (Base32Encoder.TryDecode(base32, out var decoded, out var errorMessage)) {
    Console.WriteLine($"Decoded: {Encoding.UTF8.GetString(decoded)}");
} else {
    Console.WriteLine($"Decoding failed: {errorMessage}");
}
```

---

#### Features

1. **Encoding and Decoding**:
   - Converts binary data into a Base32 string and vice versa.

2. **Padding**:
   - Ensures Base32-encoded strings are properly padded to be a multiple of 8 characters.

3. **Error Handling**:
   - Provides descriptive error messages if encoding or decoding fails.

4. **Compatibility**:
   - Adheres to standard Base32 encoding, making it compatible with other Base32 implementations.

---

#### Notes

1. **Use Cases**:
   - Suitable for encoding binary data such as keys, tokens, and identifiers in a human-readable format.
   - Commonly used in applications like TOTP (Time-Based One-Time Passwords).

2. **Limitations**:
   - Input strings for decoding must strictly follow the Base32 format.
   - Encoding may include padding characters (`=`), which are not always required in certain applications.

3. **Performance**:
   - Efficient for small to medium-sized data. For large data, consider stream-based approaches.

---

#### Example End-to-End Usage

```csharp
// Original data
byte[] originalData = Encoding.UTF8.GetBytes("Example Data");

// Encode to Base32
if (Base32Encoder.TryEncode(originalData, out var encoded, out var encodeError)) {
    Console.WriteLine($"Encoded: {encoded}");

    // Decode back to binary
    if (Base32Encoder.TryDecode(encoded, out var decodedData, out var decodeError)) {
        Console.WriteLine($"Decoded: {Encoding.UTF8.GetString(decodedData)}");
    } else {
        Console.WriteLine($"Decoding failed: {decodeError}");
    }
} else {
    Console.WriteLine($"Encoding failed: {encodeError}");
}
```

---

This utility provides a robust implementation of Base32 encoding and decoding, making it ideal for use in security and data encoding scenarios.

---


### ChecksumUtility

The `ChecksumUtility` class in the `MaksIT.Core.Security` namespace provides methods to calculate and verify CRC32 checksums for byte arrays and files. This utility is designed for efficient data integrity checks and supports chunk-based processing for large files.

---

#### Methods

##### 1. **`TryCalculateCRC32Checksum`**

###### Summary
Calculates the CRC32 checksum for a given byte array.

###### Usage
```csharp
byte[] data = Encoding.UTF8.GetBytes("Sample data");
if (ChecksumUtility.TryCalculateCRC32Checksum(data, out var checksum, out var errorMessage)) {
    Console.WriteLine($"CRC32 Checksum: {checksum}");
} else {
    Console.WriteLine($"Error: {errorMessage}");
}
```

---

##### 2. **`TryCalculateCRC32ChecksumFromFile`**

###### Summary
Calculates the CRC32 checksum for a file.

###### Usage
```csharp
if (ChecksumUtility.TryCalculateCRC32ChecksumFromFile("sample.txt", out var checksum, out var errorMessage)) {
    Console.WriteLine($"File CRC32 Checksum: {checksum}");
} else {
    Console.WriteLine($"Error: {errorMessage}");
}
```

---

##### 3. **`TryCalculateCRC32ChecksumFromFileInChunks`**

###### Summary
Calculates the CRC32 checksum for a file in chunks, optimizing for large files.

###### Usage
```csharp
if (ChecksumUtility.TryCalculateCRC32ChecksumFromFileInChunks("largefile.txt", out var checksum, out var errorMessage)) {
    Console.WriteLine($"Chunked File CRC32 Checksum: {checksum}");
} else {
    Console.WriteLine($"Error: {errorMessage}");
}
```

---

##### 4. **`VerifyCRC32Checksum`**

###### Summary
Verifies that a byte array matches a given CRC32 checksum.

###### Usage
```csharp
if (ChecksumUtility.VerifyCRC32Checksum(data, "5d41402abc4b2a76b9719d911017c592")) {
    Console.WriteLine("Checksum matches.");
} else {
    Console.WriteLine("Checksum does not match.");
}
```

---

##### 5. **`VerifyCRC32ChecksumFromFile`**

###### Summary
Verifies that a file matches a given CRC32 checksum.

###### Usage
```csharp
if (ChecksumUtility.VerifyCRC32ChecksumFromFile("sample.txt", "5d41402abc4b2a76b9719d911017c592")) {
    Console.WriteLine("File checksum matches.");
} else {
    Console.WriteLine("File checksum does not match.");
}
```

---

##### 6. **`VerifyCRC32ChecksumFromFileInChunks`**

###### Summary
Verifies that a file matches a given CRC32 checksum using chunk-based processing.

###### Usage
```csharp
if (ChecksumUtility.VerifyCRC32ChecksumFromFileInChunks("largefile.txt", "5d41402abc4b2a76b9719d911017c592")) {
    Console.WriteLine("Chunked file checksum matches.");
} else {
    Console.WriteLine("Chunked file checksum does not match.");
}
```

---

#### Features

1. **CRC32 Checksum Calculation**:
   - Supports byte arrays and files.
   - Provides chunk-based processing for large files to optimize memory usage.

2. **Verification**:
   - Verifies whether data or files match a given CRC32 checksum.

3. **Error Handling**:
   - Outputs descriptive error messages for all failures.

4. **Flexible File Handling**:
   - Offers both full-file and chunked approaches for checksum calculations.

---

#### Notes

1. **Use Cases**:
   - Validate data integrity during file transfers or storage.
   - Verify file authenticity by comparing checksums.

2. **Performance**:
   - Chunked methods are recommended for large files to reduce memory consumption.

3. **Limitations**:
   - CRC32 is not suitable for cryptographic purposes due to its vulnerability to collisions.

---

#### Example End-to-End Usage

```csharp
string filePath = "testfile.txt";
string expectedChecksum = "5d41402abc4b2a76b9719d911017c592";

// Calculate checksum
if (ChecksumUtility.TryCalculateCRC32ChecksumFromFile(filePath, out var checksum, out var error)) {
    Console.WriteLine($"Calculated Checksum: {checksum}");

    // Verify checksum
    if (ChecksumUtility.VerifyCRC32ChecksumFromFile(filePath, expectedChecksum)) {
        Console.WriteLine("Checksum verification successful.");
    } else {
        Console.WriteLine("Checksum verification failed.");
    }
} else {
    Console.WriteLine($"Error calculating checksum: {error}");
}
```

---

This utility provides efficient and reliable CRC32 checksum calculation and verification for various data integrity scenarios.

---

### Crc32

The `Crc32` class in the `MaksIT.Core.Security` namespace provides an implementation of the CRC32 (Cyclic Redundancy Check) hash algorithm. This class extends the `HashAlgorithm` base class, enabling usage in cryptographic workflows while also providing utility methods for directly computing CRC32 checksums.

---

#### Overview

The CRC32 algorithm is a commonly used error-detection mechanism for digital data. This class supports the default CRC32 polynomial and seed, with options to specify custom values. It is designed to compute CRC32 checksums for data efficiently and includes both instance-based and static methods.

---

#### Methods

##### 1. **`Initialize`**

Resets the hash computation to the initial state.

##### 2. **`HashCore`**

Processes a segment of data and updates the hash state.

##### 3. **`HashFinal`**

Finalizes the hash computation and returns the CRC32 checksum.

---

##### 4. **Static Methods for Direct CRC32 Computation**

###### **`TryCompute(byte[] buffer, out uint result, out string? errorMessage)`**
Computes the CRC32 checksum for a given byte array using the default polynomial and seed.

---

###### **`TryCompute(uint seed, byte[] buffer, out uint result, out string? errorMessage)`**
Computes the CRC32 checksum for a given byte array using the default polynomial and a custom seed.

```csharp
public static bool TryCompute(uint seed, byte[] buffer, out uint result, out string? errorMessage);
```

---

###### **`TryCompute(uint polynomial, uint seed, byte[] buffer, out uint result, out string? errorMessage)`**
Computes the CRC32 checksum for a given byte array using a custom polynomial and seed.

---

#### Example Usage

##### Instance-Based Hashing
```csharp
byte[] data = Encoding.UTF8.GetBytes("Example data");
using var crc32 = new Crc32();
byte[] hash = crc32.ComputeHash(data);
Console.WriteLine($"CRC32 Hash: {BitConverter.ToString(hash).Replace("-", "").ToLower()}");
```

##### Static Method for Direct Calculation
```csharp
byte[] data = Encoding.UTF8.GetBytes("Example data");
if (Crc32.TryCompute(data, out var result, out var errorMessage)) {
    Console.WriteLine($"CRC32 Checksum: {result:X}");
} else {
    Console.WriteLine($"Error: {errorMessage}");
}
```

##### Using Custom Polynomial and Seed
```csharp
byte[] data = Encoding.UTF8.GetBytes("Custom polynomial example");
uint polynomial = 0x04C11DB7; // Example polynomial
uint seed = 0xFFFFFFFF;

if (Crc32.TryCompute(polynomial, seed, data, out var result, out var errorMessage)) {
    Console.WriteLine($"Custom CRC32 Checksum: {result:X}");
} else {
    Console.WriteLine($"Error: {errorMessage}");
}
```

---

#### Features

1. **Customizable**:
   - Supports custom polynomials and seeds for flexibility.

2. **Integration**:
   - Extends `HashAlgorithm`, making it compatible with cryptographic workflows in .NET.

3. **Static Utility**:
   - Includes static methods for direct computation without creating an instance.

4. **Efficiency**:
   - Uses a precomputed lookup table for fast CRC32 calculation.

5. **Error Handling**:
   - Provides `TryCompute` methods with detailed error messages.

---

#### Notes

- **Use Cases**:
  - Commonly used for data integrity checks in file transfers and storage.
  - Can be used in networking protocols for error detection.

- **Performance**:
  - Optimized for performance using a lookup table.

- **Limitations**:
  - CRC32 is not suitable for cryptographic purposes due to its vulnerability to collisions.

---

This implementation provides a robust and efficient solution for CRC32 checksum computation, suitable for data integrity and verification workflows.

---

### JwtGenerator

The `JwtGenerator` class in the `MaksIT.Core.Security` namespace provides methods for generating, validating, and managing JSON Web Tokens (JWTs). It supports creating tokens with claims, validating tokens, and generating secure secrets and refresh tokens.

---

#### Purpose
Represents the claims of a JWT, including metadata such as username, roles, and token issuance and expiration times.

---

#### Methods

#### 1. **`TryGenerateToken`**

###### Summary
Generates a JWT with the specified claims and metadata.

###### Usage
```csharp
if (JwtGenerator.TryGenerateToken("mysecret", "myissuer", "myaudience", 60, "user123", new List<string> { "Admin" }, out var tokenData, out var errorMessage)) {
    Console.WriteLine($"Token: {tokenData?.Item1}");
    Console.WriteLine($"Expires At: {tokenData?.Item2.ExpiresAt}");
} else {
    Console.WriteLine($"Error: {errorMessage}");
}
```

---

#### 2. **`GenerateSecret`**

###### Summary
Generates a secure, random secret key for JWT signing.

###### Usage
```csharp
string secret = JwtGenerator.GenerateSecret();
Console.WriteLine($"Generated Secret: {secret}");
```

---

#### 3. **`TryValidateToken`**

###### Summary
Validates a JWT against the provided secret, issuer, and audience.

###### Usage
```csharp
if (JwtGenerator.TryValidateToken("mysecret", "myissuer", "myaudience", "jwtTokenHere", out var claims, out var errorMessage)) {
    Console.WriteLine($"Token is valid for user: {claims?.Username}");
} else {
    Console.WriteLine($"Token validation failed: {errorMessage}");
}
```

---

#### 4. **`GenerateRefreshToken`**

###### Summary
Generates a secure, random refresh token.

###### Usage
```csharp
string refreshToken = JwtGenerator.GenerateRefreshToken();
Console.WriteLine($"Refresh Token: {refreshToken}");
```

---

#### Features

1. **Token Generation**:
   - Create JWTs with configurable claims and expiration times.
   
2. **Token Validation**:
   - Validate JWTs against the expected issuer, audience, and secret.

3. **Security**:
   - Uses HMAC-SHA256 for signing.
   - Supports generating secure secrets and refresh tokens.

4. **Claims Management**:
   - Embed and extract custom claims such as roles and metadata.

---

#### Example End-to-End Usage

```csharp
string secret = JwtGenerator.GenerateSecret();
string issuer = "myissuer";
string audience = "myaudience";

// Generate a token
if (JwtGenerator.TryGenerateToken(secret, issuer, audience, 60, "user123", new List<string> { "Admin" }, out var tokenData, out var errorMessage)) {
    Console.WriteLine($"Generated Token: {tokenData?.Item1}");

    // Validate the token
    if (JwtGenerator.TryValidateToken(secret, issuer, audience, tokenData?.Item1, out var claims, out var validationError)) {
        Console.WriteLine($"Token valid for user: {claims?.Username}");
    } else {
        Console.WriteLine($"Validation failed: {validationError}");
    }
} else {
    Console.WriteLine($"Token generation failed: {errorMessage}");
}
```

---

#### Notes

- **Use Cases**:
  - API authentication and authorization.
  - Session management with short-lived tokens and refresh tokens.

- **Best Practices**:
  - Store secrets securely, e.g., in a configuration service or environment variables.
  - Use HTTPS to protect tokens in transit.

- **Limitations**:
  - Ensure token expiration times are appropriate for your security model.

---

This class provides a comprehensive implementation for secure JWT generation and validation, suitable for modern .NET applications.

---

### PasswordHasher

The `PasswordHasher` class in the `MaksIT.Core.Security` namespace provides functionality for securely hashing passwords with salt and validating hashed passwords using the PBKDF2 (Password-Based Key Derivation Function 2) algorithm with HMAC-SHA512.

---

#### Overview

The `PasswordHasher` class includes methods for:
- Generating a secure, salted hash for a password.
- Validating a password against a stored salted hash.
- Ensuring security using best practices, such as high iteration counts and fixed-time comparison.

---

#### Methods

##### 1. **`TryCreateSaltedHash`**

###### Summary
Creates a salted hash for the given password.

###### Usage
```csharp
if (PasswordHasher.TryCreateSaltedHash("mypassword", out var saltedHash, out var errorMessage)) {
    Console.WriteLine($"Salt: {saltedHash?.Salt}");
    Console.WriteLine($"Hash: {saltedHash?.Hash}");
} else {
    Console.WriteLine($"Error: {errorMessage}");
}
```

---

##### 2. **`TryValidateHash`**

###### Summary
Validates a password against a stored hash and salt.

###### Usage
```csharp
if (PasswordHasher.TryValidateHash("mypassword", saltedHash.Salt, saltedHash.Hash, out var isValid, out var errorMessage)) {
    Console.WriteLine(isValid ? "Password is valid." : "Password is invalid.");
} else {
    Console.WriteLine($"Error: {errorMessage}");
}
```

---

#### Features

1. **Salted Hashing**:
   - Ensures uniqueness for each hash by combining a password with a unique salt.

2. **Secure Validation**:
   - Uses `CryptographicOperations.FixedTimeEquals` for constant-time comparison, preventing timing attacks.

3. **High Security**:
   - Implements PBKDF2 with HMAC-SHA512.
   - Uses 100,000 iterations for strong resistance against brute force attacks.

4. **Error Handling**:
   - Provides detailed error messages for any failure during hashing or validation.

---

#### Notes

- **Performance**:
  - PBKDF2 with 100,000 iterations ensures a good balance between security and performance. The iteration count can be adjusted for specific security requirements.
  
- **Use Cases**:
  - Ideal for securely storing and validating user passwords in web applications and APIs.

- **Limitations**:
  - This implementation does not include password policies (e.g., minimum length, complexity), which should be enforced separately.

---

#### Example End-to-End Usage

```csharp
// Step 1: Hash a password
if (PasswordHasher.TryCreateSaltedHash("securepassword", out var saltedHash, out var errorMessage)) {
    Console.WriteLine($"Salt: {saltedHash?.Salt}");
    Console.WriteLine($"Hash: {saltedHash?.Hash}");

    // Step 2: Validate the password
    if (PasswordHasher.TryValidateHash("securepassword", saltedHash.Salt, saltedHash.Hash, out var isValid, out var validationError)) {
        Console.WriteLine(isValid ? "Password is valid." : "Password is invalid.");
    } else {
        Console.WriteLine($"Validation Error: {validationError}");
    }
} else {
    Console.WriteLine($"Hashing Error: {errorMessage}");
}
```

---

#### Best Practices

- **Key Stretching**:
  - Use high iteration counts (e.g., 100,000 or more) to make brute force attacks computationally expensive.

- **Salt Storage**:
  - Store salts separately or alongside the hash in the database.

- **Encryption**:
  - Use a secure channel (e.g., HTTPS) when transmitting passwords to prevent interception.

---

The `PasswordHasher` class provides a secure and efficient implementation for password hashing and validation, adhering to modern security standards.

---

### TotpGenerator

The `TotpGenerator` class in the `MaksIT.Core.Security` namespace provides methods for generating and validating Time-Based One-Time Passwords (TOTP) as per the TOTP standard (RFC 6238). It also includes utility methods for generating shared secrets, recovery codes, and TOTP authentication links.

---

#### Overview

This class supports:
- TOTP generation and validation.
- Generation of base32-encoded shared secrets for TOTP.
- Recovery code generation.
- TOTP authentication link generation for integration with authenticator apps.

---

#### Methods

##### 1. **`TryValidate`**

##### Summary
Validates a given TOTP code against a shared secret with time tolerance.

##### Usage
```csharp
if (TotpGenerator.TryValidate("123456", "MZXW6YTBOI======", 1, out var isValid, out var errorMessage)) {
    Console.WriteLine(isValid ? "TOTP is valid." : "TOTP is invalid.");
} else {
    Console.WriteLine($"Error: {errorMessage}");
}
```

---

##### 2. **`TryGenerate`**

##### Summary
Generates a TOTP code for a given shared secret and time step.

##### Usage
```csharp
if (TotpGenerator.TryGenerate("MZXW6YTBOI======", TotpGenerator.GetCurrentTimeStepNumber(), out var totpCode, out var errorMessage)) {
    Console.WriteLine($"Generated TOTP: {totpCode}");
} else {
    Console.WriteLine($"Error: {errorMessage}");
}
```

---

##### 3. **`GetCurrentTimeStepNumber`**

##### Summary
Calculates the current time step number based on the current Unix timestamp.

##### Usage
```csharp
long timestep = TotpGenerator.GetCurrentTimeStepNumber();
Console.WriteLine($"Current Time Step: {timestep}");
```

---

##### 4. **`TryGenerateSecret`**

##### Summary
Generates a random shared secret encoded in base32.

##### Usage
```csharp
if (TotpGenerator.TryGenerateSecret(out var secret, out var errorMessage)) {
    Console.WriteLine($"Generated Secret: {secret}");
} else {
    Console.WriteLine($"Error: {errorMessage}");
}
```

---

##### 5. **`TryGenerateRecoveryCodes`**

##### Summary
Generates recovery codes for account recovery in case of TOTP unavailability.

##### Usage
```csharp
if (TotpGenerator.TryGenerateRecoveryCodes(5, out var recoveryCodes, out var errorMessage)) {
    Console.WriteLine("Recovery Codes:");
    recoveryCodes?.ForEach(Console.WriteLine);
} else {
    Console.WriteLine($"Error: {errorMessage}");
}
```

---

##### 6. **`TryGenerateTotpAuthLink`**

##### Summary
Generates an OTPAuth link for use with authenticator apps.

##### Usage
```csharp
if (TotpGenerator.TryGenerateTotpAuthLink("MyApp", "user@example.com", "MZXW6YTBOI======", "MyIssuer", "SHA1", 6, 30, out var authLink, out var errorMessage)) {
    Console.WriteLine($"Auth Link: {authLink}");
} else {
    Console.WriteLine($"Error: {errorMessage}");
}
```

---

#### Features

1. **TOTP Support**:
   - Generate and validate TOTPs with configurable tolerance.

2. **Shared Secrets**:
   - Generate base32-encoded secrets for use in TOTP.

3. **Recovery Codes**:
   - Generate recovery codes for account recovery scenarios.

4. **Integration**:
   - Create OTPAuth links for easy integration with authenticator apps.

---

#### Notes

- **Use Cases**:
  - Two-factor authentication (2FA) for applications.
  - Secure account recovery workflows.

- **Security**:
  - Use HTTPS for secure transmission of secrets and TOTPs.
  - Store shared secrets securely in a trusted configuration or key vault.

---

This class provides a robust implementation for managing TOTP workflows and integrating with authenticator apps.

---

## Web API Models

This documentation provides an overview of classes in the `MaksIT.Core.Webapi.Models` namespace, which are designed to facilitate Web API request and response handling. These models include functionality for pagination, filtering, patching operations, and dynamic LINQ expressions.

---

### Classes and Enums

#### **1. `PagedRequest`**

#### Summary
Represents a request model for paginated data retrieval, with support for filtering, sorting, and pagination.

#### Usage
```csharp
var request = new PagedRequest {
    PageNumber = 1,
    PageSize = 10,
    Filters = "Name='John' && Age>30"
};

var filterExpression = request.BuildFilterExpression<User>("Name='John' && Age>30");
```

---

#### **2. `PagedResponse<T>`**

##### Summary
Represents a response model for paginated data, including metadata about pagination and the data itself.

##### Usage
```csharp
var response = new PagedResponse<User>(users, totalCount: 100, pageNumber: 1, pageSize: 10);
```

---

#### **3. `PatchOperation`**

##### Summary
Enumerates the types of patch operations that can be performed on a field or collection.

##### Usage
```csharp
var operation = PatchOperation.Replace;
```

---

### Features and Benefits

1. **Pagination**:
   - Simplifies paginated data handling with consistent request and response models.

2. **Dynamic Filtering**:
   - Supports dynamic LINQ expressions for filtering datasets.

3. **Partial Updates**:
   - Facilitates patch operations for efficient updates without replacing entire objects.

4. **Extensibility**:
   - Designed as base models, allowing customization and extension.

---

### Example End-to-End Usage

#### Paginated Data Retrieval
```csharp
var request = new PagedRequest {
    PageNumber = 1,
    PageSize = 10,
    Filters = "Age>25 && IsActive=true"
};

// Build a LINQ expression
var filterExpression = request.BuildFilterExpression<User>("Age>25");

// Generate a paginated response
var pagedResponse = new PagedResponse<User>(users, totalCount: 100, pageNumber: 1, pageSize: 10);
Console.WriteLine($"Total Pages: {pagedResponse.TotalPages}");
```

### Partial Updates Using PatchField
```csharp
var patch = new SomePatchRequestModel {
    Username = "Updated Name"

    Operations = new Dictionary<string, PatchOperation> {
        { "Username", PartchOperation.SetField }
    }
};

// Extract operation per field
var usernmae = patch.Username;
if (TryGetOperation(nameOf(patch.Username), out operation)) {
  Console.WriteLine($"Operation: {operation}, Value: {value}");
}


```

---

#### Best Practices

1. **Validation**:
   - Validate `PagedRequest` properties to ensure proper filtering and sorting inputs.

2. **Error Handling**:
   - Handle edge cases, such as invalid filters or unsupported patch operations.

3. **Custom Extensions**:
   - Extend `PagedRequest` and `PagedResponse` to include additional metadata or logic as needed.

---

This set of classes provides a robust framework for managing Web API requests and responses, focusing on dynamic query capabilities, efficient data handling, and flexible update mechanisms.

---

## Others

### Culture

The `Culture` class in the `MaksIT.Core` namespace provides a utility method for managing and setting the culture for the current thread. This is particularly useful for applications that need to support multiple cultures or require culture-specific behavior.

---

#### Overview

The `Culture` class simplifies culture management for the current thread by allowing the culture and UI culture to be set dynamically. It ensures thread safety and uses the invariant culture as a fallback when no specific culture is provided.

---

#### Methods

##### **`TrySet`**

###### Summary
Sets the culture and UI culture for the current thread.

---

###### Usage
```csharp
if (Culture.TrySet("fr-FR", out var errorMessage)) {
    Console.WriteLine("Culture set to French (France).");
} else {
    Console.WriteLine($"Failed to set culture: {errorMessage}");
}

// Set to the invariant culture
if (Culture.TrySet(null, out errorMessage)) {
    Console.WriteLine("Culture set to invariant culture.");
} else {
    Console.WriteLine($"Failed to set culture: {errorMessage}");
}
```

---

#### Features

1. **Dynamic Culture Setting**:
   - Allows changing the culture for the current thread dynamically.

2. **Fallback to Invariant Culture**:
   - Automatically uses the invariant culture when no specific culture is provided.

3. **Error Handling**:
   - Provides detailed error messages if the culture setting fails.

4. **Thread Safety**:
   - Ensures that the culture changes only affect the current thread.

---

#### Notes

- **Use Cases**:
  - Localizing applications for different regions and languages.
  - Ensuring consistent culture settings for thread-specific operations, such as formatting or parsing dates and numbers.

- **Error Handling**:
  - If an invalid culture string is provided, an exception will be caught, and the error message will describe the issue.

- **Limitations**:
  - This class only affects the current thread. For global culture settings, consider setting the culture for the entire application domain.

---

#### Example End-to-End Usage

```csharp
// Set culture to French (France)
if (Culture.TrySet("fr-FR", out var errorMessage)) {
    Console.WriteLine("Culture set to French (France).");
    Console.WriteLine($"CurrentCulture: {Thread.CurrentThread.CurrentCulture}");
    Console.WriteLine($"CurrentUICulture: {Thread.CurrentThread.CurrentUICulture}");
} else {
    Console.WriteLine($"Error: {errorMessage}");
}

// Attempt to set an invalid culture
if (!Culture.TrySet("invalid-culture", out errorMessage)) {
    Console.WriteLine($"Expected failure: {errorMessage}");
}

// Reset to invariant culture
if (Culture.TrySet(null, out errorMessage)) {
    Console.WriteLine("Culture reset to invariant culture.");
    Console.WriteLine($"CurrentCulture: {Thread.CurrentThread.CurrentCulture}");
}
```

---

#### Best Practices

- Always validate input culture strings to ensure they conform to supported culture formats.
- Use this utility method in localized applications where culture settings need to be adjusted dynamically for individual threads.

---

This class provides a simple and effective way to manage culture settings for thread-specific operations, making it a valuable tool for applications that require localization or dynamic culture management.



### EnvVar

The `EnvVar` class in the `MaksIT.Core` namespace provides utilities for managing environment variables, including adding paths to the `PATH` environment variable and setting or unsetting environment variables at different scopes.

---

#### Overview

The `EnvVar` class supports:
- Adding a path to the `PATH` environment variable.
- Setting and unsetting environment variables for `Process`, `User`, or `Machine` scopes.
- Cross-platform compatibility with appropriate checks for platform-specific operations.

---

#### Methods

##### 1. **`TryAddToPath`**

###### Summary
Adds a new path to the `PATH` environment variable if it is not already present.

###### Usage
```csharp
if (EnvVar.TryAddToPath("/usr/local/bin", out var errorMessage)) {
    Console.WriteLine("Path added successfully.");
} else {
    Console.WriteLine($"Failed to add path: {errorMessage}");
}
```

---

##### 2. **`TrySet`**

###### Summary
Sets an environment variable at a specified scope.

###### Usage
```csharp
if (EnvVar.TrySet("MY_ENV_VAR", "my_value", "user", out var errorMessage)) {
    Console.WriteLine("Environment variable set successfully.");
} else {
    Console.WriteLine($"Failed to set environment variable: {errorMessage}");
}
```

---

##### 3. **`TryUnSet`**

###### Summary
Unsets (removes) an environment variable at a specified scope.

###### Usage
```csharp
if (EnvVar.TryUnSet("MY_ENV_VAR", "user", out var errorMessage)) {
    Console.WriteLine("Environment variable unset successfully.");
} else {
    Console.WriteLine($"Failed to unset environment variable: {errorMessage}");
}
```

---

#### Features

1. **Environment Variable Management**:
   - Supports setting, unsetting, and updating environment variables dynamically.

2. **Platform Awareness**:
   - Handles platform-specific differences (e.g., path separators and machine-level variables on Windows only).

3. **Error Handling**:
   - Provides descriptive error messages for failed operations.

4. **Cross-Scope Support**:
   - Operates on `Process`, `User`, or `Machine` scopes, depending on requirements.

---

#### Notes

- **Platform-Specific Operations**:
  - Machine-level operations are only supported on Windows.
  - Path separators (`;` or `:`) are handled automatically based on the operating system.

- **Thread-Safety**:
  - Changes to environment variables affect the entire process and may impact other threads.

- **Use Cases**:
  - Dynamically configuring environment variables for applications.
  - Modifying the `PATH` variable for runtime dependencies.

---

#### Example End-to-End Usage

```csharp
// Add a new path to PATH
if (EnvVar.TryAddToPath("/usr/local/bin", out var pathError)) {
    Console.WriteLine("Path added to PATH successfully.");
} else {
    Console.WriteLine($"Failed to add path: {pathError}");
}

// Set an environment variable
if (EnvVar.TrySet("MY_ENV_VAR", "value123", "user", out var setError)) {
    Console.WriteLine("Environment variable set successfully.");
} else {
    Console.WriteLine($"Failed to set variable: {setError}");
}

// Unset an environment variable
if (EnvVar.TryUnSet("MY_ENV_VAR", "user", out var unsetError)) {
    Console.WriteLine("Environment variable unset successfully.");
} else {
    Console.WriteLine($"Failed to unset variable: {unsetError}");
}
```

---

#### Best Practices

- Use **process-level** variables for temporary configurations that do not require persistence.
- Use **user-level** variables for configurations specific to a user account.
- Avoid modifying **machine-level** variables unless necessary, and always test the impact on the system.

---

This class provides a convenient and platform-aware way to manage environment variables, making it suitable for dynamic runtime configurations.

---

### FileSystem

The `FileSystem` class in the `MaksIT.Core` namespace provides utility methods for working with the file system, including file and folder operations, resolving wildcard paths, and handling duplicate file names.

---

#### Overview

The `FileSystem` class supports:
- Copying files and folders to a target directory.
- Deleting files and directories.
- Resolving paths with wildcards.
- Handling file name duplication by appending a counter.
- Ensures cross-platform compatibility with platform-specific handling for Windows and other OSes.

---

#### Methods

#### 1. **`TryCopyToFolder`**

##### Summary
Copies a file or the contents of a folder to a specified destination folder.

##### Usage
```csharp
if (FileSystem.TryCopyToFolder("sourcePath", "destinationPath", true, out var errorMessage)) {
    Console.WriteLine("Copy operation successful.");
} else {
    Console.WriteLine($"Copy operation failed: {errorMessage}");
}
```

---

#### 2. **`TryDeleteFileOrDirectory`**

##### Summary
Deletes a file or directory at the specified path.

##### Usage
```csharp
if (FileSystem.TryDeleteFileOrDirectory("pathToItem", out var errorMessage)) {
    Console.WriteLine("Delete operation successful.");
} else {
    Console.WriteLine($"Delete operation failed: {errorMessage}");
}
```

---

#### 3. **`ResolveWildcardedPath`**

##### Summary
Resolves paths with wildcards and returns all matching paths.

##### Usage
```csharp
var resolvedPaths = FileSystem.ResolveWildcardedPath("?:\\Users\\*\\AppData\\Roaming\\*");
resolvedPaths.ForEach(Console.WriteLine);
```

---

#### 4. **`DuplicateFileNameCheck`**

##### Summary
Checks for file name duplication and appends a counter if a duplicate is found.

##### Usage
```csharp
var uniqueFilePath = FileSystem.DuplicateFileNameCheck("path/to/file.txt");
Console.WriteLine($"Unique file path: {uniqueFilePath}");
```

---

#### Features

1. **File and Folder Operations**:
   - Copy files or entire folders with an option to overwrite existing files.
   - Delete files and directories, including recursive deletion for directories.

2. **Wildcard Path Resolution**:
   - Resolve and return all matching paths for wildcard-based patterns.

3. **Duplicate File Name Handling**:
   - Automatically appends a counter to duplicate file names to ensure uniqueness.

4. **Cross-Platform Support**:
   - Handles platform-specific path separators and behaviors.

---

#### Notes

- **Platform Awareness**:
  - Uses `RuntimeInformation` to differentiate between Windows and non-Windows environments for platform-specific operations.

- **Error Handling**:
  - All methods provide detailed error messages for failed operations.

- **Use Cases**:
  - Managing file and folder content dynamically.
  - Handling file system paths with wildcard patterns.
  - Avoiding file name collisions during file operations.

---

#### Example End-to-End Usage

```csharp
// Example 1: Copy a folder
if (FileSystem.TryCopyToFolder("sourceFolderPath", "destinationFolderPath", true, out var copyError)) {
    Console.WriteLine("Folder copied successfully.");
} else {
    Console.WriteLine($"Error copying folder: {copyError}");
}

// Example 2: Delete a file or folder
if (FileSystem.TryDeleteFileOrDirectory("pathToDelete", out var deleteError)) {
    Console.WriteLine("Item deleted successfully.");
} else {
    Console.WriteLine($"Error deleting item: {deleteError}");
}

// Example 3: Resolve wildcard paths
var paths = FileSystem.ResolveWildcardedPath("?:\\Users\\*\\Documents\\*");
paths.ForEach(Console.WriteLine);

// Example 4: Handle duplicate file names
var uniqueFile = FileSystem.DuplicateFileNameCheck("path/to/myFile.txt");
Console.WriteLine($"Unique file path: {uniqueFile}");
```

---

#### Best Practices

- Ensure proper permissions for file and directory operations.
- Handle exceptions for inaccessible paths when using wildcard resolutions.
- Use `DuplicateFileNameCheck` to prevent overwriting existing files unintentionally.

---

The `FileSystem` class provides a comprehensive set of utilities for dynamic file system operations, making it ideal for applications requiring flexible and robust file management capabilities.


### Processes

The `Processes` class in the `MaksIT.Core` namespace provides helper methods to manage system processes, allowing you to start new processes or kill existing ones by name.

---

#### Overview

The `Processes` class supports:
- Starting new processes with optional arguments and timeout handling.
- Killing processes by name, with support for wildcard matching (`*` and `?`).

#### Key Features:
- **Start Process**: Launch a process with customizable settings.
- **Kill Process**: Terminate processes by name, including wildcard support.

---

#### Methods

##### 1. **`TryStart`**

###### Summary
Starts a new process with specified arguments and settings.

###### Usage
```csharp
if (Processes.TryStart("notepad.exe", "", 0, false, out var errorMessage)) {
    Console.WriteLine("Process started successfully.");
} else {
    Console.WriteLine($"Failed to start process: {errorMessage}");
}
```

---

##### 2. **`TryKill`**

###### Summary
Terminates processes matching the specified name.

###### Usage
```csharp
if (Processes.TryKill("notepad*", out var errorMessage)) {
    Console.WriteLine("Processes killed successfully.");
} else {
    Console.WriteLine($"Failed to kill processes: {errorMessage}");
}
```

---

#### Features

1. **Process Management**:
   - Start new processes with optional timeout and window visibility settings.
   - Kill processes by name, with wildcard matching for flexible targeting.

2. **Error Handling**:
   - Provides detailed error messages for any failed operations.

3. **Wildcard Support**:
   - Matches process names using patterns (`*` and `?`) to handle dynamic process names.

4. **Customizable Execution**:
   - Configure process settings such as visibility (`silent`) and wait time (`timeout`).

---

#### Notes

- **Thread-Safety**:
  - Process management may affect other threads in the application.

- **Platform-Specific Behavior**:
  - Works on all platforms supported by .NET, but behavior may vary depending on the operating system.

- **Use Cases**:
  - Automating the launch of system tools or scripts.
  - Cleaning up orphaned or unnecessary processes in an automated environment.

---

#### Example End-to-End Usage

```csharp
// Example 1: Start a process
if (Processes.TryStart("notepad.exe", "", 10, false, out var startError)) {
    Console.WriteLine("Notepad started successfully.");
} else {
    Console.WriteLine($"Error starting Notepad: {startError}");
}

// Example 2: Kill processes matching a pattern
if (Processes.TryKill("notepad*", out var killError)) {
    Console.WriteLine("Notepad processes killed successfully.");
} else {
    Console.WriteLine($"Error killing Notepad processes: {killError}");
}
```

---

#### Best Practices

1. **Error Handling**:
   - Always check the return value and error messages to handle exceptions gracefully.

2. **Timeout Configuration**:
   - Use a reasonable timeout when waiting for a process to exit to avoid indefinite blocking.

3. **Wildcard Patterns**:
   - Use precise patterns to avoid unintended process termination when using `TryKill`.

4. **Permission Handling**:
   - Ensure the application has appropriate permissions to start or terminate processes.

---

The `Processes` class provides a robust utility for managing processes in .NET applications, suitable for both user-level and system-level automation tasks.

---