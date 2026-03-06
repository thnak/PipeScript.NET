using System.Dynamic;
using PipeScript.NET.Steps;

namespace PipeScript.NET.Tests.Steps;

public class OrderByStepTests
{
    private static List<dynamic> MakeItems()
    {
        dynamic a = new ExpandoObject(); a.Name = "C"; a.Price = 300.0;
        dynamic b = new ExpandoObject(); b.Name = "A"; b.Price = 100.0;
        dynamic c = new ExpandoObject(); c.Name = "B"; c.Price = 200.0;
        return new List<dynamic> { a, b, c };
    }

    [Fact]
    public void Execute_SortsByFieldAscending()
    {
        var step = new OrderByStep("Price", "Asc");
        var result = step.Execute(MakeItems()).ToList();
        Assert.Equal(100.0, (double)result[0].Price);
        Assert.Equal(200.0, (double)result[1].Price);
        Assert.Equal(300.0, (double)result[2].Price);
    }

    [Fact]
    public void Execute_SortsByFieldDescending()
    {
        var step = new OrderByStep("Price", "Desc");
        var result = step.Execute(MakeItems()).ToList();
        Assert.Equal(300.0, (double)result[0].Price);
        Assert.Equal(200.0, (double)result[1].Price);
        Assert.Equal(100.0, (double)result[2].Price);
    }

    [Fact]
    public void Execute_DefaultDirectionIsAscending()
    {
        var step = new OrderByStep("Price");
        var result = step.Execute(MakeItems()).ToList();
        Assert.Equal(100.0, (double)result[0].Price);
    }

    [Fact]
    public void Execute_SortsStringField()
    {
        var step = new OrderByStep("Name", "Asc");
        var result = step.Execute(MakeItems()).ToList();
        Assert.Equal("A", (string)result[0].Name);
        Assert.Equal("B", (string)result[1].Name);
        Assert.Equal("C", (string)result[2].Name);
    }
}
