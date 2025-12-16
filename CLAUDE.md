# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a C# educational repository demonstrating SOLID principles, specifically focused on the Liskov Substitution Principle (LSP). The codebase contains multiple examples showing LSP violations and compliant solutions.

## Architecture

### What is LSP?

The Liskov Substitution Principle states that subtypes must be substitutable for their base types without breaking the client's expectations. Violations occur when:

1. **Strengthening preconditions**: Subtype requires stricter input constraints than parent
2. **Weakening postconditions**: Subtype provides weaker guarantees than parent
3. **Changing behavioral contracts**: Subtype behaves differently than clients expect

### Example 1: Rectangle/Square (liskov/LiskovSamples/LiskovSamples/Rectangle.cs)

This classic example demonstrates LSP violations, though it has limitations as a teaching tool:

**Mutable approach (Rectangle/Square)**:
- `Rectangle`: Mutable base class with virtual Height/Width properties
- `Square`: Overrides setters to maintain square invariant - when you set Width, it also changes Height
- **Violation**: Breaks Rectangle's behavioral contract that width and height are independent

**Immutable approach (Rectangle2/Square2)**:
- `Rectangle2`: Immutable rectangle with 2-parameter constructor (height, width)
- `Square2`: Immutable square with 1-parameter constructor (single side)
- **Violation**: Even though calculations work correctly, Square2 **strengthens the precondition** by requiring width == height, while Rectangle2's contract allows independent dimensions (x != y)

The key insight: LSP violations aren't just about runtime errors or wrong calculations - they're about **behavioral contracts and constraints**. Even when the code produces correct results, if a subtype imposes stronger constraints than its parent, it violates LSP.

### Example 2: Database Connections (liskov/LiskovSamples/LiskovSamples/DatabaseConnection.cs)

A realistic example based on actual problems in production codebases - different database providers with varying transaction support:

**Real-world context**: Applications often need to support multiple database types (SQL Server, MongoDB, PostgreSQL). Not all databases support the same features, especially transactions.

**LSP Violations**:
- `DatabaseConnection`: Base class with contract "supports transactions via BeginTransaction/Commit/Rollback"
- `MongoDbConnection`: **Strengthens precondition** - throws `NotSupportedException` because MongoDB (in some configurations) doesn't support transactions
- `ReadOnlyDatabaseConnection`: **Strengthens precondition** - rejects write operations and transactions, only allows SELECT queries

When `DataService.SaveWithTransaction()` expects any `DatabaseConnection` to support transactions, passing `MongoDbConnection` causes runtime failures - clear LSP violation.

**LSP-Compliant Solution using Interface Segregation**:
- `IDataConnection`: Base interface for all connections (Open/Close/ExecuteNonQuery)
- `ITransactionalConnection`: Extends IDataConnection, adds transaction support (BeginTransaction/Commit/Rollback)
- `IReadOnlyConnection`: Extends IDataConnection for read-only operations

**Implementations**:
- `SqlServerConnection`: Implements `ITransactionalConnection` - clearly advertises transaction support
- `MongoDbConnectionCorrect`: Implements only `IDataConnection` - no false promises about transactions
- `ReadOnlyConnectionCorrect`: Implements `IReadOnlyConnection` - explicit about limitations

**Key benefit**: The type system prevents LSP violations at **compile time**. You cannot pass a non-transactional connection to `DataService.SaveWithTransactionCorrect(ITransactionalConnection connection)` - the compiler stops you.

This demonstrates the real-world solution: **Interface Segregation Principle** + proper type design prevents LSP violations before runtime.

### Example 3: Birds and Flying (liskov/LiskovSamples/LiskovSamples/Bird.cs)

The most intuitive LSP example - demonstrating that inheritance should model behavioral capabilities, not just taxonomic classification:

**Real-world context**: Not all birds can fly. Penguins, ostriches, and kiwis are flightless birds. Assuming all birds fly creates LSP violations.

**LSP Violations**:
- `Bird`: Base class with contract "all birds can fly via Fly() method"
- `Penguin`: **Strengthens precondition** - throws `InvalidOperationException` because penguins cannot fly (they swim instead)
- `Ostrich`: **Strengthens precondition** - throws exception because ostriches are too heavy to fly (they run at 70 km/h instead)
- `RubberDuck`: **Multiple violations** - it's a toy, cannot fly, cannot eat (violates multiple Bird contracts)

