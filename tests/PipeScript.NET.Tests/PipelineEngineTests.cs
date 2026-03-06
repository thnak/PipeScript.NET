using System.Dynamic;
using PipeScript.NET.Core;

namespace PipeScript.NET.Tests;

public class PipelineEngineTests
{
    private static List<dynamic> MakeProducts()
    {
        dynamic p1 = new ExpandoObject(); p1.Name = "Laptop"; p1.Price = 1200.0; p1.Quantity = 2; p1.Category = "Electronics";
        dynamic p2 = new ExpandoObject(); p2.Name = "Book"; p2.Price = 20.0; p2.Quantity = 5; p2.Category = "Books";
        dynamic p3 = new ExpandoObject(); p3.Name = "Phone"; p3.Price = 800.0; p3.Quantity = 3; p3.Category = "Electronics";
        dynamic p4 = new ExpandoObject(); p4.Name = "Tablet"; p4.Price = 500.0; p4.Quantity = 1; p4.Category = "Electronics";
        return new List<dynamic> { p1, p2, p3, p4 };
    }

    [Fact]
    public void FromJson_CreatesAndExecutesPipeline()
    {
        const string json = """
        {
          "steps": [
            { "type": "Where", "condition": "Price > 100" },
            { "type": "Calculate", "field": "Total", "expression": "Price * Quantity" },
            { "type": "OrderBy", "field": "Total", "direction": "Desc" }
          ]
        }
        """;

        var engine = PipelineEngine.FromJson(json);
        var result = engine.Execute(MakeProducts()).ToList();

        // Book (20) is filtered out
        Assert.Equal(3, result.Count);
        // First should have highest total
        Assert.True((double)result[0].Total >= (double)result[result.Count - 1].Total);
    }

    [Fact]
    public void FromJson_FullPipeline_WithTransform()
    {
        const string json = """
        {
          "steps": [
            { "type": "Where", "condition": "Category == \"Electronics\"" },
            { "type": "Calculate", "field": "Total", "expression": "Price * Quantity" },
            { "type": "Transform", "mappings": { "ProductName": "Name", "FinalTotal": "Total" } }
          ]
        }
        """;

        var engine = PipelineEngine.FromJson(json);
        var result = engine.Execute(MakeProducts()).ToList();

        Assert.Equal(3, result.Count);
        foreach (var item in result)
        {
            var dict = (IDictionary<string, object>)item;
            Assert.True(dict.ContainsKey("ProductName"));
            Assert.True(dict.ContainsKey("FinalTotal"));
        }
    }

    [Fact]
    public void Execute_EmptyData_ReturnsEmpty()
    {
        const string json = """{"steps": [{"type": "Where", "condition": "Price > 0"}]}""";
        var engine = PipelineEngine.FromJson(json);
        var result = engine.Execute(Enumerable.Empty<dynamic>()).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void Execute_EmptySteps_ReturnsAllData()
    {
        const string json = """{"steps": []}""";
        var engine = PipelineEngine.FromJson(json);
        var result = engine.Execute(MakeProducts()).ToList();
        Assert.Equal(4, result.Count);
    }
}
