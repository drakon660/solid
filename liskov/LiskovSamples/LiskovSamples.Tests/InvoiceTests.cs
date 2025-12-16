using AwesomeAssertions;
using LiskovSamples.InvoiceWorkflow;
using LiskovSamples.InvoiceWorkflow.GoodStuff;

namespace LiskovSamples.Tests;

public class InvoiceTests
{
    [Fact]
    public void PurchaseOrderInvoice_ViolatesLSP_ThrowsExceptionWhenAmountMismatch()
    {
        var po = new PurchaseOrder("PO-12345", 10000m, "Acme Corp", DateTime.Today.AddDays(-5));
        var invoiceWithPO = new InvoiceWithPO("INV-PO-002", 15000m, DateTime.Today);
        invoiceWithPO.SetPurchaseOrder(po);

        Invoice invoice = invoiceWithPO;

        var invoiceService = new InvoiceService();
        var result = invoiceService.ApproveInvoice(invoice);

        result.Should().BeTrue();
    }
    
    [Fact]
    public void InvoiceProcessingService_BatchProcessing_CrashesOnSingleViolation()
    {
        var po = new PurchaseOrder("PO-12345", 10000m, "Acme Corp", DateTime.Today.AddDays(-5));

        var poInvoice = new InvoiceWithPO("INV-PO-003", 15000m, DateTime.Today);
        poInvoice.SetPurchaseOrder(po);

        var invoices = new List<Invoice>
        {
            new Invoice("INV-001", 1000m, DateTime.Today),
            new Invoice("INV-002", 2000m, DateTime.Today),
            poInvoice,
            new Invoice("INV-004", 3000m, DateTime.Today)
        };

        var invoiceService = new InvoiceService();
        var approvedCount = invoiceService.ApproveInvoiceBatch(invoices);

        approvedCount.Should().Be(3);
    }
    
    [Fact]
    public void PurchaseOrderInvoice_ViolatesLSP_WrongDueDateCalculation()
    {
        var poApprovalDate = new DateTime(2025, 1, 1);
        var invoiceDate = new DateTime(2025, 1, 15);

        var po = new PurchaseOrder("PO-12345", 10000m, "Acme Corp", poApprovalDate);
        var invoiceWithPo = new InvoiceWithPO("INV-PO-001", 10000m, invoiceDate);
        invoiceWithPo.SetPurchaseOrder(po);

        Invoice invoice = invoiceWithPo;

        var invoiceService = new InvoiceService();
        var dueDate = invoiceService.GetDueDate(invoice);

        var expectedDueDate = new DateTime(2025, 1, 29);
        dueDate.Should().Be(expectedDueDate);
    }
    
    [Fact]
    public void PurchaseOrderInvoice_ViolatesLSP_WrongDiscountAmount()
    {
        var poApprovalDate = new DateTime(2025, 1, 1);
        var invoiceDate = new DateTime(2025, 1, 15);
        var po = new PurchaseOrder("PO-12345", 10000m, "Acme Corp", poApprovalDate);
        var invoiceWithPO = new InvoiceWithPO("INV-PO-001", 10000m, invoiceDate);
        invoiceWithPO.SetPurchaseOrder(po);

        Invoice invoice = invoiceWithPO;

        var paymentDate = new DateTime(2025, 1, 16);

        var invoiceService = new InvoiceService();

        var actualAmount = invoice.CalculateDiscountedAmount(paymentDate);

        var expectedAmount = 9800m;
        actualAmount.Should().Be(expectedAmount);
    }
    
    [Fact]
    public void AgingReport_ProducesWrongResults_WithPurchaseOrderInvoice()
    {
        var poApprovalDate = new DateTime(2025, 1, 1);
        var invoiceDate = new DateTime(2025, 1, 15);
        var po = new PurchaseOrder("PO-12345", 10000m, "Acme Corp", poApprovalDate);

        var poInvoice = new InvoiceWithPO("INV-PO-003", 10000m, invoiceDate);
        poInvoice.SetPurchaseOrder(po);

        var invoices = new List<Invoice>
        {
            new Invoice("INV-001", 1000m, invoiceDate),
            new Invoice("INV-002", 2000m, invoiceDate),
            poInvoice
        };

        var invoiceService = new InvoiceService();
        var currentDate = new DateTime(2025, 2, 5);

        var invoice1Overdue = (currentDate - invoiceService.GetDueDate(invoices[0])).Days;
        var invoice2Overdue = (currentDate - invoiceService.GetDueDate(invoices[1])).Days;
        var poInvoiceOverdue = (currentDate - invoiceService.GetDueDate(invoices[2])).Days;

        invoice1Overdue.Should().Be(7);
        invoice2Overdue.Should().Be(7);

        poInvoiceOverdue.Should().Be(7);
    }
    
