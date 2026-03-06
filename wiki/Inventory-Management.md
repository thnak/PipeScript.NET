# Use Case: Inventory Management

Identify products that are running low on stock, calculate the cost to restock each item to a target level, and surface the most expensive restocking needs first.

## Scenario

A warehouse system holds stock records like:

```json
{ "SKU": "A001", "Name": "Widget",  "Stock": 5,  "ReorderPoint": 20, "UnitCost": 3.50 }
{ "SKU": "B002", "Name": "Gadget",  "Stock": 50, "ReorderPoint": 30, "UnitCost": 12.00 }
{ "SKU": "C003", "Name": "Gizmo",   "Stock": 8,  "ReorderPoint": 25, "UnitCost": 7.00 }
{ "SKU": "D004", "Name": "Doohick", "Stock": 15, "ReorderPoint": 10, "UnitCost": 5.00 }
```

The pipeline should:

1. Keep only items where **Stock < ReorderPoint** (low-stock items).
2. Compute **RestockQty** = `ReorderPoint - Stock`.
3. Compute **RestockCost** = `RestockQty * UnitCost`.
4. Sort by **RestockCost descending** (most expensive restock first).
5. Project to: `SKU`, `Name`, `RestockQty`, `RestockCost`.

## Pipeline JSON

```json
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
```

## Expected Output

Only SKUs A001 and C003 are below their reorder points.

| SKU  | Name   | RestockQty | RestockCost |
|------|--------|------------|-------------|
| C003 | Gizmo  | 17         | 119.00      |
| A001 | Widget | 15         |  52.50      |

## C# Usage

```csharp
using System.Dynamic;
using PipeScript.NET.Core;

var inventory = new List<dynamic>();
dynamic i1 = new ExpandoObject(); i1.SKU = "A001"; i1.Name = "Widget";  i1.Stock =  5.0; i1.ReorderPoint = 20.0; i1.UnitCost =  3.50;
dynamic i2 = new ExpandoObject(); i2.SKU = "B002"; i2.Name = "Gadget";  i2.Stock = 50.0; i2.ReorderPoint = 30.0; i2.UnitCost = 12.00;
dynamic i3 = new ExpandoObject(); i3.SKU = "C003"; i3.Name = "Gizmo";   i3.Stock =  8.0; i3.ReorderPoint = 25.0; i3.UnitCost =  7.00;
dynamic i4 = new ExpandoObject(); i4.SKU = "D004"; i4.Name = "Doohick"; i4.Stock = 15.0; i4.ReorderPoint = 10.0; i4.UnitCost =  5.00;
inventory.AddRange(new[] { i1, i2, i3, i4 });

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
var report = engine.Execute(inventory).ToList();
// report[0]: { SKU = "C003", Name = "Gizmo",  RestockQty = 17, RestockCost = 119.00 }
// report[1]: { SKU = "A001", Name = "Widget", RestockQty = 15, RestockCost =  52.50 }
```
