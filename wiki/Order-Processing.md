# Use Case: Order Processing

Enrich active customer orders with a subtotal, apply sales tax, and surface the highest-value orders for prioritised fulfilment.

## Scenario

An e-commerce system holds order records like:

```json
{ "OrderId": "ORD-001", "Customer": "Alice",   "Status": "Active",    "UnitPrice": 250.0, "Qty": 2, "TaxRate": 0.08 }
{ "OrderId": "ORD-002", "Customer": "Bob",     "Status": "Cancelled", "UnitPrice": 150.0, "Qty": 1, "TaxRate": 0.08 }
{ "OrderId": "ORD-003", "Customer": "Charlie", "Status": "Active",    "UnitPrice": 800.0, "Qty": 3, "TaxRate": 0.10 }
{ "OrderId": "ORD-004", "Customer": "Diana",   "Status": "Active",    "UnitPrice":  50.0, "Qty": 5, "TaxRate": 0.05 }
```

The pipeline should:

1. Keep only **Active** orders.
2. Compute **Subtotal** = `UnitPrice * Qty`.
3. Compute **TotalWithTax** = `Subtotal * (1 + TaxRate)`.
4. Sort by **TotalWithTax descending** (highest-value orders first).
5. Project to: `OrderId`, `Customer`, `Subtotal`, `TotalWithTax`.

## Pipeline JSON

```json
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
```

## Expected Output

Only ORD-002 (Cancelled) is excluded.

| OrderId  | Customer | Subtotal | TotalWithTax |
|----------|----------|----------|--------------|
| ORD-003  | Charlie  | 2400     | 2640.00      |
| ORD-001  | Alice    | 500      | 540.00       |
| ORD-004  | Diana    | 250      | 262.50       |

## C# Usage

```csharp
using System.Dynamic;
using PipeScript.NET.Core;

var orders = new List<dynamic>();
dynamic o1 = new ExpandoObject(); o1.OrderId = "ORD-001"; o1.Customer = "Alice";   o1.Status = "Active";    o1.UnitPrice = 250.0; o1.Qty = 2.0; o1.TaxRate = 0.08;
dynamic o2 = new ExpandoObject(); o2.OrderId = "ORD-002"; o2.Customer = "Bob";     o2.Status = "Cancelled"; o2.UnitPrice = 150.0; o2.Qty = 1.0; o2.TaxRate = 0.08;
dynamic o3 = new ExpandoObject(); o3.OrderId = "ORD-003"; o3.Customer = "Charlie"; o3.Status = "Active";    o3.UnitPrice = 800.0; o3.Qty = 3.0; o3.TaxRate = 0.10;
dynamic o4 = new ExpandoObject(); o4.OrderId = "ORD-004"; o4.Customer = "Diana";   o4.Status = "Active";    o4.UnitPrice =  50.0; o4.Qty = 5.0; o4.TaxRate = 0.05;
orders.AddRange(new[] { o1, o2, o3, o4 });

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
var result = engine.Execute(orders).ToList();
// result[0]: { OrderId = "ORD-003", Customer = "Charlie", TotalWithTax = 2640 }
// result[1]: { OrderId = "ORD-001", Customer = "Alice",   TotalWithTax =  540 }
// result[2]: { OrderId = "ORD-004", Customer = "Diana",   TotalWithTax =  262.5 }
```
