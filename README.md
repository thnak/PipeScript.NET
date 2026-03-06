# PipeScript.NET

A lightweight, high-performance .NET 8 library for building JSON-configurable data processing pipelines. Define multi-step transformations using a simple declarative syntax — no code changes needed when business logic evolves.

## Features

- **JSON-driven pipelines** — define pipelines in JSON config files or strings
- **Built-in expression engine** — arithmetic, comparison, logical and string operations
- **AST caching** — parsed expressions are cached for high-throughput scenarios
- **DI-friendly** — integrates with `Microsoft.Extensions.DependencyInjection`
- **Composable steps** — `Where`, `Calculate`, `OrderBy`, `Transform`
- **ExpandoObject-native** — works seamlessly with dynamic data

---

## Installation

Add the project reference or reference the compiled library:

```bash
dotnet add reference src/PipeScript.NET/PipeScript.NET.csproj
```

---

## Quick Start

```csharp
using System.Dynamic;
using PipeScript.NET.Core;

// Build some dynamic data
var products = new List<dynamic>();
dynamic p1 = new ExpandoObject(); p1.Name = "Laptop"; p1.Price = 1200.0; p1.Quantity = 2; p1.Category = "Electronics";
dynamic p2 = new ExpandoObject(); p2.Name = "Book";   p2.Price = 20.0;   p2.Quantity = 5; p2.Category = "Books";
dynamic p3 = new ExpandoObject(); p3.Name = "Phone";  p3.Price = 800.0;  p3.Quantity = 3; p3.Category = "Electronics";
products.AddRange(new[] { p1, p2, p3 });

// Define pipeline from JSON
const string json = """
{
  "steps": [
    { "type": "Where",     "condition":  "Category == \"Electronics\"" },
    { "type": "Calculate", "field": "Total", "expression": "Price * Quantity" },
    { "type": "OrderBy",   "field": "Total", "direction": "Desc" }
  ]
}
""";

var engine = PipelineEngine.FromJson(json);
var result = engine.Execute(products).ToList();

// result: [{ Name: "Laptop", Total: 2400 }, { Name: "Phone", Total: 2400 }]
```

---

## Pipeline Step Reference

### Where

Filters records using a boolean expression.

```json
{ "type": "Where", "condition": "Price > 100" }
{ "type": "Where", "condition": "Category == \"Electronics\" && Quantity > 1" }
```

| Property    | Required | Description                      |
|-------------|----------|----------------------------------|
| `condition` | ✅        | Boolean expression to evaluate   |

---

### Calculate

Adds or updates a field using an expression.

```json
{ "type": "Calculate", "field": "Total", "expression": "Price * Quantity" }
{ "type": "Calculate", "field": "Discounted", "expression": "Price * (1 - Discount)" }
```

| Property     | Required | Description                                  |
|--------------|----------|----------------------------------------------|
| `field`      | ✅        | Name of the field to create or update        |
| `expression` | ✅        | Value expression to evaluate and assign      |

---

### OrderBy

Sorts the dataset by a field.

```json
{ "type": "OrderBy", "field": "Price", "direction": "Asc" }
{ "type": "OrderBy", "field": "Total", "direction": "Desc" }
```

| Property    | Required | Default | Description                   |
|-------------|----------|---------|-------------------------------|
| `field`     | ✅        |         | Field to sort by              |
| `direction` | ❌        | `Asc`   | `Asc` or `Desc`               |

---

### Transform

Projects and/or renames fields into a new shape. Only the mapped fields are included in the output.

```json
{
  "type": "Transform",
  "mappings": {
    "ProductName": "Name",
    "FinalTotal":  "Total"
  }
}
```

| Property   | Required | Description                                                   |
|------------|----------|---------------------------------------------------------------|
| `mappings` | ✅        | Object mapping `{ newFieldName: sourceFieldOrExpression }`    |

---

## C# API

### PipelineEngine.FromJson

```csharp
var engine = PipelineEngine.FromJson(jsonString);
var results = engine.Execute(data);
```

