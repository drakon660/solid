# AR vs AP - Domain-Driven Design with Liskov Substitution Principle

Based on decision.txt - A proper DDD approach that respects LSP

## Overview: Two Separate Bounded Contexts

```mermaid
graph TB
    subgraph "Bounded Context: BILLING / SALES (AR - Accounts Receivable)"
        direction TB
        AR[Invoice Aggregate]
        AR --> |contains| ARFields["- InvoiceNumber<br/>- Amount<br/>- Customer<br/>- DueDate<br/>- Status"]
        AR --> |optional| ARPO["- PurchaseOrderReference<br/>(just metadata)"]
        AR --> |lifecycle| ARFlow["Create → Send → Wait → Receive Payment"]

        ARRepo[InvoiceRepository]
        ARRepo --> |manages| ARCollection[(Invoice Collection)]
        ARCollection --> |stores| AR
    end

    subgraph "Bounded Context: ACCOUNTS PAYABLE (AP)"
        direction TB

        subgraph "Aggregate: PO Invoice"
            POInv[PoInvoice]
            POInv --> |requires| PORef["✓ PurchaseOrder (mandatory)<br/>- PO Number<br/>- PO Amount<br/>- Approved Budget"]
            POInv --> |workflow| POFlow["Receive → Match PO → Approve → Schedule Payment"]
            POInv --> |rules| PORules["- Must match PO<br/>- Cannot exceed PO amount<br/>- Requires approval<br/>- 3-way matching"]
        end

        subgraph "Aggregate: Non-PO Invoice"
            NonPOInv[NonPoInvoice]
            NonPOInv --> |workflow| NonPOFlow["Receive → Approve → Schedule Payment"]
            NonPOInv --> |rules| NonPORules["- Different approval workflow<br/>- No matching required<br/>- May have spending limits"]
        end

        PORepo[PoInvoiceRepository]
        NonPORepo[NonPoInvoiceRepository]

        PORepo --> |manages| POCollection[(PoInvoice Collection)]
        NonPORepo --> |manages| NonPOCollection[(NonPoInvoice Collection)]

        POCollection --> |stores| POInv
        NonPOCollection --> |stores| NonPOInv
    end

    style AR fill:#90EE90
    style POInv fill:#FFB6C1
    style NonPOInv fill:#FFB6C1
```

## AR (Accounts Receivable) - Simple Model

```mermaid
classDiagram
    class Invoice {
        <<Aggregate Root>>
        +InvoiceId Id
        +string InvoiceNumber
        +decimal Amount
        +CustomerId Customer
        +DateTime DueDate
        +InvoiceStatus Status
        +PurchaseOrderReference? PoReference
        ---
        +Create(amount, customer)
        +Send()
        +ReceivePayment(amount)
        +Cancel()
    }

    class PurchaseOrderReference {
        <<Value Object>>
        +string PoNumber
        +string Description
    }

    class InvoiceRepository {
        <<Repository>>
        +Save(Invoice invoice)
        +GetById(InvoiceId id)
        +GetByCustomer(CustomerId customerId)
        +GetOverdue()
    }

    Invoice *-- "0..1" PurchaseOrderReference : optional metadata
    InvoiceRepository --> Invoice : manages

    note for Invoice "Single aggregate type\nPO is just optional data\nNo special behavior"
```

## AP (Accounts Payable) - Complex Model with Separation

```mermaid
classDiagram
    class VendorInvoice {
        <<Abstract Base - Interface Segregation>>
        +InvoiceId Id
        +string InvoiceNumber
        +decimal Amount
        +VendorId Vendor
        +DateTime ReceivedDate
        +PaymentStatus Status
        ---
        +ReceiveInvoice()
        +SchedulePayment()
        +Pay()
    }

    class PoInvoice {
        <<Aggregate Root>>
        +PurchaseOrder PurchaseOrder
        +MatchingStatus MatchingStatus
        +ApprovalStatus ApprovalStatus
        ---
        +MatchToPurchaseOrder()
        +ThreeWayMatch(po, receipt)
        +SubmitForApproval()
        +Approve()
        +Reject(reason)
    }

    class NonPoInvoice {
        <<Aggregate Root>>
        +decimal SpendingLimit
        +ApprovalWorkflow Workflow
        ---
        +SubmitForApproval()
        +ApproveByManager()
        +ApproveByFinance()
    }

    class PurchaseOrder {
        <<Value Object / Entity Reference>>
        +string PoNumber
        +decimal ApprovedAmount
        +decimal RemainingAmount
        +DateTime ApprovalDate
        ---
        +ValidateInvoiceAmount(decimal amount)
    }

    class PoInvoiceRepository {
        <<Repository>>
        +Save(PoInvoice invoice)
        +GetById(InvoiceId id)
        +GetPendingMatching()
        +GetPendingApproval()
    }

    class NonPoInvoiceRepository {
        <<Repository>>
        +Save(NonPoInvoice invoice)
        +GetById(InvoiceId id)
        +GetPendingApproval()
    }

    VendorInvoice <|.. PoInvoice : implements
    VendorInvoice <|.. NonPoInvoice : implements
    PoInvoice *-- PurchaseOrder : requires
    PoInvoiceRepository --> PoInvoice : manages
    NonPoInvoiceRepository --> NonPoInvoice : manages

    note for PoInvoice "Different invariants:\n- MUST have PO\n- MUST match PO amount\n- Requires 3-way matching"
    note for NonPoInvoice "Different invariants:\n- NO PO required\n- Different approval flow\n- Simpler workflow"
```

