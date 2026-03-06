using System.Dynamic;

namespace PipeScript.NET.Tests.Helpers;

public static class DynamicHelper
{
    public static dynamic CreateProduct(string name, string category, double price, int quantity)
    {
        dynamic obj = new ExpandoObject();
        obj.Name = name;
        obj.Category = category;
        obj.Price = price;
        obj.Quantity = quantity;
        return obj;
    }
}
