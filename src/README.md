# MaksIT.Core Library Documentation

## Table of Contents

- [Abstractions](#abstractions)
  - [Base Classes](#base-classes)
  - [Enumeration](#enumeration)
- [Extensions](#extensions)
  - [Expression Extensions](#expression-extensions)
  - [DateTime Extensions](#datetime-extensions)
  - [String Extensions](#string-extensions)
  - [Object Extensions](#object-extensions)
  - [DataTable Extensions](#datatable-extensions)
  - [Guid Extensions](#guid-extensions)
- [Logging](#logging)
- [Networking](#networking)
  - [Network Connection](#network-connection)
  - [Ping Port](#ping-port)
- [Security](#security)
  - [AES-GCM Utility](#aes-gcm-utility)
  - [Base32 Encoder](#base32-encoder)
  - [Checksum Utility](#checksum-utility)
  - [Password Hasher](#password-hasher)
  - [JWT Generator](#jwt-generator)
  - [TOTP Generator](#totp-generator)
- [Web API Models](#web-api-models)
- [Sagas](#sagas)
- [Others](#others)
  - [Culture](#culture)
  - [Environment Variables](#environment-variables)
  - [File System](#file-system)
  - [Processes](#processes)


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

The `Enumeration` class provides a powerful alternative to traditional enums, offering flexibility and functionality for scenarios requiring additional metadata or logic.

---

### Extensions

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

### Expression Extensions

The `ExpressionExtensions` class provides utility methods for combining and manipulating LINQ expressions. These methods are particularly useful for building dynamic queries in a type-safe manner.

---

#### Features

1. **Combine Expressions**:
   - Combine two expressions using logical operators like `AndAlso` and `OrElse`.

2. **Negate Expressions**:
   - Negate an expression using the `Not` method.

3. **Batch Processing**:
   - Divide a collection into smaller batches for processing.

---

#### Example Usage

##### Combining Expressions
```csharp
Expression<Func<int, bool>> isEven = x => x % 2 == 0;
Expression<Func<int, bool>> isPositive = x => x > 0;

var combined = isEven.AndAlso(isPositive);
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
   - Add a specified number of workdays to a date, excluding weekends and holidays.

2. **Find Specific Dates**:
   - Find the next occurrence of a specific day of the week.

3. **Month and Year Boundaries**:
   - Get the start or end of the current month or year.

---

#### Example Usage

##### Adding Workdays
```csharp
DateTime today = DateTime.Today;
DateTime futureDate = today.AddWorkdays(5);
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
   - Check if a string matches a pattern using SQL-like wildcards.

2. **Substring Extraction**:
   - Extract substrings from the left, right, or middle of a string.

3. **Type Conversion**:
   - Convert strings to various types, such as integers, booleans, and enums.

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

---

### Object Extensions

The `ObjectExtensions` class provides methods for serializing objects to JSON strings and deserializing JSON strings back to objects.

---

#### Features

1. **JSON Serialization**:
   - Convert objects to JSON strings.

2. **JSON Deserialization**:
   - Convert JSON strings back to objects.

---

#### Example Usage

##### Serialization
```csharp
var person = new { Name = "John", Age = 30 };
string json = person.ToJson();
```

##### Deserialization
```csharp
var person = json.ToObject<Person>();
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

## Logging

The `Logging` namespace provides a custom file-based logging implementation that integrates with the `Microsoft.Extensions.Logging` framework.

---

#### Features

1. **File-Based Logging**:
   - Log messages to a specified file.

2. **Log Levels**:
   - Supports all standard log levels.

3. **Thread Safety**:
   - Ensures thread-safe writes to the log file.

---

#### Example Usage

```csharp
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddFile("logs.txt"));

var logger = services.BuildServiceProvider().GetRequiredService<ILogger<FileLogger>>();
logger.LogInformation("Logging to file!");
```

---

## Networking

### Network Connection

The `NetworkConnection` class provides methods for managing connections to network shares on Windows.

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
if (NetworkConnection.TryCreate(logger, "\\server\share", credentials, out var connection, out var error)) {
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

### Checksum Utility

The `ChecksumUtility` class provides methods for calculating and verifying CRC32 checksums.

---

#### Features

1. **Checksum Calculation**:
   - Calculate CRC32 checksums for data.

2. **Checksum Verification**:
   - Verify data integrity using CRC32 checksums.

---

#### Example Usage

##### Calculating a Checksum
```csharp
ChecksumUtility.TryCalculateCRC32Checksum(data, out var checksum, out var error);
```

---

### Password Hasher

The `PasswordHasher` class provides methods for securely hashing and validating passwords.

---

#### Features

1. **Salted Hashing**:
   - Hash passwords with a unique salt.

2. **Validation**:
   - Validate passwords against stored hashes.

---

#### Example Usage

##### Hashing a Password
```csharp
PasswordHasher.TryCreateSaltedHash("password", out var hash, out var error);
```

---

### JWT Generator

The `JwtGenerator` class provides methods for generating and validating JSON Web Tokens (JWTs).

---

#### Features

1. **Token Generation**:
   - Generate JWTs with claims and metadata.

2. **Token Validation**:
   - Validate JWTs against a secret.

---

#### Example Usage

##### Generating a Token
```csharp
JwtGenerator.TryGenerateToken(secret, issuer, audience, 60, "user", roles, out var token, out var error);
```

---

### TOTP Generator

The `TotpGenerator` class provides methods for generating and validating Time-Based One-Time Passwords (TOTP).

---

#### Features

1. **TOTP Generation**:
   - Generate TOTPs based on shared secrets.

2. **TOTP Validation**:
   - Validate TOTPs with time tolerance.

---

#### Example Usage

##### Generating a TOTP
```csharp
TotpGenerator.TryGenerate(secret, TotpGenerator.GetCurrentTimeStepNumber(), out var totp, out var error);
```

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
```

---

### File System

The `FileSystem` class provides methods for working with files and directories.

---

#### Features

1. **Copy Files and Folders**:
   - Copy files or directories to a target location.

2. **Delete Files and Folders**:
   - Delete files or directories.

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
   - Terminate processes by name.

---

#### Example Usage

##### Starting a Process
```csharp
Processes.TryStart("notepad.exe", "", 0, false, out var error);
```

---