## LSP-Compliant Design Pattern

### ❌ WRONG: Trying to make them subtypes

```mermaid
classDiagram
    class Invoice {
        +decimal Amount
        +Process()
    }

    class InvoiceWithPO {
        +PurchaseOrder PO
        +Process()
    }

    Invoice <|-- InvoiceWithPO

    note for InvoiceWithPO "❌ LSP VIOLATION!\nStrengthens precondition:\n- Base: no PO needed\n- Subtype: REQUIRES PO\n\nDifferent constraints:\n- Base: amount can be anything\n- Subtype: amount ≤ PO.ApprovedAmount"
```

### ✅ CORRECT: Interface Segregation + Separate Aggregates

```mermaid
classDiagram
    class IInvoice {
        <<interface>>
        +InvoiceId Id
        +decimal Amount
        +InvoiceStatus Status
        +ReceiveInvoice()
        +Process()
    }

    class IPayable {
        <<interface>>
        +SchedulePayment()
        +Pay()
    }

    class IRequiresApproval {
        <<interface>>
        +SubmitForApproval()
        +Approve()
        +Reject()
    }

    class IRequiresMatching {
        <<interface>>
        +MatchToPurchaseOrder()
        +GetMatchingStatus()
    }

    class ArInvoice {
        <<AR Aggregate>>
        +CustomerId Customer
        +Send()
        +ReceivePayment()
    }

    class PoInvoice {
        <<AP Aggregate>>
        +PurchaseOrder PO
        +ThreeWayMatch()
    }

    class NonPoInvoice {
        <<AP Aggregate>>
        +decimal Limit
        +SimpleApproval()
    }

    IInvoice <|.. ArInvoice
    IPayable <|.. ArInvoice

    IInvoice <|.. PoInvoice
    IPayable <|.. PoInvoice
    IRequiresApproval <|.. PoInvoice
    IRequiresMatching <|.. PoInvoice

    IInvoice <|.. NonPoInvoice
    IPayable <|.. NonPoInvoice
    IRequiresApproval <|.. NonPoInvoice

    note for ArInvoice "AR Context:\n✓ Simple lifecycle\n✓ PO is optional metadata"
    note for PoInvoice "AP Context:\n✓ PO drives behavior\n✓ Complex workflow\n✓ Different invariants"
    note for NonPoInvoice "AP Context:\n✓ No PO required\n✓ Different approval\n✓ Different rules"
```

## Invariants and Constraints Analysis

### AR Invoice (Simple)

```
Invariant:
  - Amount > 0
  - Customer exists
  - Status ∈ {Draft, Sent, Paid, Cancelled}

Constraint:
  - Status never goes backward (Draft → Sent → Paid)
  - Amount cannot change after Sent
  - PO reference is immutable (optional metadata only)
```

### AP PoInvoice (Complex)

```
Invariant:
  - Amount > 0
  - PurchaseOrder MUST exist (mandatory)
  - Amount ≤ PO.ApprovedAmount
  - MatchingStatus ∈ {Pending, Matched, Mismatched}
  - ApprovalStatus ∈ {Pending, Approved, Rejected}

Constraint:
  - PO cannot be changed after creation
  - Cannot pay before matching is complete
  - Cannot pay before approval
  - Matching → Approval → Payment (strict order)
```

### AP NonPoInvoice (Different Complex)