When `BirdMigrationService.MigrateSouth()` expects all `Bird` instances to fly, passing a `Penguin` or `Ostrich` causes runtime exceptions - classic LSP violation.

**Why this is confusing**: Biologically, penguins ARE birds. But in OOP, inheritance models **behavioral substitutability**, not taxonomic classification. Just because "IS-A" is true in biology doesn't mean inheritance is correct in code.

**LSP-Compliant Solution using Capability Interfaces**:
- `IAnimal`: Base interface (Name, Weight, MakeSound, Eat)
- `IFlyable`: Capability interface for flying animals (Fly, MaxAltitude)
- `ISwimmable`: Capability interface for swimming animals (Swim)
- `IRunnable`: Capability interface for running animals (Run, MaxSpeed)

**Implementations based on actual capabilities**:
- `EagleCorrect`: Implements `IAnimal` + `IFlyable` - can fly
- `DuckCorrect`: Implements `IAnimal` + `IFlyable` + `ISwimmable` - can fly AND swim
- `PenguinCorrect`: Implements `IAnimal` + `ISwimmable` - can only swim, NOT fly
- `OstrichCorrect`: Implements `IAnimal` + `IRunnable` - can only run, NOT fly

**Key benefit**:
- `BirdMigrationService.MigrateSouthCorrect(List<IFlyable> flyingAnimals)` only accepts animals that can actually fly
- Compiler prevents passing `PenguinCorrect` - it doesn't implement `IFlyable`
- Each animal advertises exactly what it can do - no false promises

This example teaches: **Use composition over inheritance. Model capabilities, not classifications.**

### Example 4: Invoice Processing with Purchase Orders (liskov/LiskovSamples/LiskovSamples/Invoice.cs)

A realistic business example demonstrating LSP violations through unexpected exceptions in an accounting/invoicing system:

**Real-world context**: Companies process different types of invoices. Regular invoices can be processed immediately, but Purchase Order (PO) invoices have pre-approved spending limits. When an invoice exceeds the PO limit, it violates the spending constraint. This business rule often leads to LSP violations.

**LSP Violation - Throwing Unexpected Exception:**

**`PurchaseOrderInvoice`** - Violates LSP:
- PO is already approved with a spending limit (e.g., PO-12345 approved for $10,000)
- Throws `PurchaseOrderLimitExceededException` when invoice amount exceeds the approved limit
- Base `Invoice` class doesn't document this exception - has no concept of spending limits

**Why This Violates LSP:**

The base `Invoice` class establishes a contract:
```csharp
// Contract: Process invoice if valid, return bool
public virtual bool Process() { ... }

// Contract: Amend amount if not processed, return bool
public virtual bool AmendAmount(decimal newAmount) { ... }
```

But `PurchaseOrderInvoice` **strengthens the precondition** by requiring:
- Amount must not exceed PO approved limit (new constraint not in base contract)

When client code expects base `Invoice` behavior, it crashes:

```csharp
// Client expects: any Invoice can be processed
Invoice invoice = new PurchaseOrderInvoice("INV-001", 15000m, DateTime.Today, "PO-12345", 10000m);
invoice.Process(); // CRASH! Throws PurchaseOrderLimitExceededException
```

**Real-World Impact - Batch Processing:**

The violation becomes critical in batch processing scenarios:

```csharp
// Process 100 invoices overnight
foreach (var invoice in invoices) {
    invoice.Process(); // Works for 99 invoices, crashes on 1 PO invoice exceeding limit
}
// Entire batch fails, nothing gets processed
```

**LSP-Compliant Solution: Result Pattern**

Instead of throwing exceptions, return explicit results:

```csharp
public interface IInvoiceDocument {
    InvoiceResult Process();
    InvoiceResult AmendAmount(decimal newAmount);
}

public class InvoiceResult {
    public bool Success { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, object>? ErrorDetails { get; init; }
}
```

**Compliant Implementations:**