    [Fact]
    public void PaymentProcessing_ProducesWrongDiscounts_WithPurchaseOrderInvoice()
    {
        var poApprovalDate = new DateTime(2025, 1, 1);
        var invoiceDate = new DateTime(2025, 1, 15);
        var po = new PurchaseOrder("PO-12345", 5000m, "Acme Corp", poApprovalDate);

        var poInvoice = new InvoiceWithPO("INV-PO-003", 5000m, invoiceDate);
        poInvoice.SetPurchaseOrder(po);

        var invoices = new List<Invoice>
        {
            new Invoice("INV-001", 5000m, invoiceDate),
            new Invoice("INV-002", 5000m, invoiceDate),
            poInvoice
        };

        var paymentDate = new DateTime(2025, 1, 16);

        var invoiceService = new InvoiceService();
        var totalActual = 0m;

        foreach (var invoice in invoices)
        {
            totalActual += invoice.CalculateDiscountedAmount(paymentDate);
        }

        totalActual.Should().Be(14700m);
    }

    // ============================================
    // LSP COMPLIANT SOLUTION TESTS
    // ============================================
    
    [Fact]
    public void PurchaseOrderInvoiceCompliant_ReturnsErrorWhenAmountMismatch()
    {
        var po = new PurchaseOrder("PO-12345", 10000m, "Acme Corp", DateTime.Today.AddDays(-5));
        var invoice = new PurchaseOrderInvoiceCompliant("INV-PO-002", 15000m, DateTime.Today, po);

        invoice.SubmitForApproval().Success.Should().BeTrue();

        // LSP COMPLIANT: Returns detailed error with amounts, no exception
        var result = invoice.Approve();

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("PO_AMOUNT_MISMATCH");
        result.ErrorMessage.Should().Contain("does not match PO");
        result.ErrorMessage.Should().Contain("total");
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!["PurchaseOrderNumber"].Should().Be("PO-12345");
        result.ErrorDetails["PurchaseOrderVendor"].Should().Be("Acme Corp");
        result.ErrorDetails["PurchaseOrderTotal"].Should().Be(10000m);
        result.ErrorDetails["InvoiceAmount"].Should().Be(15000m);
    }

    // ============================================
    // LSP COMPLIANT: ALL IMPLEMENTATIONS SUBSTITUTABLE
    // ============================================

    [Fact]
    public void LSPCompliant_AllInvoiceTypesSubstitutable()
    {
        // Both implementations can be substituted without breaking client code
        IInvoiceDocument standard = new StandardInvoice("INV-001", 1000m, DateTime.Today);

        var po = new PurchaseOrder("PO-12345", 5000m, "Acme Corp", DateTime.Today.AddDays(-5));
        IInvoiceDocument poInvoice = new PurchaseOrderInvoiceCompliant("INV-PO-002", 5000m, DateTime.Today, po);

        // Same client code works with all implementations
        var result1 = InvoiceService.ApproveInvoiceCorrect(standard);
        var result2 = InvoiceService.ApproveInvoiceCorrect(poInvoice);

        // All return InvoiceResult - same contract, no exceptions, LSP satisfied
        result1.Success.Should().BeTrue();
        result2.Success.Should().BeTrue();
    }