### PipelineEngine.FromConfig

```csharp
var config = new PipelineConfig
{
    Steps = new List<StepConfig>
    {
        new StepConfig { Type = "Where", Condition = "Price > 100" },
        new StepConfig { Type = "OrderBy", Field = "Price", Direction = "Desc" }
    }
};
var engine = PipelineEngine.FromConfig(config);
var results = engine.Execute(data);
```

### Direct Step Usage

```csharp
using PipeScript.NET.Steps;

var step = new WhereStep("Price > 100");
var filtered = step.Execute(data);

var calc = new CalculateStep("Total", "Price * Quantity");
var withTotal = calc.Execute(filtered);
```

---

## Expression Syntax

Expressions support the following constructs:

### Arithmetic

| Operator | Description    | Example             |
|----------|----------------|---------------------|
| `+`      | Add / Concat   | `Price + 10`        |
| `-`      | Subtract       | `Price - Discount`  |
| `*`      | Multiply       | `Price * Quantity`  |
| `/`      | Divide         | `Total / Count`     |
| `%`      | Modulo         | `Index % 2`         |

### Comparison

| Operator | Description          |
|----------|----------------------|
| `==`     | Equal                |
| `!=`     | Not equal            |
| `>`      | Greater than         |
| `<`      | Less than            |
| `>=`     | Greater or equal     |
| `<=`     | Less or equal        |

### Logical

| Operator     | Aliases   | Description |
|--------------|-----------|-------------|
| `&&`         | `and`     | Logical AND |
| `\|\|`       | `or`      | Logical OR  |
| `!`          | `not`     | Logical NOT |

### Literals

| Type    | Example                        |
|---------|--------------------------------|
| Number  | `42`, `3.14`, `-7`             |
| String  | `"hello"`, `'world'`           |
| Boolean | `true`, `false`                |
| Null    | `null`                         |

### String Concatenation

When `+` is used with a string on either side, it performs concatenation:

```
"Hello" + " " + Name   →  "Hello Alice"
```

---

## Dependency Injection

```csharp
using PipeScript.NET.Extensions;

builder.Services.AddPipelineFromJson("""
{
  "steps": [
    { "type": "Where", "condition": "Active == true" },
    { "type": "OrderBy", "field": "Name" }
  ]
}
""");

// Then inject PipelineEngine into your services:
public class MyService(PipelineEngine engine)
{
    public IEnumerable<dynamic> Process(IEnumerable<dynamic> data) => engine.Execute(data);
}
```

---

## Architecture Overview

```
PipeScript.NET
├── Core/
│   ├── IPipelineStep.cs        # Step interface
│   └── PipelineEngine.cs       # Orchestrates steps; JSON/config factory
├── Config/
│   ├── PipelineConfig.cs       # Top-level JSON model
│   └── StepConfig.cs           # Per-step JSON model
├── Engine/
│   ├── Token.cs                # Token types and Token class
│   ├── Tokenizer.cs            # Lexer: string → token list
│   ├── ExpressionNode.cs       # AST nodes (Literal, Property, Unary, Binary)
│   ├── ExpressionParser.cs     # Recursive-descent parser
│   └── ExpressionEngine.cs     # Public API + AST cache
├── Steps/
│   ├── WhereStep.cs            # Filter step
│   ├── CalculateStep.cs        # Field computation step
│   ├── OrderByStep.cs          # Sort step
│   ├── TransformStep.cs        # Projection/rename step
│   └── StepFactory.cs          # Creates steps from StepConfig
└── Extensions/
    └── ServiceCollectionExtensions.cs  # DI helpers
```

### Expression Engine

The expression engine is a hand-written recursive-descent parser that produces an AST and caches the result per expression string in a `ConcurrentDictionary`. This means each unique expression string is parsed exactly once, regardless of how many rows are processed or how many threads are running.

Evaluation is pure — it does not mutate the AST — making the cached nodes safe for concurrent use.
