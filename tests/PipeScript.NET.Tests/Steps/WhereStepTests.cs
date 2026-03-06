using System.Dynamic;
using PipeScript.NET.Steps;

namespace PipeScript.NET.Tests.Steps;

public class WhereStepTests
{
    private static List<dynamic> MakeProducts()
    {
        dynamic p1 = new ExpandoObject(); p1.Name = "Laptop"; p1.Price = 1200.0; p1.Category = "Electronics";
        dynamic p2 = new ExpandoObject(); p2.Name = "Book"; p2.Price = 20.0; p2.Category = "Books";
        dynamic p3 = new ExpandoObject(); p3.Name = "Phone"; p3.Price = 800.0; p3.Category = "Electronics";
        return new List<dynamic> { p1, p2, p3 };
    }

    [Fact]
    public void Execute_FiltersCorrectly()
    {
        var step = new WhereStep("Price > 100");
        var result = step.Execute(MakeProducts()).ToList();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Execute_WithStringCondition()
    {
        var step = new WhereStep("Category == \"Electronics\"");
        var result = step.Execute(MakeProducts()).ToList();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Execute_WithAndCondition()
    {
        var step = new WhereStep("Price > 100 && Category == \"Electronics\"");
        var result = step.Execute(MakeProducts()).ToList();
        Assert.Equal(2, result.Count);
        Assert.All(result, item => Assert.Equal("Electronics", (string)item.Category));
    }

    [Fact]
    public void Execute_NoMatch_ReturnsEmpty()
    {
        var step = new WhereStep("Price > 9999");
        var result = step.Execute(MakeProducts()).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void Constructor_EmptyCondition_Throws()
    {
        Assert.Throws<ArgumentException>(() => new WhereStep(""));
    }
}