    [Fact]
    public void LSPCompliant_BatchProcessing_HandlesAllFailuresGracefully()
    {
        var po = new PurchaseOrder("PO-12345", 10000m, "Acme Corp", DateTime.Today.AddDays(-5));

        var invoices = new List<IInvoiceDocument>
        {
            new StandardInvoice("INV-001", 1000m, DateTime.Today),
            new StandardInvoice("INV-002", 2000m, DateTime.Today),
            new PurchaseOrderInvoiceCompliant("INV-PO-003", 15000m, DateTime.Today, po), // Amount mismatch - will fail
            new StandardInvoice("INV-004", 3000m, DateTime.Today)
        };

        // Batch processing continues even when individual invoices fail
        var results = InvoiceService.ApproveInvoiceBatchCorrect(invoices);

        // First, second, and fourth invoices succeed
        results["Successful"].Count.Should().Be(3);
        results["Successful"].Should().Contain("INV-001");
        results["Successful"].Should().Contain("INV-002");
        results["Successful"].Should().Contain("INV-004");

        // Third invoice fails gracefully with error details
        results["Failed"].Count.Should().Be(1);
        results["Failed"][0].Should().Contain("INV-PO-003");
        results["Failed"][0].Should().Contain("PO_AMOUNT_MISMATCH");

        // No exceptions thrown - all failures handled gracefully
    }

    [Fact]
    public void LSPCompliant_BatchProcessing_WithMixedFailures()
    {
        var po1 = new PurchaseOrder("PO-12345", 5000m, "Acme Corp", DateTime.Today.AddDays(-5));
        var po2 = new PurchaseOrder("PO-67890", 10000m, "Widgets Inc", DateTime.Today.AddDays(-3));
        var po3 = new PurchaseOrder("PO-99999", 3000m, "Tools Ltd", DateTime.Today.AddDays(-7));

        var matchingInvoice = new PurchaseOrderInvoiceCompliant("INV-PO-001", 5000m, DateTime.Today, po1);
        var mismatchInvoice1 = new PurchaseOrderInvoiceCompliant("INV-PO-002", 15000m, DateTime.Today, po2);
        var mismatchInvoice2 = new PurchaseOrderInvoiceCompliant("INV-PO-003", 12000m, DateTime.Today, po3);

        var invoices = new List<IInvoiceDocument>
        {
            new StandardInvoice("INV-001", 1000m, DateTime.Today),
            matchingInvoice,  // Should succeed
            mismatchInvoice1,  // Should fail - amount mismatch
            mismatchInvoice2,  // Should fail - amount mismatch
            new StandardInvoice("INV-005", 4000m, DateTime.Today)
        };

        var results = InvoiceService.ApproveInvoiceBatchCorrect(invoices);

        // Successful: INV-001, INV-PO-001, INV-005
        results["Successful"].Count.Should().Be(3);

        // Failed: INV-PO-002 (amount mismatch), INV-PO-003 (amount mismatch)
        results["Failed"].Count.Should().Be(2);
        results["Failed"][0].Should().Contain("PO_AMOUNT_MISMATCH");
        results["Failed"][1].Should().Contain("PO_AMOUNT_MISMATCH");
    }

    // ============================================
    // LSP COMPLIANT: PAYMENT TERMS EXPLICIT
    // ============================================

    /// <summary>
    /// LSP Compliant: Aging reports work correctly with explicit payment terms
    /// Client uses PaymentTerms.DueDate instead of calling CalculateDueDate()
    /// All invoice types substitutable - no hidden surprises
    /// </summary>
    [Fact]
    public void LSPCompliant_AgingReport_WorksCorrectlyWithExplicitTerms()
    {
        var poApprovalDate = new DateTime(2025, 1, 1);
        var invoiceDate = new DateTime(2025, 1, 15);
        var po = new PurchaseOrder("PO-12345", 10000m, "Acme Corp", poApprovalDate);

        var invoices = new List<IInvoiceDocument>
        {
            new StandardInvoice("INV-001", 1000m, invoiceDate),
            new StandardInvoice("INV-002", 2000m, invoiceDate),
            new PurchaseOrderInvoiceCompliant("INV-PO-003", 10000m, invoiceDate, po)
        };

        var currentDate = new DateTime(2025, 2, 5);

        // Client uses explicit PaymentTerms - no surprises
        var invoice1Terms = invoices[0].GetPaymentTerms();
        var invoice2Terms = invoices[1].GetPaymentTerms();
        var poInvoiceTerms = invoices[2].GetPaymentTerms();

        // Standard invoices: Due Jan 29 (14 days), overdue by 7 days
        var invoice1Overdue = (currentDate - invoice1Terms.DueDate).Days;
        var invoice2Overdue = (currentDate - invoice2Terms.DueDate).Days;

        invoice1Overdue.Should().Be(7);
        invoice2Overdue.Should().Be(7);

        // PO invoice: Due Mar 2, not yet overdue
        var poInvoiceOverdue = (currentDate - poInvoiceTerms.DueDate).Days;

        // LSP COMPLIANT: Client gets correct aging because terms are explicit
        // Client knows PO invoice has different terms - no wrong expectations
        poInvoiceOverdue.Should().Be(-25); // Not yet due, 25 days remaining

        // All invoice types are substitutable - same interface, explicit data
    }

