using System.Dynamic;
using PipeScript.NET.Core;

namespace PipeScript.NET.Tests;

/// <summary>
/// Real-life use-case tests that mirror the scenarios documented in the wiki folder.
/// Each test builds sample data, executes a JSON-defined pipeline, and asserts on the
/// output in a way that matches the expected results shown in the wiki pages.
/// </summary>
public class UseCaseTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Use case 1 – Sales Report
    // wiki/Sales-Report.md
    // ─────────────────────────────────────────────────────────────────────────

    private static List<dynamic> MakeSalesData()
    {
        dynamic p1 = new ExpandoObject(); p1.Name = "Laptop"; p1.Price = 1200.0; p1.Quantity = 2; p1.Category = "Electronics";
        dynamic p2 = new ExpandoObject(); p2.Name = "Book";   p2.Price =   20.0; p2.Quantity = 5; p2.Category = "Books";
        dynamic p3 = new ExpandoObject(); p3.Name = "Phone";  p3.Price =  800.0; p3.Quantity = 3; p3.Category = "Electronics";
        dynamic p4 = new ExpandoObject(); p4.Name = "Tablet"; p4.Price =  500.0; p4.Quantity = 4; p4.Category = "Electronics";
        return new List<dynamic> { p1, p2, p3, p4 };
    }

    [Fact]
    public void SalesReport_FiltersElectronics_CalculatesRevenue_SortedDesc()
    {
        const string json = """
        {
          "steps": [
            { "type": "Where",     "condition":  "Category == \"Electronics\"" },
            { "type": "Calculate", "field": "Revenue",     "expression": "Price * Quantity" },
            { "type": "OrderBy",   "field": "Revenue",     "direction": "Desc" },
            { "type": "Transform", "mappings": { "ProductName": "Name", "Revenue": "Revenue" } }
          ]
        }
        """;

        var engine = PipelineEngine.FromJson(json);
        var report = engine.Execute(MakeSalesData()).ToList();

        // Only Electronics items are included (Book is filtered out)
        Assert.Equal(3, report.Count);

        // Each row should expose exactly ProductName and Revenue
        foreach (var row in report)
        {
            var dict = (IDictionary<string, object>)row;
            Assert.True(dict.ContainsKey("ProductName"));
            Assert.True(dict.ContainsKey("Revenue"));
        }

        // Results are ordered by Revenue descending
        var revenues = report.Select(r => (double)r.Revenue).ToList();
        Assert.Equal(revenues.OrderByDescending(v => v), revenues);

        // Tablet (500 × 4 = 2000) should be last
        Assert.Equal(2000.0, (double)report[^1].Revenue);
    }

    [Fact]
    public void SalesReport_AllBooksFiltered_EmptyResult()
    {
        const string json = """
        {
          "steps": [
            { "type": "Where", "condition": "Category == \"Electronics\"" }
          ]
        }
        """;

        // Only books in the dataset – pipeline should return nothing
        dynamic b1 = new ExpandoObject(); b1.Name = "Book A"; b1.Price = 10.0; b1.Quantity = 2; b1.Category = "Books";
        dynamic b2 = new ExpandoObject(); b2.Name = "Book B"; b2.Price = 15.0; b2.Quantity = 3; b2.Category = "Books";
        var books = new List<dynamic> { b1, b2 };

        var engine = PipelineEngine.FromJson(json);
        var result = engine.Execute(books).ToList();

        Assert.Empty(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Use case 2 – Payroll Processing
    // wiki/Payroll-Processing.md
    // ─────────────────────────────────────────────────────────────────────────

    private static List<dynamic> MakeEmployeeData()
    {
        dynamic e1 = new ExpandoObject(); e1.Name = "Alice";   e1.Department = "Engineering"; e1.HoursWorked = 160.0; e1.HourlyRate = 50.0; e1.TaxRate = 0.20;
        dynamic e2 = new ExpandoObject(); e2.Name = "Bob";     e2.Department = "Marketing";   e2.HoursWorked = 140.0; e2.HourlyRate = 40.0; e2.TaxRate = 0.18;
        dynamic e3 = new ExpandoObject(); e3.Name = "Charlie"; e3.Department = "Engineering"; e3.HoursWorked = 170.0; e3.HourlyRate = 55.0; e3.TaxRate = 0.22;
        dynamic e4 = new ExpandoObject(); e4.Name = "Diana";   e4.Department = "HR";          e4.HoursWorked = 150.0; e4.HourlyRate = 35.0; e4.TaxRate = 0.15;
        return new List<dynamic> { e1, e2, e3, e4 };
    }

    [Fact]
    public void PayrollProcessing_ComputesGrossAndNetPay_SortedByNetPayDesc()
    {
        const string json = """
        {
          "steps": [
            { "type": "Calculate", "field": "GrossPay", "expression": "HoursWorked * HourlyRate" },
            { "type": "Calculate", "field": "NetPay",   "expression": "GrossPay * (1 - TaxRate)" },
            { "type": "OrderBy",   "field": "NetPay",   "direction": "Desc" },
            {
              "type": "Transform",
              "mappings": {
                "EmployeeName": "Name",
                "Department":   "Department",
                "GrossPay":     "GrossPay",
                "NetPay":       "NetPay"
              }
            }
          ]
        }
        """;

        var engine  = PipelineEngine.FromJson(json);
        var payroll = engine.Execute(MakeEmployeeData()).ToList();

        Assert.Equal(4, payroll.Count);

        // Every output record should have the projected fields
        foreach (var row in payroll)
        {
            var dict = (IDictionary<string, object>)row;
            Assert.True(dict.ContainsKey("EmployeeName"));
            Assert.True(dict.ContainsKey("Department"));
            Assert.True(dict.ContainsKey("GrossPay"));
            Assert.True(dict.ContainsKey("NetPay"));
        }

        // Charlie has the highest net pay (170 × 55 × 0.78 = 7293)
        Assert.Equal("Charlie", (string)payroll[0].EmployeeName);
        Assert.Equal(7293.0, (double)payroll[0].NetPay, 2);

        // Results are ordered by NetPay descending
        var netPays = payroll.Select(r => (double)r.NetPay).ToList();
        Assert.Equal(netPays.OrderByDescending(v => v), netPays);
    }

    [Fact]
    public void PayrollProcessing_GrossPayCalculatedCorrectly()
    {
        const string json = """
        {
          "steps": [
            { "type": "Calculate", "field": "GrossPay", "expression": "HoursWorked * HourlyRate" }
          ]
        }
        """;

        dynamic emp = new ExpandoObject(); emp.HoursWorked = 160.0; emp.HourlyRate = 50.0;

        var engine = PipelineEngine.FromJson(json);
        var result = engine.Execute(new[] { emp }).ToList();

        Assert.Single(result);
        Assert.Equal(8000.0, (double)result[0].GrossPay);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Use case 3 – Inventory Management
    // wiki/Inventory-Management.md
    // ─────────────────────────────────────────────────────────────────────────

    private static List<dynamic> MakeInventoryData()
    {
        dynamic i1 = new ExpandoObject(); i1.SKU = "A001"; i1.Name = "Widget";  i1.Stock =  5.0; i1.ReorderPoint = 20.0; i1.UnitCost =  3.50;
        dynamic i2 = new ExpandoObject(); i2.SKU = "B002"; i2.Name = "Gadget";  i2.Stock = 50.0; i2.ReorderPoint = 30.0; i2.UnitCost = 12.00;
        dynamic i3 = new ExpandoObject(); i3.SKU = "C003"; i3.Name = "Gizmo";   i3.Stock =  8.0; i3.ReorderPoint = 25.0; i3.UnitCost =  7.00;
        dynamic i4 = new ExpandoObject(); i4.SKU = "D004"; i4.Name = "Doohick"; i4.Stock = 15.0; i4.ReorderPoint = 10.0; i4.UnitCost =  5.00;
        return new List<dynamic> { i1, i2, i3, i4 };
    }

    [Fact]
    public void InventoryManagement_FiltersLowStock_CalculatesRestockCost_SortedDesc()
    {
        const string json = """
        {
          "steps": [
            { "type": "Where",     "condition":  "Stock < ReorderPoint" },
            { "type": "Calculate", "field": "RestockQty",  "expression": "ReorderPoint - Stock" },
            { "type": "Calculate", "field": "RestockCost", "expression": "RestockQty * UnitCost" },
            { "type": "OrderBy",   "field": "RestockCost", "direction": "Desc" },
            {
              "type": "Transform",
              "mappings": {
                "SKU":         "SKU",
                "Name":        "Name",
                "RestockQty":  "RestockQty",
                "RestockCost": "RestockCost"
              }
            }
          ]
        }
        """;

        var engine = PipelineEngine.FromJson(json);
        var report = engine.Execute(MakeInventoryData()).ToList();

        // Only A001 (Stock 5 < ReorderPoint 20) and C003 (Stock 8 < ReorderPoint 25) are low
        Assert.Equal(2, report.Count);

        foreach (var row in report)
        {
            var dict = (IDictionary<string, object>)row;
            Assert.True(dict.ContainsKey("SKU"));
            Assert.True(dict.ContainsKey("RestockQty"));
            Assert.True(dict.ContainsKey("RestockCost"));
        }

        // C003: RestockQty = 17, RestockCost = 17 × 7 = 119 — highest cost first
        Assert.Equal("C003", (string)report[0].SKU);
        Assert.Equal(119.0, (double)report[0].RestockCost, 2);

        // A001: RestockQty = 15, RestockCost = 15 × 3.5 = 52.5
        Assert.Equal("A001", (string)report[1].SKU);
        Assert.Equal(52.5, (double)report[1].RestockCost, 2);
    }

    [Fact]
    public void InventoryManagement_NoLowStockItems_ReturnsEmpty()
    {
        const string json = """
        {
          "steps": [
            { "type": "Where", "condition": "Stock < ReorderPoint" }
          ]
        }
        """;

        // All items are above their reorder points
        dynamic i1 = new ExpandoObject(); i1.Stock = 100.0; i1.ReorderPoint = 10.0;
        dynamic i2 = new ExpandoObject(); i2.Stock =  50.0; i2.ReorderPoint =  5.0;
        var wellStocked = new List<dynamic> { i1, i2 };

        var engine = PipelineEngine.FromJson(json);
        var result = engine.Execute(wellStocked).ToList();

        Assert.Empty(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Use case 4 – Order Processing
    // wiki/Order-Processing.md
    // ─────────────────────────────────────────────────────────────────────────

    private static List<dynamic> MakeOrderData()
    {
        dynamic o1 = new ExpandoObject(); o1.OrderId = "ORD-001"; o1.Customer = "Alice";   o1.Status = "Active";    o1.UnitPrice = 250.0; o1.Qty = 2.0; o1.TaxRate = 0.08;
        dynamic o2 = new ExpandoObject(); o2.OrderId = "ORD-002"; o2.Customer = "Bob";     o2.Status = "Cancelled"; o2.UnitPrice = 150.0; o2.Qty = 1.0; o2.TaxRate = 0.08;
        dynamic o3 = new ExpandoObject(); o3.OrderId = "ORD-003"; o3.Customer = "Charlie"; o3.Status = "Active";    o3.UnitPrice = 800.0; o3.Qty = 3.0; o3.TaxRate = 0.10;
        dynamic o4 = new ExpandoObject(); o4.OrderId = "ORD-004"; o4.Customer = "Diana";   o4.Status = "Active";    o4.UnitPrice =  50.0; o4.Qty = 5.0; o4.TaxRate = 0.05;
        return new List<dynamic> { o1, o2, o3, o4 };
    }

    [Fact]
    public void OrderProcessing_FiltersActiveOrders_AppliesTax_SortedByTotalDesc()
    {
        const string json = """
        {
          "steps": [
            { "type": "Where",     "condition":  "Status == \"Active\"" },
            { "type": "Calculate", "field": "Subtotal",     "expression": "UnitPrice * Qty" },
            { "type": "Calculate", "field": "TotalWithTax", "expression": "Subtotal * (1 + TaxRate)" },
            { "type": "OrderBy",   "field": "TotalWithTax", "direction": "Desc" },
            {
              "type": "Transform",
              "mappings": {
                "OrderId":      "OrderId",
                "Customer":     "Customer",
                "Subtotal":     "Subtotal",
                "TotalWithTax": "TotalWithTax"
              }
            }
          ]
        }
        """;

        var engine = PipelineEngine.FromJson(json);
        var result = engine.Execute(MakeOrderData()).ToList();

        // Cancelled order ORD-002 must be excluded
        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, r => (string)r.OrderId == "ORD-002");

        foreach (var row in result)
        {
            var dict = (IDictionary<string, object>)row;
            Assert.True(dict.ContainsKey("OrderId"));
            Assert.True(dict.ContainsKey("Customer"));
            Assert.True(dict.ContainsKey("Subtotal"));
            Assert.True(dict.ContainsKey("TotalWithTax"));
        }

        // ORD-003: Subtotal = 2400, TotalWithTax = 2400 × 1.10 = 2640 — highest
        Assert.Equal("ORD-003", (string)result[0].OrderId);
        Assert.Equal(2400.0, (double)result[0].Subtotal);
        Assert.Equal(2640.0, (double)result[0].TotalWithTax, 2);

        // Results are ordered by TotalWithTax descending
        var totals = result.Select(r => (double)r.TotalWithTax).ToList();
        Assert.Equal(totals.OrderByDescending(v => v), totals);
    }

    [Fact]
    public void OrderProcessing_NoActiveOrders_ReturnsEmpty()
    {
        const string json = """
        {
          "steps": [
            { "type": "Where", "condition": "Status == \"Active\"" }
          ]
        }
        """;

        dynamic o1 = new ExpandoObject(); o1.Status = "Cancelled";
        dynamic o2 = new ExpandoObject(); o2.Status = "Pending";

        var engine = PipelineEngine.FromJson(json);
        var result = engine.Execute(new[] { o1, o2 }).ToList();

        Assert.Empty(result);
    }
}
