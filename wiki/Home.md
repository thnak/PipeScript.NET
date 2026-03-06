# PipeScript.NET Wiki

Welcome to the PipeScript.NET wiki. Here you'll find real-life use cases, examples, and best practices for building JSON-configurable data pipelines.

## Use Cases

| Page | Description |
|------|-------------|
| [Sales Report](./Sales-Report.md) | Filter products, compute revenue totals, sort and rename for export |
| [Payroll Processing](./Payroll-Processing.md) | Calculate gross pay, apply deductions, and summarise department costs |
| [Inventory Management](./Inventory-Management.md) | Identify low-stock items and estimate restocking cost |
| [Order Processing](./Order-Processing.md) | Enrich active orders with totals, apply tax, and prioritise by value |

## Quick Reference

- Each pipeline is expressed as a JSON object with a `steps` array.
- Steps execute in order; the output of one step becomes the input of the next.
- Supported step types: `Where`, `Calculate`, `OrderBy`, `Transform`.

See the [README](../README.md) for full API documentation and expression syntax.