    /// <summary>
    /// LSP Compliant: Discount calculations work correctly with explicit payment terms
    /// All invoice types use same algorithm - different percentages are EXPLICIT in PaymentTerms
    /// </summary>
    [Fact]
    public void LSPCompliant_DiscountCalculations_WorkCorrectlyWithExplicitTerms()
    {
        var poApprovalDate = new DateTime(2025, 1, 1);
        var invoiceDate = new DateTime(2025, 1, 15);
        var po = new PurchaseOrder("PO-12345", 5000m, "Acme Corp", poApprovalDate);

        var invoices = new List<IInvoiceDocument>
        {
            new StandardInvoice("INV-001", 5000m, invoiceDate),
            new StandardInvoice("INV-002", 5000m, invoiceDate),
            new PurchaseOrderInvoiceCompliant("INV-PO-003", 5000m, invoiceDate, po)
        };

        var paymentDate = new DateTime(2025, 1, 16);

        // Client uses same method for all invoices
        var invoice1Payment = invoices[0].CalculateDiscountedAmount(paymentDate);
        var invoice2Payment = invoices[1].CalculateDiscountedAmount(paymentDate);
        var poInvoicePayment = invoices[2].CalculateDiscountedAmount(paymentDate);

        // Standard invoices: 2% discount
        invoice1Payment.Should().Be(4900m);
        invoice2Payment.Should().Be(4900m);

        // PO invoice: 3% discount (different, but EXPLICIT in PaymentTerms)
        poInvoicePayment.Should().Be(4850m);

        // LSP COMPLIANT: Different discount is expected because PaymentTerms.DiscountPercentage = 3.0
        // Client can check terms.DiscountPercentage to know what to expect
        var poTerms = invoices[2].GetPaymentTerms();
        poTerms.DiscountPercentage.Should().Be(3.0m); // Explicitly 3%, not hidden

        // All invoice types substitutable - same method, explicit behavior
    }

    /// <summary>
    /// LSP Compliant: Payment reminders work correctly with all invoice types
    /// Client uses explicit payment terms, no hidden surprises
    /// </summary>
    [Fact]
    public void LSPCompliant_PaymentReminders_WorkCorrectlyWithExplicitTerms()
    {
        var poApprovalDate = new DateTime(2025, 1, 1);
        var invoiceDate = new DateTime(2025, 1, 15);
        var po = new PurchaseOrder("PO-12345", 10000m, "Acme Corp", poApprovalDate);

        var invoices = new List<IInvoiceDocument>
        {
            new StandardInvoice("INV-001", 1000m, invoiceDate),
            new PurchaseOrderInvoiceCompliant("INV-PO-002", 10000m, invoiceDate, po)
        };

        var currentDate = new DateTime(2025, 1, 20);

        // Client checks if discount deadline is approaching (3 days warning)
        var reminders = new List<string>();

        foreach (var invoice in invoices)
        {
            var terms = invoice.GetPaymentTerms();
            var daysUntilDiscount = (terms.DiscountDate - currentDate).Days;

            if (daysUntilDiscount >= 0 && daysUntilDiscount <= 3)
            {
                reminders.Add($"{invoice.InvoiceNumber}: {terms.DiscountPercentage}% discount expires in {daysUntilDiscount} days");
            }
        }

        reminders.Count.Should().Be(0);
    }
}