1. **`StandardInvoice`** - Returns results for success/failure
2. **`PurchaseOrderInvoiceCompliant`** - Returns error results instead of throwing:
   - `PO_LIMIT_EXCEEDED` - when invoice amount exceeds PO approved limit
   - Includes rich error details: PO number, approved amount, invoice amount, difference

**Key Benefits:**

1. **Batch processing works**: One failed invoice doesn't crash entire batch
```csharp
var results = ProcessInvoiceBatchCorrect(invoices);
// Returns: { Successful: [...], Failed: [...] }
// All invoices processed, failures logged with reasons
```

2. **All failures are explicit**: ErrorCode makes failures discoverable
3. **Rich error context**: ErrorDetails dictionary provides business data (approved amount, requested amount, difference)
4. **Perfect substitutability**: All `IInvoiceDocument` implementations have identical contracts

**Real-world lesson**: Business rules that add constraints (spending limits, approval requirements) should be modeled as explicit failures in the return type, not as exceptions. This enables robust batch processing and prevents LSP violations.

**Why this example is realistic**: In real accounting systems, Purchase Orders are pre-approved with budget limits. When processing hundreds of invoices overnight, you don't want one invoice exceeding its PO limit to crash the entire batch - you want to log the failure and continue processing the others.

### Example 5: Covariance and Contravariance (liskov/LiskovSamples/LiskovSamples/Rules/Tau.cs + liskov/contravariance-cpp-example.cpp)

Understanding the formal rules of LSP regarding method signatures:

**What LSP Allows and Prohibits:**

1. **Return Types - Covariance (Allowed)**:
   - Base method returns `Result`
   - Derived method can return `SubResult` (more specific type)
   - ✓ LSP Compliant: Client gets at least what was promised
   - Both C# (modern) and C++ support this

2. **Parameter Types - Contravariance (Would Violate LSP)**:
   - Base method accepts `ServiceArgument`
   - Derived method CANNOT accept `Argument` (more general type)
   - ✗ LSP Violation: Strengthens precondition, breaks client expectations
   - Neither C# nor C++ allow this in method overrides

**Why Contravariance Violates LSP:**

The `Tau.cs` example demonstrates this:
- `Service.Action(ServiceArgument arg)` - client expects `ServiceArgument` with its specific methods
- If derived class could accept `Action(Argument arg)` - client code would break
- Client might call `arg.GetServiceId()` which doesn't exist on base `Argument`

**C++ vs C# Behavior:**

- **C#**: Method signatures must match exactly (except covariant returns in modern C#)
- **C++**:
  - Covariant return types are allowed
  - Different parameter types create **overloading** (new function), not **overriding**
  - The overloaded version is not accessible through base class pointer
  - See `contravariance-cpp-example.cpp` for detailed demonstration

**Formal LSP Signature Rules:**
- Return types: Can be **covariant** (more specific)
- Parameter types: Must be **contravariant or invariant** (same or more general)
- BUT: Most languages enforce **invariant** parameters to prevent LSP violations
- The asymmetry makes sense: returning more is safe, accepting less is unsafe

## Development Commands

### Building the Solution

```bash
dotnet build liskov/LiskovSamples/LiskovSamples.sln
```

### Running Tests

```bash
# Run all tests
dotnet test liskov/LiskovSamples/LiskovSamples.sln

# Run a specific test
dotnet test liskov/LiskovSamples/LiskovSamples.Tests/LiskovSamples.Tests.csproj --filter "FullyQualifiedName~AreaCalculatorTests.Six_For_2x3_Rectangle"

# Run tests with detailed output
dotnet test liskov/LiskovSamples/LiskovSamples.sln --verbosity normal
```

### Running the Application

```bash
dotnet run --project liskov/LiskovSamples/LiskovSamples/LiskovSamples.csproj
```

## Project Structure

- `liskov/LiskovSamples/LiskovSamples/`: Main console application (currently minimal, just prints "Hello, World!")
- `liskov/LiskovSamples/LiskovSamples.Tests/`: xUnit test project using AwesomeAssertions for fluent assertions

## Technology Stack

- .NET 10.0
- xUnit for testing
- AwesomeAssertions for fluent assertion syntax
- coverlet.collector for code coverage
