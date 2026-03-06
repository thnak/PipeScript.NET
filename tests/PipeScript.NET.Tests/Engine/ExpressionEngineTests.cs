using System.Dynamic;
using PipeScript.NET.Engine;

namespace PipeScript.NET.Tests.Engine;

public class ExpressionEngineTests
{
    private static dynamic MakeObj(string name, double price, int qty)
    {
        dynamic obj = new ExpandoObject();
        obj.Name = name;
        obj.Price = price;
        obj.Quantity = qty;
        return obj;
    }

    [Theory]
    [InlineData("Price > 100", 150.0, true)]
    [InlineData("Price > 100", 50.0, false)]
    [InlineData("Price >= 100", 100.0, true)]
    [InlineData("Price <= 50", 50.0, true)]
    [InlineData("Price == 100", 100.0, true)]
    [InlineData("Price != 100", 50.0, true)]
    public void EvaluateCondition_NumericComparisons(string expr, double price, bool expected)
    {
        var obj = MakeObj("Test", price, 1);
        Assert.Equal(expected, ExpressionEngine.EvaluateCondition(expr, obj));
    }

    [Fact]
    public void EvaluateCondition_StringEquality()
    {
        dynamic obj = new ExpandoObject();
        obj.Category = "Electronics";
        Assert.True(ExpressionEngine.EvaluateCondition("Category == \"Electronics\"", obj));
        Assert.False(ExpressionEngine.EvaluateCondition("Category == \"Books\"", obj));
    }

    [Fact]
    public void EvaluateCondition_LogicalAnd()
    {
        var obj = MakeObj("Test", 150.0, 3);
        Assert.True(ExpressionEngine.EvaluateCondition("Price > 100 && Quantity > 2", obj));
        Assert.False(ExpressionEngine.EvaluateCondition("Price > 100 && Quantity > 5", obj));
    }

    [Fact]
    public void EvaluateCondition_LogicalOr()
    {
        var obj = MakeObj("Test", 50.0, 10);
        Assert.True(ExpressionEngine.EvaluateCondition("Price > 100 || Quantity > 5", obj));
        Assert.False(ExpressionEngine.EvaluateCondition("Price > 100 || Quantity > 15", obj));
    }

    [Fact]
    public void EvaluateValue_Arithmetic()
    {
        var obj = MakeObj("Test", 100.0, 3);
        var result = ExpressionEngine.EvaluateValue("Price * Quantity", obj);
        Assert.Equal(300.0, result);
    }

    [Fact]
    public void EvaluateValue_ComplexExpression()
    {
        var obj = MakeObj("Test", 100.0, 3);
        var result = ExpressionEngine.EvaluateValue("(Price + 10) * Quantity", obj);
        Assert.Equal(330.0, result);
    }

    [Fact]
    public void EvaluateValue_StringConcatenation()
    {
        dynamic obj = new ExpandoObject();
        obj.First = "Hello";
        obj.Last = "World";
        var result = ExpressionEngine.EvaluateValue("First + \" \" + Last", obj);
        Assert.Equal("Hello World", result);
    }
}
