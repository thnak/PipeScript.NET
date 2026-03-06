# Use Case: Sales Report

Generate a per-product revenue report that focuses on high-value electronics, ordered from highest to lowest revenue.

## Scenario

A retail system stores product records like:

```json
{ "Name": "Laptop",  "Category": "Electronics", "Price": 1200.0, "Quantity": 2 }
{ "Name": "Book",    "Category": "Books",        "Price":   20.0, "Quantity": 5 }
{ "Name": "Phone",   "Category": "Electronics", "Price":  800.0, "Quantity": 3 }
{ "Name": "Tablet",  "Category": "Electronics", "Price":  500.0, "Quantity": 4 }
```

The pipeline should:

1. Keep only **Electronics** items.
2. Compute **Revenue** = `Price * Quantity`.
3. Sort by **Revenue descending**.
4. Rename fields for the report output: `ProductName`, `Revenue`.

## Pipeline JSON

```json
{
  "steps": [
    { "type": "Where",     "condition":  "Category == \"Electronics\"" },
    { "type": "Calculate", "field": "Revenue",     "expression": "Price * Quantity" },
    { "type": "OrderBy",   "field": "Revenue",     "direction": "Desc" },
    { "type": "Transform", "mappings": { "ProductName": "Name", "Revenue": "Revenue" } }
  ]
}
```

## Expected Output

| ProductName | Revenue |
|-------------|---------|
| Laptop      | 2400    |
| Phone       | 2400    |
| Tablet      | 2000    |

> **Note:** Laptop and Phone share the top revenue (2400). Their relative order depends on the stable sort of the original list.

## C# Usage

```csharp
using System.Dynamic;
using PipeScript.NET.Core;

var products = new List<dynamic>();
dynamic p1 = new ExpandoObject(); p1.Name = "Laptop"; p1.Price = 1200.0; p1.Quantity = 2; p1.Category = "Electronics";
dynamic p2 = new ExpandoObject(); p2.Name = "Book";   p2.Price =   20.0; p2.Quantity = 5; p2.Category = "Books";
dynamic p3 = new ExpandoObject(); p3.Name = "Phone";  p3.Price =  800.0; p3.Quantity = 3; p3.Category = "Electronics";
dynamic p4 = new ExpandoObject(); p4.Name = "Tablet"; p4.Price =  500.0; p4.Quantity = 4; p4.Category = "Electronics";
products.AddRange(new[] { p1, p2, p3, p4 });

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
var report  = engine.Execute(products).ToList();
// report[0]: { ProductName = "Laptop",  Revenue = 2400 }
// report[1]: { ProductName = "Phone",   Revenue = 2400 }
// report[2]: { ProductName = "Tablet",  Revenue = 2000 }
```