```
Invariant:
  - Amount > 0
  - Amount ≤ SpendingLimit
  - ApprovalStatus ∈ {Pending, ManagerApproved, FinanceApproved, Rejected}

Constraint:
  - No PO matching required
  - Manager approval → Finance approval (different workflow)
  - Cannot pay before final approval
```

## Why They Cannot Be Subtypes

| Aspect | AR Invoice | AP PoInvoice | LSP Issue |
|--------|-----------|--------------|-----------|
| **PO Requirement** | Optional metadata | Mandatory, drives behavior | Strengthened precondition ✗ |
| **Amount Validation** | Any amount | Must ≤ PO.ApprovedAmount | Additional constraint ✗ |
| **Processing** | Send → Wait → Receive | Receive → Match → Approve → Pay | Different workflow ✗ |
| **Invariants** | Simple | Complex with PO dependencies | Cannot substitute ✗ |
| **Constraints** | Linear status progression | Multi-stage approval + matching | Different history properties ✗ |

## Repository Pattern

### AR Context - Single Repository

```csharp
public interface IInvoiceRepository
{
    Task<Invoice> GetByIdAsync(InvoiceId id);
    Task<IEnumerable<Invoice>> GetByCustomerAsync(CustomerId customerId);
    Task<IEnumerable<Invoice>> GetOverdueAsync();
    Task SaveAsync(Invoice invoice);
}
```

### AP Context - Separate Repositories

```csharp
public interface IPoInvoiceRepository
{
    Task<PoInvoice> GetByIdAsync(InvoiceId id);
    Task<IEnumerable<PoInvoice>> GetPendingMatchingAsync();
    Task<IEnumerable<PoInvoice>> GetPendingApprovalAsync();
    Task<IEnumerable<PoInvoice>> GetByPurchaseOrderAsync(string poNumber);
    Task SaveAsync(PoInvoice invoice);
}

public interface INonPoInvoiceRepository
{
    Task<NonPoInvoice> GetByIdAsync(InvoiceId id);
    Task<IEnumerable<NonPoInvoice>> GetPendingApprovalAsync();
    Task<IEnumerable<NonPoInvoice>> GetByVendorAsync(VendorId vendorId);
    Task SaveAsync(NonPoInvoice invoice);
}
```

## Decision Table

```mermaid
flowchart TD
    Start{What are you modeling?}
    Start -->|Money others owe YOU| AR[AR: Accounts Receivable]
    Start -->|Money YOU owe others| AP[AP: Accounts Payable]

    AR --> ARDecision{Does PO affect<br/>business rules?}
    ARDecision -->|No, just metadata| SingleAgg[Single Invoice Aggregate<br/>+ optional PO reference]

    AP --> APDecision{Does PO drive<br/>behavior/workflow?}
    APDecision -->|YES| SeparateAgg[Separate Aggregates:<br/>PoInvoice + NonPoInvoice]
    APDecision -->|NO| ReviewContext[Review your context<br/>Might actually be AR]

    SingleAgg --> OneRepo[One Repository<br/>One Collection]
    SeparateAgg --> TwoRepos[Separate Repositories<br/>Separate Collections]

    style AR fill:#90EE90
    style AP fill:#FFB6C1
    style SingleAgg fill:#90EE90
    style SeparateAgg fill:#FFB6C1
```

## Key Insights

### Why This Respects LSP

1. **No False Inheritance**: AR Invoice and AP PoInvoice are NOT subtypes
2. **Interface Segregation**: Each type implements only the interfaces it needs
3. **Proper Invariants**: Each aggregate has its own invariants without conflicts
4. **Separate Constraints**: Different history properties for different contexts
5. **No Precondition Strengthening**: Each type defines its own requirements

### Why Separate Collections Make Sense

```
AR Invoice Collection:
- Simple queries: by customer, by due date, overdue
- Straightforward indexing
- No complex joins

AP PoInvoice Collection:
- Complex queries: by PO, by matching status, by approval status
- Different indexes needed
- Often needs joins with PO system

AP NonPoInvoice Collection:
- Different queries: by approval workflow stage
- Different performance characteristics
- Different archival rules
```

### The DDD Insight

> **"Even if they share the word 'Invoice' and similar fields, AR and AP are DIFFERENT BOUNDED CONTEXTS with different ubiquitous language, different invariants, and different business rules. Sharing implementation is a design smell."**

## Summary

- **AR**: Single aggregate, optional PO, one collection ✓
- **AP**: Separate aggregates driven by PO requirement, separate collections ✓
- **LSP**: Respected by NOT forcing inheritance where contracts differ ✓
- **DDD**: Proper bounded context separation ✓
