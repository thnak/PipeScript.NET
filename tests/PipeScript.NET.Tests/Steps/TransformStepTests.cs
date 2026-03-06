using System.Dynamic;
using PipeScript.NET.Steps;

namespace PipeScript.NET.Tests.Steps;

public class TransformStepTests
{
    [Fact]
    public void Execute_RenamesFields()
    {
        dynamic item = new ExpandoObject();
        item.Name = "Laptop";
        item.Price = 1200.0;

        var step = new TransformStep(new Dictionary<string, string>
        {
            { "ProductName", "Name" },
            { "ProductPrice", "Price" }
        });

        var result = step.Execute(new[] { item }).ToList();
        Assert.Single(result);
        var dict = (IDictionary<string, object>)result[0];
        Assert.True(dict.ContainsKey("ProductName"));
        Assert.True(dict.ContainsKey("ProductPrice"));
        Assert.Equal("Laptop", dict["ProductName"]);
        Assert.Equal(1200.0, dict["ProductPrice"]);
    }

    [Fact]
    public void Execute_SelectsOnlyMappedFields()
    {
        dynamic item = new ExpandoObject();
        item.Name = "Laptop";
        item.Price = 1200.0;
        item.Category = "Electronics";

        var step = new TransformStep(new Dictionary<string, string>
        {
            { "Name", "Name" }
        });

        var result = step.Execute(new[] { item }).ToList();
        var dict = (IDictionary<string, object>)result[0];
        Assert.Single(dict);
        Assert.True(dict.ContainsKey("Name"));
    }
}
