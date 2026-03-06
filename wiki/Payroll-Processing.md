# Use Case: Payroll Processing

Compute net pay for each employee after applying an income-tax deduction, then produce a payroll summary sorted by net pay.

## Scenario

An HR system holds employee records like:

```json
{ "Name": "Alice",   "Department": "Engineering", "HoursWorked": 160, "HourlyRate": 50.0, "TaxRate": 0.20 }
{ "Name": "Bob",     "Department": "Marketing",   "HoursWorked": 140, "HourlyRate": 40.0, "TaxRate": 0.18 }
{ "Name": "Charlie", "Department": "Engineering", "HoursWorked": 170, "HourlyRate": 55.0, "TaxRate": 0.22 }
{ "Name": "Diana",   "Department": "HR",          "HoursWorked": 150, "HourlyRate": 35.0, "TaxRate": 0.15 }
```

The pipeline should:

1. Compute **GrossPay** = `HoursWorked * HourlyRate`.
2. Compute **NetPay** = `GrossPay * (1 - TaxRate)`.
3. Sort by **NetPay descending**.
4. Project to a clean payroll record: `EmployeeName`, `Department`, `GrossPay`, `NetPay`.

## Pipeline JSON

```json
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
```

## Expected Output

| EmployeeName | Department  | GrossPay | NetPay   |
|--------------|-------------|----------|----------|
| Charlie      | Engineering | 9350     | 7293     |
| Alice        | Engineering | 8000     | 6400     |
| Bob          | Marketing   | 5600     | 4592     |
| Diana        | HR          | 5250     | 4462.50  |

## C# Usage

```csharp
using System.Dynamic;
using PipeScript.NET.Core;

var employees = new List<dynamic>();
dynamic e1 = new ExpandoObject(); e1.Name = "Alice";   e1.Department = "Engineering"; e1.HoursWorked = 160.0; e1.HourlyRate = 50.0; e1.TaxRate = 0.20;
dynamic e2 = new ExpandoObject(); e2.Name = "Bob";     e2.Department = "Marketing";   e2.HoursWorked = 140.0; e2.HourlyRate = 40.0; e2.TaxRate = 0.18;
dynamic e3 = new ExpandoObject(); e3.Name = "Charlie"; e3.Department = "Engineering"; e3.HoursWorked = 170.0; e3.HourlyRate = 55.0; e3.TaxRate = 0.22;
dynamic e4 = new ExpandoObject(); e4.Name = "Diana";   e4.Department = "HR";          e4.HoursWorked = 150.0; e4.HourlyRate = 35.0; e4.TaxRate = 0.15;
employees.AddRange(new[] { e1, e2, e3, e4 });

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
var payroll = engine.Execute(employees).ToList();
// payroll[0]: { EmployeeName = "Charlie", NetPay = 7293 }
// payroll[1]: { EmployeeName = "Alice",   NetPay = 6400 }
```
