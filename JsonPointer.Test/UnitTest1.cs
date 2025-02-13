using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonPointer.Test;

public class UnitTest1
{
    private record Example(string Value, Example2 Nested, [property: JsonPropertyName("Barrs")] Example3[] Nested2);

    private record Example2(string Value);

    private record Example3(string Value);

    [Fact]
    public void Test1()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var result = JsonPointerHelper.GetJsonPointer<Example>(t => t.Value, options);

        Assert.Equal("/value", result);
    }

    [Fact]
    public void Test2()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var result = JsonPointerHelper.GetJsonPointer<Example>(t => t.Nested.Value, options);

        Assert.Equal("/nested/value", result);
    }

    [Fact]
    public void Test3()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var result = JsonPointerHelper.GetJsonPointer<Example>(t => t.Nested2[0].Value, options);

        Assert.Equal("/Barrs/0/value", result);
    }
}