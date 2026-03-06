using PipeScript.NET.Config;
using PipeScript.NET.Steps;

namespace PipeScript.NET.Tests;

public class StepFactoryTests
{
    [Fact]
    public void Create_WhereStep_Success()
    {
        var config = new StepConfig { Type = "Where", Condition = "Price > 0" };
        var step = StepFactory.Create(config);
        Assert.IsType<WhereStep>(step);
    }

    [Fact]
    public void Create_CalculateStep_Success()
    {
        var config = new StepConfig { Type = "Calculate", Field = "Total", Expression = "Price * Qty" };
        var step = StepFactory.Create(config);
        Assert.IsType<CalculateStep>(step);
    }

    [Fact]
    public void Create_OrderByStep_Success()
    {
        var config = new StepConfig { Type = "OrderBy", Field = "Price" };
        var step = StepFactory.Create(config);
        Assert.IsType<OrderByStep>(step);
    }

    [Fact]
    public void Create_TransformStep_Success()
    {
        var config = new StepConfig { Type = "Transform", Mappings = new() { { "NewName", "OldName" } } };
        var step = StepFactory.Create(config);
        Assert.IsType<TransformStep>(step);
    }

    [Fact]
    public void Create_UnknownType_ThrowsNotSupportedException()
    {
        var config = new StepConfig { Type = "Unknown" };
        Assert.Throws<NotSupportedException>(() => StepFactory.Create(config));
    }

    [Fact]
    public void Create_Where_MissingCondition_Throws()
    {
        var config = new StepConfig { Type = "Where" };
        Assert.Throws<InvalidOperationException>(() => StepFactory.Create(config));
    }
}
