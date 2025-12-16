using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using LiskovSamples;

namespace LiskovSamples.InvoiceWorkflow.DddStuff;

// Case 2: InvoiceWithPO has its own lifecycle and rules
//
// Now consider:
//
// PO invoices must be matched, approved, three-way checked
//
// Cannot be paid until PO + receipt are reconciled
//
// Different states, commands, and invariants
//
// Different business language: AP vs AR, compliance, audits
//
// Then ask:
//
// Can an Invoice turn into an InvoiceWithPO?
public sealed class InvoiceWithPO : Entity<int>
{
    public string Number { get; }
    public decimal Amount { get; private set; }
    public DateTime IssuedOn { get; }
    public InvoiceState State { get; private set; }
    public IReadOnlyCollection<PaymentTerms> PaymentTerms { get; private set; }

    public PurchaseOrder PurchaseOrder { get; private set; }

    private InvoiceWithPO(string number, decimal amount, DateTime issuedOn)
    {
        Number = string.IsNullOrWhiteSpace(number)
            ? throw new ArgumentException("Invoice number must be provided", nameof(number))
            : number;

        Amount = amount;
        IssuedOn = issuedOn;
        State = InvoiceState.Draft;
        PaymentTerms = Array.AsReadOnly(new[] { DddStuff.PaymentTerms.CreateStandard() });
    }

    public static InvoiceWithPO Create(string number, decimal amount, DateTime issuedOn)
        => new(number, amount, issuedOn);

   
    public bool AttachPurchaseOrder(PurchaseOrder po)
    {
        if (po is null)
            return false;
   
        if (State != InvoiceState.Draft)
            return false;

        PurchaseOrder = po;
        return true;
    }
    
    public bool UpdateAmount(decimal newAmount)
    {
        if (State != InvoiceState.Draft)
            return false;

        Amount = newAmount;
        return true;
    }
    
    public bool SubmitForApproval()
    {
        if (Amount <= 0)
            return false;
        if (State != InvoiceState.Draft)
            return false;

        if (PurchaseOrder is not null)
        {
            if (Amount > PurchaseOrder.TotalAmount)
                return false;
        }

        State = InvoiceState.ReadyForApproval;
        return true;
    }
    
    public bool Approve()
    {
        if (State != InvoiceState.ReadyForApproval)
            return false;

        State = InvoiceState.Approved;
        return true;
    }
    
    public DateTime DueDate()
    {
        var terms = GetTerms();
        return IssuedOn.AddDays(terms.DaysToPay);
    }
    
    public DateTime DiscountDate()
    {
        var terms = GetTerms();
        return IssuedOn.AddDays(terms.DaysToPay);
    }
    
    public decimal AmountIfPaidOn(DateTime paymentDate)
    {
        var terms = GetTerms(paymentDate);
        var discountDeadline = IssuedOn.AddDays(terms.DaysToPay);
        return paymentDate <= discountDeadline
            ? Amount * (1 - terms.DiscountPercentage / 100m)
            : Amount;
    }

    private PaymentTerms GetTerms()
    {
        if (PaymentTerms is { Count: > 0 })
        {
            foreach (var t in PaymentTerms)
                return t;
        }
        return DddStuff.PaymentTerms.CreateStandard();
    }

    private PaymentTerms GetTerms(DateTime asOfDate)
    {
        return GetTerms();
    }
}
