# MaksIT.Core Library Documentation

![Line Coverage](https://img.shields.io/badge/Line%20Coverage-38.5%25-yellow)
![Branch Coverage](https://img.shields.io/badge/Branch%20Coverage-28.4%25-yellow)
![Method Coverage](https://img.shields.io/badge/Method%20Coverage-37.9%25-yellow)

**MaksIT.Core** is a .NET 10 library of shared helpers used across MaksIT products: domain/DTO/Web API bases, strongly-typed enumerations, extensions, logging, security (JWT, JWK, JWS, TOTP, AES-GCM), sagas, COMB GUIDs, and Web API pagination.

Install from NuGet:

```bash
dotnet add package MaksIT.Core
```

The secrets CLI (`MaksIT.Core.Cli`) is **not** published to NuGet. It ships in the GitHub release zip next to `MaksIT.Core.*.nupkg`.

| | |
|--|--|
| Tests / coverage badges | `utils\Invoke-TestEngine.bat` |
| Release (pack, NuGet, GitHub) | `utils\Invoke-ReleasePackage.bat` |
| Contributing | [CONTRIBUTING.md](CONTRIBUTING.md) |
| Changelog | [CHANGELOG.md](CHANGELOG.md) |
| License | [LICENSE.md](LICENSE.md) |

## Table of Contents

- [CLI (secrets toolkit)](#cli-secrets-toolkit)
- [Abstractions](#abstractions)
  - [Base Classes](#base-classes)
  - [Enumeration](#enumeration)
- [Extensions](#extensions)
  - [Expression Extensions](#expression-extensions)
  - [DateTime Extensions](#datetime-extensions)
  - [String Extensions](#string-extensions)
  - [Object Extensions](#object-extensions)
  - [Exception Extensions](#exception-extensions)
  - [Formats Extensions](#formats-extensions)
  - [DataTable Extensions](#datatable-extensions)
  - [Guid Extensions](#guid-extensions)
  - [Enum Extensions](#enum-extensions)
- [Logging](#logging)
  - [File Logger](#file-logger)
  - [JSON File Logger](#json-file-logger)
  - [Console Loggers](#console-loggers)
  - [Logger Prefix](#logger-prefix)
- [Threading](#threading)
  - [Lock Manager](#lock-manager)
- [Networking](#networking)
  - [Network Connection](#network-connection)
  - [Ping Port](#ping-port)
- [Security](#security)
  - [AES-GCM Utility](#aes-gcm-utility)
  - [Base32 Encoder](#base32-encoder)
  - [Base64Url Utility](#base64url-utility)
  - [Checksum Utility](#checksum-utility)
  - [Password Hasher](#password-hasher)
  - [JWT Generator](#jwt-generator)
  - [JWK Generator](#jwk-generator)
  - [JWS Generator](#jws-generator)
  - [JWK Thumbprint Utility](#jwk-thumbprint-utility)
  - [TOTP Generator](#totp-generator)
- [Web API](#web-api)
  - [Paged Request](#paged-request)
  - [Paged Response](#paged-response)
  - [Patch Operation](#patch-operation)
  - [Error Handling Middleware](#error-handling-middleware)
  - [Trace ID Logging Scope Middleware](#trace-id-logging-scope-middleware)
- [Sagas](#sagas)
- [CombGuidGenerator](#combguidgenerator)
- [Others](#others)
  - [Culture](#culture)
  - [Environment Variables](#environment-variables)
  - [File System](#file-system)
  - [Processes](#processes)

## CLI (secrets toolkit)

Generates the same secrets the library uses at runtime (JWT signing keys, password pepper, AES-256 keys, TOTP material, COMB GUIDs). It is **not** published to NuGet; the exe ships in the GitHub release zip next to `MaksIT.Core.*.nupkg`.

- **No arguments** — interactive numbered menu.
- **With commands** — non-interactive flags for scripts and agents. Values go to stdout; errors to stderr; exit `0`/`1`.

### Run

```bash
cd src
dotnet run --project MaksIT.Core.Cli
dotnet run --project MaksIT.Core.Cli -- --help
dotnet run --project MaksIT.Core.Cli -- secret
```

From an unpacked release zip:

```bash
MaksIT.Core.Cli/MaksIT.Core.Cli
MaksIT.Core.Cli/MaksIT.Core.Cli secret --bytes 32
```

### Agent commands

| Command | Output |
|---------|--------|
| `secret [--bytes 32]` | Base64 secret (JWT signing / pepper) |
| `jwt secret [--bytes 32]` | Same as `secret` |
| `jwt refresh` | Opaque refresh token |
| `jwt generate --secret S --issuer I --audience A [--expiration 60] [--user-id] [--username] [--roles] [--acl]` | Access JWT |
| `jwt validate --secret S --issuer I --audience A --token T` | Claims JSON |
| `aes key` | Base64 AES-256 key |
| `totp secret` | Base32 TOTP secret |
| `totp recovery [--count 10]` | Recovery codes (one per line) |
| `totp link --label L --username U --secret S --issuer I` | `otpauth://` URI |
| `totp validate --secret S --code C [--tolerance 1]` | `valid` / `invalid` (exit 1 if invalid) |
| `password hash --pepper P --password PWD` | JSON `{ salt, hash }` |
| `guid comb [--type PostgreSql]` | COMB GUID (`SqlServer` also accepted) |

Typical `appsecrets.json` values:

| Menu / command | Writes |
|----------------|--------|
| Generate secret / `secret` | `JwtSettings` signing secret or `PasswordPepper` |
| AES-GCM key / `aes key` | host encryption key |
| TOTP / 2FA / `totp secret` | authenticator shared key / recovery codes |
| JWT generate | debug access tokens against a known secret |

## Abstractions

### Base Classes

The following base classes in the `MaksIT.Core.Abstractions` namespaces (`Domain`, `Dto`, `Webapi`, `Query`) provide a foundation for implementing domain, DTO, query, and Web API models, ensuring consistency and maintainability in application design.

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

##### 7. **`PatchRequestModelBase`**

###### Summary
Represents the base class for Web API PATCH request models.

###### Purpose
- Extends `RequestModelBase` with a dictionary of property names to `PatchOperation` values.
- Validates that each operation is a defined `PatchOperation` enum value.
- Provides `TryGetOperation` for case-insensitive lookup by property name.

###### Example Usage
```csharp
public class UserPatchRequest : PatchRequestModelBase {
    public string? Name { get; set; }
    public List<string>? Roles { get; set; }
}

var patch = new UserPatchRequest {
    Name = "New Name",
    Operations = new Dictionary<string, PatchOperation> {
        ["Name"] = PatchOperation.SetField,
        ["Roles"] = PatchOperation.AddToCollection
    }
};

if (patch.TryGetOperation(nameof(UserPatchRequest.Name), out var operation)) {
    // operation == PatchOperation.SetField
}
```

---

##### 8. **`QueryResultBase<T>`**

###### Summary
Represents a base class for query-layer results with a unique identifier (`MaksIT.Core.Abstractions.Query`).

###### Purpose
- Provides a common `Id` property for read-model / query results.

###### Example Usage
```csharp
public class UserQueryResult : QueryResultBase<Guid> {
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

### Enumeration

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

The `Enumeration` class provides a powerful alternative to traditional enums, offering flexibility and functionality for scenarios requiring additional metadata or logic. In-library examples include `LoggerPrefix`, `CustomClaims`, `JwkKeyType`, `JwkCurve`, and `JwkAlgorithm`.

---

## Extensions

### Expression Extensions

The `ExpressionExtensions` class provides utility methods for combining and manipulating LINQ expressions. These methods are particularly useful for building dynamic queries in a type-safe manner.

---

#### Features

1. **Combine Expressions**:
   - Combine two predicates with `AndAlso` and `OrElse` using parameter replacement (no `Expression.Invoke`), so the result is safe for `IQueryable` and EF Core.

2. **Negate Expressions**:
   - Negate a predicate with `Not`.

3. **Batch Processing**:
   - Split an `IEnumerable<T>` into smaller lists with `Batch`.

---

#### Example Usage

##### Combining Expressions
```csharp
Expression<Func<int, bool>> isEven = x => x % 2 == 0;
Expression<Func<int, bool>> isPositive = x => x > 0;

var combined = isEven.AndAlso(isPositive);
var either = isEven.OrElse(isPositive);
var result = combined.Compile()(4); // True
```

##### Negating Expressions
```csharp
Expression<Func<int, bool>> isEven = x => x % 2 == 0;
var notEven = isEven.Not();
var result = notEven.Compile()(3); // True
```

---

### DateTime Extensions

The `DateTimeExtensions` class provides methods for manipulating and querying `DateTime` objects. These methods simplify common date-related operations.

---

#### Features

1. **Add Workdays**:
   - Add a specified number of workdays to a date, skipping weekends and dates in an `IHolidayCalendar`.

2. **Find Specific Dates**:
   - Find the next occurrence of a specific day of the week (`NextWeekday`, `ToNextWeekday`).

3. **Month and Year Boundaries**:
   - Get the start or end of the current month or year, and test those boundaries.

---

#### Example Usage

##### Adding Workdays
```csharp
public sealed class NoHolidays : IHolidayCalendar {
    public bool Contains(DateTime date) => false;
}

DateTime today = DateTime.Today;
DateTime futureDate = today.AddWorkdays(5, new NoHolidays());
```

##### Finding the Next Monday
```csharp
DateTime today = DateTime.Today;
DateTime nextMonday = today.NextWeekday(DayOfWeek.Monday);
```

---

### String Extensions

The `StringExtensions` class provides a wide range of methods for string manipulation, validation, and conversion.

---

#### Features

1. **Pattern Matching**:
   - Check if a string matches a pattern using SQL-like wildcards (`Like`).

2. **Substring Extraction**:
   - Extract substrings from the left, right, or middle of a string.

3. **Type Conversion**:
   - Convert strings to integers, booleans, dates, GUIDs, and enums (`ToObject<T>` deserializes JSON).

4. **JSON Deserialization**:
   - `ToObject<T>()` / `ToObject<T>(converters)` using `System.Text.Json`.

---

#### Example Usage

##### Pattern Matching
```csharp
bool matches = "example".Like("exa*e"); // True
```

##### Substring Extraction
```csharp
string result = "example".Left(3); // "exa"
```

##### JSON Deserialization
```csharp
var person = json.ToObject<Person>();
```

---

### Object Extensions

The `ObjectExtensions` class provides advanced methods for working with objects, including serialization, deep cloning, and structural equality comparison.

---

#### Features

1. **JSON Serialization**:
   - Convert objects to JSON strings with optional custom converters.

2. **Deep Cloning**:
   - Create a deep clone of an object, preserving reference identity and supporting cycles.

3. **Structural Equality**:
   - Compare two objects deeply for structural equality, including private fields.

4. **Snapshot Reversion**:
   - Revert an object to a previous state by copying all fields from a snapshot.

---

#### Example Usage

##### JSON Serialization
```csharp
var person = new { Name = "John", Age = 30 };
string json = person.ToJson();

// With custom converters
var converters = new List<JsonConverter> { new CustomConverter() };
string jsonWithConverters = person.ToJson(converters);
```

##### Deep Cloning
```csharp
var original = new Person { Name = "John", Age = 30 };
var clone = original.DeepClone();
```

##### Structural Equality
```csharp
var person1 = new Person { Name = "John", Age = 30 };
var person2 = new Person { Name = "John", Age = 30 };

bool areEqual = person1.DeepEqual(person2); // True
```

##### Snapshot Reversion
```csharp
var snapshot = new Person { Name = "John", Age = 30 };
var current = new Person { Name = "Doe", Age = 25 };

current.RevertFrom(snapshot);
// current.Name is now "John"
// current.Age is now 30
```

---

#### Best Practices

1. **Use Deep Cloning for Complex Objects**:
   - Ensure objects are deeply cloned when working with mutable reference types.

2. **Validate Structural Equality**:
   - Use `DeepEqual` for scenarios requiring precise object comparisons.

3. **Revert State Safely**:
   - Use `RevertFrom` to safely restore object states in tracked entities.

---

### Exception Extensions

The `ExceptionExtensions` class in the `MaksIT.Core.Extensions` namespace walks an exception chain and collects messages from the exception and every inner exception.

---

#### Features

1. **Extract Messages**:
   - `ExtractMessages()` returns a `List<string>` from the exception and its `InnerException` chain.

---

#### Example Usage

```csharp
try {
    // ...
}
catch (Exception ex) {
    var messages = ex.ExtractMessages();
}
```

---

### Formats Extensions

The `FormatsExtensions` class in the `MaksIT.Core.Extensions` namespace creates a Pax TAR archive from a directory tree.

---

#### Features

1. **Create TAR Archives**:
   - `TryCreateTarFromDirectory(sourceDirectory, outputTarPath)` writes all files under the source directory into a TAR file. Returns `false` if the source is missing, empty, or the output path cannot be created.

---

#### Example Usage

```csharp
if (FormatsExtensions.TryCreateTarFromDirectory(@"C:\data", @"C:\out\archive.tar")) {
    Console.WriteLine("TAR created");
}
```

---

### DataTable Extensions

The `DataTableExtensions` class provides methods for working with `DataTable` objects, such as counting duplicate rows and retrieving distinct records.

---

#### Features

1. **Count Duplicates**:
   - Count duplicate rows between two `DataTable` instances.

2. **Retrieve Distinct Records**:
   - Get distinct rows based on specified columns.

---

#### Example Usage

##### Counting Duplicates
```csharp
int duplicateCount = table1.DuplicatesCount(table2);
```

##### Retrieving Distinct Records
```csharp
DataTable distinctTable = table.DistinctRecords(new[] { "Name", "Age" });
```

---

### Guid Extensions

The `GuidExtensions` class provides methods for working with `Guid` values, including converting them to nullable types.

---

#### Features

1. **Convert to Nullable**:
   - Convert a `Guid` to a nullable `Guid?`, returning `null` if the `Guid` is empty.

---

#### Example Usage

##### Converting to Nullable
```csharp
Guid id = Guid.NewGuid();
Guid? nullableId = id.ToNullable();
```

---

### Enum Extensions

The `EnumExtensions` class provides utility methods for working with enum types, specifically for retrieving display names defined via the `DisplayAttribute`.

---

#### Features

1. **Get Display Name**:
   - Retrieve the value of the `DisplayAttribute.Name` property for an enum value, or fall back to the enum's name if the attribute is not present.

---

#### Example Usage

```csharp
using System.ComponentModel.DataAnnotations;
using MaksIT.Core.Extensions;

public enum Status {
    [Display(Name = "In Progress")]
    InProgress,
    Completed
}

var status = Status.InProgress;
string displayName = status.GetDisplayName(); // "In Progress"

var completed = Status.Completed;
string completedName = completed.GetDisplayName(); // "Completed"
```

---

#### Best Practices

- Use the `Display` attribute on enum members to provide user-friendly names for UI or logging.
- Use `GetDisplayName()` to consistently retrieve display names for enums throughout your application.

---

## Logging

The `Logging` namespace provides a custom file-based logging implementation that integrates with the `Microsoft.Extensions.Logging` framework.

---

### File Logger

The `FileLogger` class in the `MaksIT.Core.Logging` namespace provides a simple and efficient way to log messages to plain text files. It supports log retention policies and ensures thread-safe writes using the `LockManager`.

#### Features

1. **Plain Text Logging**:
   - Logs messages in a human-readable plain text format.

2. **Log Retention**:
   - Automatically deletes old log files based on a configurable retention period.

3. **Thread Safety**:
   - Ensures safe concurrent writes to the log file using the `LockManager`.

4. **Folder-Based Logging**:
   - Organize logs into subfolders using the `LoggerPrefix` feature.

#### Example Usage

```csharp
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddFileLogger("logs", TimeSpan.FromDays(7)));

var logger = services.BuildServiceProvider().GetRequiredService<ILogger<FileLogger>>();
logger.LogInformation("Logging to file!");
```

---

### JSON File Logger

The `JsonFileLogger` class in the `MaksIT.Core.Logging` namespace provides structured logging in JSON format. It is ideal for machine-readable logs and integrates seamlessly with log aggregation tools.

#### Features

1. **JSON Logging**:
   - Logs messages in structured JSON format, including timestamps, log levels, and exceptions.

2. **Log Retention**:
   - Automatically deletes old log files based on a configurable retention period.

3. **Thread Safety**:
   - Ensures safe concurrent writes to the log file using the `LockManager`.

4. **Folder-Based Logging**:
   - Organize logs into subfolders using the `LoggerPrefix` feature.

#### Example Usage

```csharp
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddJsonFileLogger("logs", TimeSpan.FromDays(7)));

var logger = services.BuildServiceProvider().GetRequiredService<ILogger<JsonFileLogger>>();
logger.LogInformation("Logging to JSON file!");
```

---

### Console Loggers

`LoggingBuilderExtensions` in the `MaksIT.Core.Logging` namespace also registers console logging used by MaksIT hosts (`builder.Logging.AddConsoleLogger()`).

#### Methods

| Method | Behavior |
|--------|----------|
| `AddSimpleConsoleLogger()` | Adds a timestamped simple console logger. Does not clear existing providers. |
| `AddConsoleLogger(fileLoggerPath?)` | Clears providers, adds simple console, and optionally `AddFileLogger` when a folder path is passed. |
| `AddJsonConsoleLogger(fileLoggerPath)` | Clears providers, adds JSON console, and optionally `AddJsonFileLogger` when a folder path is passed. |

Timestamps use `yyyy-MM-ddTHH:mm:ss.fffZ` and scopes are included.

#### Example Usage

```csharp
builder.Logging.AddConsoleLogger();
builder.Logging.AddConsoleLogger("logs");
builder.Logging.AddJsonConsoleLogger("logs");
```

---

### Logger Prefix

The `LoggerPrefix` class in the `MaksIT.Core.Logging` namespace provides a type-safe way to specify logger categories with special prefixes. It extends the `Enumeration` base class and enables organizing logs into subfolders or applying custom categorization without using magic strings.

#### Features

1. **Type-Safe Prefixes**:
   - Avoid magic strings by using strongly-typed prefix constants.

2. **Folder-Based Organization**:
   - Use `LoggerPrefix.Folder` to write logs to specific subfolders.

3. **Extensible Categories**:
   - Additional prefixes like `LoggerPrefix.Category` and `LoggerPrefix.Tag` are available for future use.

4. **Automatic Parsing**:
   - Parse category names to extract prefix and value using `LoggerPrefix.Parse()`.

5. **Backward Compatible**:
   - Standard `ILogger<T>` usage remains unchanged; prefixes are only applied when explicitly used.

#### Available Prefixes

| Prefix | Purpose |
|--------|---------|
| `LoggerPrefix.Folder` | Writes logs to a subfolder with the specified name |
| `LoggerPrefix.Category` | Reserved for categorization (future use) |
| `LoggerPrefix.Tag` | Reserved for tagging (future use) |

#### Example Usage

##### Creating a Logger with a Folder Prefix
```csharp
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddFileLogger("logs", TimeSpan.FromDays(7)));

var provider = services.BuildServiceProvider();
var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

// Create a logger that writes to logs/Audit/log_yyyy-MM-dd.txt
var auditLogger = loggerFactory.CreateLogger(LoggerPrefix.Folder.WithValue("Audit"));
auditLogger.LogInformation("Audit event occurred");

// Create a logger that writes to logs/Orders/log_yyyy-MM-dd.txt
var ordersLogger = loggerFactory.CreateLogger(LoggerPrefix.Folder.WithValue("Orders"));
ordersLogger.LogInformation("Order processed");
```

##### Standard ILogger<T> Usage (Unchanged)
```csharp
// Standard usage - logs go to the default folder (logs/log_yyyy-MM-dd.txt)
var logger = provider.GetRequiredService<ILogger<MyService>>();
logger.LogInformation("Standard log message");
```

##### Parsing a Category Name
```csharp
var categoryName = "Folder:Audit";
var (prefix, value) = LoggerPrefix.Parse(categoryName);

if (prefix == LoggerPrefix.Folder) {
    Console.WriteLine($"Folder: {value}"); // Output: Folder: Audit
}
```

#### Result

| Logger Creation | Log File Location |
|-----------------|-------------------|
| `ILogger<MyService>` | `logs/log_2026-01-30.txt` |
| `CreateLogger(LoggerPrefix.Folder.WithValue("Audit"))` | `logs/Audit/log_2026-01-30.txt` |
| `CreateLogger(LoggerPrefix.Folder.WithValue("Orders"))` | `logs/Orders/log_2026-01-30.txt` |

#### Best Practices

1. **Use Type-Safe Prefixes**:
   - Always use `LoggerPrefix.Folder.WithValue()` instead of raw strings like `"Folder:Audit"`.

2. **Organize by Domain**:
   - Use meaningful folder names to organize logs by domain (e.g., "Audit", "Orders", "Security").

3. **Keep Default Logging Simple**:
   - Use standard `ILogger<T>` for general application logging and folder prefixes for specialized logs.

---

## Threading

### Lock Manager

The `LockManager` class in the `MaksIT.Core.Threading` namespace provides a robust solution for managing concurrency and rate limiting. It ensures safe access to shared resources in multi-threaded or multi-process environments.

#### Features

1. **Thread Safety**:
   - Ensures mutual exclusion using a semaphore.

2. **Rate Limiting**:
   - Limits the frequency of access to shared resources using a token bucket rate limiter.

3. **Reentrant Locks**:
   - Supports reentrant locks for the same thread.

#### Example Usage

```csharp
var lockManager = new LockManager();

await lockManager.ExecuteWithLockAsync(async () => {
    // Critical section
    Console.WriteLine("Executing safely");
});

lockManager.Dispose();
```

---

## Networking

### Network Connection

The `NetworkConnection` class in the `MaksIT.Core.Networking.Windows` namespace provides methods for managing connections to network shares on Windows.

---

#### Features

1. **Connect to Network Shares**:
   - Establish connections to shared network resources.

2. **Error Handling**:
   - Provides detailed error messages for connection failures.

---

#### Example Usage

```csharp
var credentials = new NetworkCredential("username", "password");
if (NetworkConnection.TryCreate(logger, @"\\server\share", credentials, out var connection, out var error)) {
    connection.Dispose();
}
```

---

### Ping Port

The `PingPort` class provides methods for checking the reachability of a host on specified TCP or UDP ports.

---

#### Features

1. **TCP Port Checking**:
   - Check if a TCP port is reachable.

2. **UDP Port Checking**:
   - Check if a UDP port is reachable.

---

#### Example Usage

##### Checking a TCP Port
```csharp
if (PingPort.TryHostPort("example.com", 80, out var error)) {
    Console.WriteLine("Port is reachable.");
}
```

---

## Security

### AES-GCM Utility

The `AESGCMUtility` class provides methods for encrypting and decrypting data using AES-GCM.

---

#### Features

1. **Secure Encryption**:
   - Encrypt data with AES-GCM.

2. **Data Integrity**:
   - Ensure data integrity with authentication tags.

---

#### Example Usage

##### Encrypting Data
```csharp
var key = AESGCMUtility.GenerateKeyBase64();
AESGCMUtility.TryEncryptData(data, key, out var encryptedData, out var error);
AESGCMUtility.TryDecryptData(encryptedData, key, out var decrypted, out var decryptError);
```

---

### Base32 Encoder

The `Base32Encoder` class provides methods for encoding and decoding data in Base32 format.

---

#### Features

1. **Encoding**:
   - Encode binary data to Base32.

2. **Decoding**:
   - Decode Base32 strings to binary data.

---

#### Example Usage

##### Encoding Data
```csharp
Base32Encoder.TryEncode(data, out var encoded, out var error);
```

---

### Base64Url Utility

The `Base64UrlUtility` class in the `MaksIT.Core.Security` namespace provides RFC 4648 §5 Base64Url encoding and decoding (used by JWK/JWS).

---

#### Features

1. **Encode**:
   - Encode a UTF-8 string or byte array to a Base64Url string (no padding; `+`/`/` replaced with `-`/`_`).

2. **Decode**:
   - Decode a Base64Url string to bytes (`Decode`) or a UTF-8 string (`DecodeToString`).

---

#### Example Usage

```csharp
var encoded = Base64UrlUtility.Encode("hello");
var decoded = Base64UrlUtility.DecodeToString(encoded);
```

---

### Checksum Utility

The `ChecksumUtility` class provides methods for calculating and verifying CRC32 checksums. `Crc32` is a public `HashAlgorithm` implementation used by these helpers.

---

#### Features

1. **Checksum Calculation**:
   - Calculate CRC32 checksums for in-memory data, files, or files in chunks.

2. **Checksum Verification**:
   - Verify data integrity using CRC32 checksums.

---

#### Example Usage

##### Calculating a Checksum
```csharp
ChecksumUtility.TryCalculateCRC32Checksum(data, out var checksum, out var error);
ChecksumUtility.TryCalculateCRC32ChecksumFromFile(path, out var fileChecksum, out var fileError);
```

---

### Password Hasher

The `PasswordHasher` class provides methods for securely hashing and validating passwords using salt and pepper.

---

#### Features

1. **Salted & Peppered Hashing**:
   - Hash passwords with a unique salt and a required application-level pepper (secret).
2. **Validation**:
   - Validate passwords against stored hashes using the same salt and pepper.
3. **Strong Security**:
   - Uses PBKDF2 with HMACSHA512 and 100,000 iterations.

---

#### Example Usage

##### Hashing a Password
```csharp
const string pepper = "YourAppSecretPepper";
PasswordHasher.TryCreateSaltedHash("password", pepper, out var hashResult, out var error);
// hashResult.Salt and hashResult.Hash are Base64 strings
```

##### Validating a Password
```csharp
const string pepper = "YourAppSecretPepper";
PasswordHasher.TryValidateHash("password", hashResult.Salt, hashResult.Hash, pepper, out var isValid, out var error);
```

---

#### API

```csharp
public static bool TryCreateSaltedHash(
    string value,
    string pepper,
    out (string Salt, string Hash)? saltedHash,
    out string? errorMessage)
```
- `value`: The password to hash.
- `pepper`: Application-level secret (not stored with the hash).
- `saltedHash`: Tuple containing the generated salt and hash (Base64 strings).
- `errorMessage`: Error message if hashing fails.

```csharp
public static bool TryValidateHash(
    string value,
    string salt,
    string hash,
    string pepper,
    out bool isValid,
    out string? errorMessage)
```
- `value`: The password to validate.
- `salt`: The Base64-encoded salt used for hashing.
- `hash`: The Base64-encoded hash to validate against.
- `pepper`: Application-level secret (must match the one used for hashing).
- `isValid`: True if the password is valid.
- `errorMessage`: Error message if validation fails.

---

#### Security Notes
- **Pepper** should be kept secret and not stored alongside the hash or salt.
- Changing the pepper will invalidate all existing password hashes.
- Always use a strong, random pepper value for your application.

---

### JWT Generator

The `JwtGenerator` class in the `MaksIT.Core.Security.JWT` namespace provides methods for generating and validating JSON Web Tokens (JWTs). ACL entries are stored with the `CustomClaims.AclEntry` claim type (`acl_entry`).

---

#### Features

1. **Token Generation**:
   - Generate JWTs from a `JWTTokenGenerateRequest` (secret, issuer, audience, expiration, optional user id, username, roles, ACL entries).

2. **Token Validation**:
   - Validate JWTs against a secret, issuer, and audience; returns `JWTTokenClaims`.

3. **Secrets**:
   - `GenerateSecret(keySize)` and `GenerateRefreshToken()` produce Base64 random values.

---

#### Example Usage

##### Generating a Token
```csharp
var request = new JWTTokenGenerateRequest {
    Secret = secret,
    Issuer = issuer,
    Audience = audience,
    Expiration = 60,
    UserId = "user-1",
    Username = "jane",
    Roles = ["Admin"],
    AclEntries = ["vault:read"]
};

if (JwtGenerator.TryGenerateToken(request, out var tokenData, out var error)) {
    var (token, claims) = tokenData.Value;
}

if (JwtGenerator.TryValidateToken(secret, issuer, audience, token, out var validated, out var validateError)) {
    // validated.UserId, validated.Roles, validated.AclEntries
}
```

---

### JWK Generator

The `JwkGenerator` class in the `MaksIT.Core.Security.JWK` namespace provides a utility method for generating a minimal RSA public JWK (JSON Web Key) from a given `RSA` instance.

---

#### Features

1. **Generate RSA Public JWK**:
 - Extracts the RSA public exponent and modulus from an `RSA` object and encodes them as a JWK.

---

#### Example Usage

```csharp
using System.Security.Cryptography;
using MaksIT.Core.Security.JWK;

using var rsa = RSA.Create(2048);
var result = JwkGenerator.TryGenerateFromRSA(rsa, out var jwk, out var errorMessage);
if (result)
{
 // jwk contains KeyType, RsaExponent, RsaModulus
 Console.WriteLine($"Exponent: {jwk!.RsaExponent}, Modulus: {jwk.RsaModulus}");
}
else
{
 Console.WriteLine($"Error: {errorMessage}");
}
```

---

#### API

```csharp
public static bool TryGenerateFromRSA(
 RSA rsa,
 [NotNullWhen(true)] out Jwk? jwk,
 [NotNullWhen(false)] out string? errorMessage
)
```
- `rsa`: The RSA instance to extract public parameters from.
- `jwk`: The resulting JWK object (with `KeyType`, `RsaExponent`, and `RsaModulus`).
- `errorMessage`: Error message if generation fails.

---

#### Notes
- Only supports RSA public keys.
- The generated JWK includes only the public exponent and modulus.
- `JwkKeyType`, `JwkCurve`, and `JwkAlgorithm` are `Enumeration` types for JWK metadata.
- Returns `false` and an error message if the RSA parameters are missing or invalid.

---

### JWS Generator

The `JwsGenerator` class in the `MaksIT.Core.Security.JWS` namespace provides methods for creating JSON Web Signatures (JWS) using RSA keys and JWKs. It supports signing string or object payloads and produces JWS objects with protected headers, payload, and signature.

---

#### Features

1. **JWS Creation**:
 - Sign string or object payloads using an RSA key and JWK.
 - Produces a JWS message containing the protected header, payload, and signature.
 - Supports generic protected header and payload types.
 - Automatically sets the `Algorithm` property to `RS256` in the protected header.
 - Sets either the `KeyId` or the full `Jwk` in the protected header, depending on the presence of `KeyId`.

---

#### Example Usage

```csharp
using System.Security.Cryptography;
using MaksIT.Core.Security.JWK;
using MaksIT.Core.Security.JWS;

using var rsa = RSA.Create(2048);
JwkGenerator.TryGenerateFromRSA(rsa, out var jwk, out var errorMessage);
var header = new JwsHeader();
var payload = "my-payload";
var result = JwsGenerator.TryEncode(rsa, jwk!, header, payload, out var jwsMessage, out var error);
if (result)
{
 Console.WriteLine($"Signature: {jwsMessage!.Signature}");
}
else
{
 Console.WriteLine($"Error: {error}");
}
```

---

#### API

```csharp
public static bool TryEncode<THeader>(
 RSA rsa,
 Jwk jwk,
 THeader protectedHeader,
 [NotNullWhen(true)] out JwsMessage? message,
 [NotNullWhen(false)] out string? errorMessage
) where THeader : JwsHeader
```
- Signs an empty payload with a generic protected header.

```csharp
public static bool TryEncode<THeader, TPayload>(
 RSA rsa,
 Jwk jwk,
 THeader protectedHeader,
 TPayload? payload,
 [NotNullWhen(true)] out JwsMessage? message,
 [NotNullWhen(false)] out string? errorMessage
) where THeader : JwsHeader
```
- Signs the provided payload (string or object) with a generic protected header.

---

#### Notes
- Only supports signing (no verification or key authorization).
- The protected header is automatically set to use RS256.
- If the JWK has a `KeyId`, it is set in the header; otherwise, the full JWK is included.
- The payload is base64url encoded (as a string or JSON).
- Returns `false` and an error message if signing fails.

---

### JWK Thumbprint Utility

The `JwkThumbprintUtility` class in the `MaksIT.Core.Security.JWK` namespace provides methods for computing RFC7638 JWK SHA-256 thumbprints and generating key authorization strings for ACME challenges.

---

#### Features

1. **JWK SHA-256 Thumbprint**:
 - Computes the RFC7638-compliant SHA-256 thumbprint of a JWK (Base64Url encoded).
2. **ACME Key Authorization**:
 - Generates the key authorization string for ACME/Let's Encrypt HTTP challenges.

---

#### Example Usage

##### Computing a JWK Thumbprint
```csharp
using System.Security.Cryptography;
using MaksIT.Core.Security.JWK;

using var rsa = RSA.Create(2048);
JwkGenerator.TryGenerateFromRSA(rsa, out var jwk, out var errorMessage);
var result = JwkThumbprintUtility.TryGetSha256Thumbprint(jwk!, out var thumbprint, out var error);
if (result)
{
 Console.WriteLine($"Thumbprint: {thumbprint}");
}
else
{
 Console.WriteLine($"Error: {error}");
}
```

##### Generating ACME Key Authorization
```csharp
var token = "acme-token";
var result = JwkThumbprintUtility.TryGetKeyAuthorization(jwk!, token, out var keyAuth, out var error);
if (result)
{
 Console.WriteLine($"Key Authorization: {keyAuth}");
}
else
{
 Console.WriteLine($"Error: {error}");
}
```

---

#### API

```csharp
public static bool TryGetSha256Thumbprint(
 Jwk jwk,
 [NotNullWhen(true)] out string? thumbprint,
 [NotNullWhen(false)] out string? errorMessage
)
```
- Computes the RFC7638 SHA-256 thumbprint of the JWK.

```csharp
public static bool TryGetKeyAuthorization(
 Jwk jwk,
 string token,
 [NotNullWhen(true)] out string? keyAuthorization,
 [NotNullWhen(false)] out string? errorMessage
)
```
- Generates the ACME key authorization string: `{token}.{thumbprint}`.

---

#### Notes
- Only supports RSA JWKs (requires exponent and modulus).
- Returns `false` and an error message if required JWK fields are missing or invalid.
- Thumbprint is Base64Url encoded and suitable for ACME/Let's Encrypt HTTP challenges.

---

### TOTP Generator

The `TotpGenerator` class in the `MaksIT.Core.Security` namespace provides methods for generating and validating Time-based One-Time Passwords (TOTP) for two-factor authentication.

---

#### Features

1. **TOTP Validation**:
   - Validate TOTP codes against a shared secret with configurable time tolerance.

2. **TOTP Generation**:
   - Generate TOTP codes from a Base32-encoded secret.

3. **Secret Generation**:
   - Generate cryptographically secure Base32 secrets for TOTP setup.

4. **Recovery Codes**:
   - Generate backup recovery codes for account recovery.

5. **Auth Link Generation**:
   - Generate `otpauth://` URIs for QR code scanning in authenticator apps.

---

#### Example Usage

##### Generating a Secret
```csharp
TotpGenerator.TryGenerateSecret(out var secret, out var error);
// secret is a Base32-encoded string for use with authenticator apps
```

##### Validating a TOTP Code
```csharp
var timeTolerance = 1; // Allow 1 time step before/after current
TotpGenerator.TryValidate(totpCode, secret, timeTolerance, out var isValid, out var error);
if (isValid) {
    Console.WriteLine("TOTP is valid");
}
```

##### Generating Recovery Codes
```csharp
TotpGenerator.TryGenerateRecoveryCodes(10, out var recoveryCodes, out var error);
// recoveryCodes contains 10 codes in format "XXXX-XXXX"
```

##### Generating an Auth Link for QR Code
```csharp
TotpGenerator.TryGenerateTotpAuthLink(
    "MyApp",
    "user@example.com",
    secret,
    "MyApp",
    null, // algorithm (default SHA1)
    null, // digits (default 6)
    null, // period (default 30)
    out var authLink,
    out var error
);
// authLink = "otpauth://totp/MyApp:user@example.com?secret=...&issuer=MyApp"
```

---

## Web API

The `Webapi` namespace provides models and utilities for building Web APIs, including pagination support and patch operations.

---

### Paged Request

The `PagedRequest` class in the `MaksIT.Core.Webapi.Models` namespace provides a base class for paginated API requests with filtering and sorting capabilities.

#### Features

1. **Pagination**:
   - Configure page size and page number for paginated results.

2. **Dynamic Filtering**:
   - Build filter expressions from string-based filter queries.

3. **Dynamic Sorting**:
   - Build sort expressions with ascending/descending order.

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PageSize` | `int` | `100` | Number of items per page |
| `PageNumber` | `int` | `1` | Current page number |
| `Filters` | `string?` | `null` | Filter expression string |
| `SortBy` | `string?` | `null` | Property name to sort by |
| `IsAscending` | `bool` | `true` | Sort direction |

#### Example Usage

```csharp
var request = new PagedRequest {
    PageSize = 20,
    PageNumber = 1,
    Filters = "Name.Contains(\"John\") && Age > 18",
    SortBy = "Name",
    IsAscending = true
};

var filterExpression = request.BuildFilterExpression<User>();
var sortExpression = request.BuildSortExpression<User>();

var results = dbContext.Users
    .Where(filterExpression)
    .OrderBy(sortExpression)
    .Skip((request.PageNumber - 1) * request.PageSize)
    .Take(request.PageSize)
    .ToList();
```

---

### Paged Response

The `PagedResponse<T>` class in the `MaksIT.Core.Webapi.Models` namespace provides a generic wrapper for paginated API responses.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Items` | `IEnumerable<T>` | The items for the current page |
| `PageNumber` | `int` | Current page number |
| `PageSize` | `int` | Number of items per page |
| `TotalCount` | `int` | Total number of items across all pages |
| `TotalPages` | `int` | Calculated total number of pages |
| `HasPreviousPage` | `bool` | Whether a previous page exists |
| `HasNextPage` | `bool` | Whether a next page exists |

#### Example Usage

```csharp
var items = await dbContext.Users
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

var totalCount = await dbContext.Users.CountAsync();

var response = new PagedResponse<UserDto>(items, totalCount, pageNumber, pageSize);

// response.TotalPages, response.HasNextPage, etc. are automatically calculated
```

---

### Patch Operation

The `PatchOperation` enum in the `MaksIT.Core.Webapi.Models` namespace defines operations for partial updates (PATCH requests). Pair it with `PatchRequestModelBase` (`Operations` dictionary + `TryGetOperation`).

#### Values

| Value | Description |
|-------|-------------|
| `SetField` | Set or replace a normal field value |
| `RemoveField` | Set a field to null |
| `AddToCollection` | Add an item to a collection property |
| `RemoveFromCollection` | Remove an item from a collection property |

#### Example Usage

```csharp
public class UserPatchRequest : PatchRequestModelBase {
    public string? Name { get; set; }
    public List<string>? Roles { get; set; }
}

var patch = new UserPatchRequest {
    Name = "New Name",
    Operations = new Dictionary<string, PatchOperation> {
        ["Name"] = PatchOperation.SetField,
        ["Roles"] = PatchOperation.AddToCollection
    }
};

if (patch.TryGetOperation(nameof(UserPatchRequest.Name), out var operation)) {
    // operation == PatchOperation.SetField
}
```

---

### Error Handling Middleware

The `ErrorHandlingMiddleware` class in the `MaksIT.Core.Webapi.Middlewares` namespace catches unhandled exceptions, logs them, and returns HTTP 500 with a JSON body `{ error, details }`. Register it early in the ASP.NET pipeline.

#### Example Usage

```csharp
app.UseMiddleware<ErrorHandlingMiddleware>();
```

---

### Trace ID Logging Scope Middleware

The `TraceIdLoggingScopeMiddleware` class in the `MaksIT.Core.Webapi.Middlewares` namespace adds a `TraceId` logging scope from `Activity.Current` or `HttpContext.TraceIdentifier`.

#### Example Usage

```csharp
app.UseMiddleware<TraceIdLoggingScopeMiddleware>();
```

---

## Sagas

The `MaksIT.Core.Sagas` namespace provides a local saga runner with LIFO compensation on failure. Steps are registered on `LocalSagaBuilder` (an `ILogger` is required). `LocalSagaStep<T>` is internal; use `AddAction` / `AddStep` / `AddActionIf` / `AddStepIf`. Share state through `LocalSagaContext`.

---

#### Features

1. **Saga Context**:
   - `Get` / `Set` / `Contains` for passing values between steps.

2. **Actions and Steps**:
   - `AddAction` for side effects; `AddStep<T>` to store a result under `outputKey`. Conditional variants skip when the predicate is false.

3. **Compensation**:
   - Optional compensate callbacks run in reverse order when a later step throws.

---

#### Example Usage

```csharp
var saga = new LocalSagaBuilder(logger)
    .AddAction("Reserve", async (ctx, ct) => {
        ctx.Set("orderId", "123");
        await Task.CompletedTask;
    }, compensate: async (ctx, ct) => {
        await Task.CompletedTask;
    })
    .AddStep<int>("Charge", async (ctx, ct) => 42, outputKey: "amount")
    .AddActionIf(ctx => ctx.Contains("amount"), "Notify", async (ctx, ct) => {
        await Task.CompletedTask;
    })
    .Build();

await saga.ExecuteAsync();
```

---

#### Best Practices

1. **Idempotency**:
   - Ensure saga steps are idempotent to handle retries gracefully.

2. **Error Handling**:
   - Implement compensation for steps that mutate external state.

3. **State Management**:
   - Use `LocalSagaContext` to pass data between steps; back up values you need to restore on compensate.

---

The `Sagas` namespace simplifies in-process workflows with compensation, not distributed two-phase commit.

---

## CombGuidGenerator

The `CombGuidGenerator` class in the `MaksIT.Core.Comb` namespace provides methods for generating and extracting COMB GUIDs (GUIDs with embedded timestamps). COMB GUIDs improve index locality by combining randomness with a sortable timestamp.

---

#### Features

1. **Generate COMB GUIDs**:
   - Create GUIDs with embedded timestamps for improved database indexing.

2. **Extract Timestamps**:
   - Retrieve the embedded timestamp from a COMB GUID.

3. **Support for Multiple Formats**:
   - Generate COMB GUIDs compatible with SQL Server and PostgreSQL.

---

#### Example Usage

##### Generating a COMB GUID
```csharp
var baseGuid = Guid.NewGuid();
var timestamp = DateTime.UtcNow;

var combGuid = CombGuidGenerator.CreateCombGuid(baseGuid, timestamp, CombGuidType.SqlServer);
var combGuidPostgres = CombGuidGenerator.CreateCombGuid(baseGuid, timestamp, CombGuidType.PostgreSql);
```

##### Extracting a Timestamp
```csharp
var extractedTimestamp = CombGuidGenerator.ExtractTimestamp(combGuid, CombGuidType.SqlServer);
Console.WriteLine($"Extracted Timestamp: {extractedTimestamp}");
```

##### Generating a COMB GUID with Current Timestamp
```csharp
var combGuidWithCurrentTimestamp = CombGuidGenerator.CreateCombGuid(Guid.NewGuid(), CombGuidType.SqlServer);
```

---

#### Best Practices

1. **Use COMB GUIDs for Indexing**:
   - COMB GUIDs are ideal for database indexing as they improve index locality.

2. **Choose the Correct Format**:
   - Use `CombGuidType.SqlServer` for SQL Server and `CombGuidType.PostgreSql` for PostgreSQL.

3. **Ensure UTC Timestamps**:
   - Always use UTC timestamps to ensure consistency across systems.

---

The `CombGuidGenerator` class simplifies the creation and management of COMB GUIDs, making it easier to work with GUIDs in database applications.

---

## Others

### Culture

The `Culture` class provides methods for dynamically setting the culture for the current thread.

---

#### Features

1. **Dynamic Culture Setting**:
   - Change the culture for the current thread.

---

#### Example Usage

##### Setting the Culture
```csharp
Culture.TrySet("fr-FR", out var error);
```

---

### Environment Variables

The `EnvVar` class provides methods for managing environment variables.

---

#### Features

1. **Add to PATH**:
   - Add directories to the `PATH` environment variable.

2. **Set and Unset Variables**:
   - Manage environment variables at different scopes.

---

#### Example Usage

##### Adding to PATH
```csharp
EnvVar.TryAddToPath("/usr/local/bin", out var error);
EnvVar.TrySet("MY_VAR", "value", "process", out var setError);
EnvVar.TryUnSet("MY_VAR", "process", out var unsetError);
```

---

### File System

The `FileSystem` class provides methods for working with files and directories.

---

#### Features

1. **Copy Files and Folders**:
   - Copy files or directories to a target location (`TryCopyToFolder`).

2. **Delete Files and Folders**:
   - Delete files or directories (`TryDeleteFileOrDirectory`).

3. **Wildcard Paths**:
   - `ResolveWildcardedPath` expands `*` / `?` path segments (including `?:` for drives on Windows).

4. **Duplicate File Names**:
   - `DuplicateFileNameCheck` returns a non-colliding path (`file(1).ext`).

---

#### Example Usage

##### Copying Files
```csharp
FileSystem.TryCopyToFolder("source", "destination", true, out var error);
```

---

### Processes

The `Processes` class provides methods for managing system processes.

---

#### Features

1. **Start Processes**:
   - Start new processes with optional arguments.

2. **Kill Processes**:
   - Terminate processes by name (`TryKill` accepts `*` / `?` wildcards).

---

#### Example Usage

##### Starting a Process
```csharp
Processes.TryStart("notepad.exe", "", 0, false, out var error);
```

---

## Contact

If you have any questions or need further assistance, feel free to reach out:

- **Email**: [maksym.sadovnychyy@gmail.com](mailto:maksym.sadovnychyy@gmail.com)

## License

See `LICENSE.md`.