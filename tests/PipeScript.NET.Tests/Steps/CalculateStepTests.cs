using System.Dynamic;
using PipeScript.NET.Steps;

namespace PipeScript.NET.Tests.Steps;

public class CalculateStepTests
{
    [Fact]
    public void Execute_AddsCalculatedField()
    {
        dynamic item = new ExpandoObject();
        item.Price = 100.0;
        item.Quantity = 3;

        var step = new CalculateStep("Total", "Price * Quantity");
        var result = step.Execute(new[] { item }).ToList();

        Assert.Single(result);
        Assert.Equal(300.0, (double)result[0].Total);
    }

    [Fact]
    public void Execute_UpdatesExistingField()
    {
        dynamic item = new ExpandoObject();
        item.Price = 100.0;
        item.Discount = 0.1;

        var step = new CalculateStep("Price", "Price * (1 - Discount)");
        var result = step.Execute(new[] { item }).ToList();

        Assert.Equal(90.0, (double)result[0].Price, 6);
    }

    [Fact]
    public void Execute_MultipleItems()
    {
        dynamic item1 = new ExpandoObject(); item1.Price = 100.0; item1.Quantity = 2;
        dynamic item2 = new ExpandoObject(); item2.Price = 50.0; item2.Quantity = 4;

        var step = new CalculateStep("Total", "Price * Quantity");
        var result = step.Execute(new[] { item1, item2 }).ToList();

        Assert.Equal(200.0, (double)result[0].Total);
        Assert.Equal(200.0, (double)result[1].Total);
    }
}
