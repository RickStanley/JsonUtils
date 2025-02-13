# JSON utils

Examples:

```csharp
var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var result = JsonPointerHelper.GetJsonPointer<Example>(t => t.Value, options); // /value

var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var result = JsonPointerHelper.GetJsonPointer<Example>(t => t.Nested.Value, options); // /nested/value

var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var result = JsonPointerHelper.GetJsonPointer<Example>(t => t.Nested2[0].Value, options); // /Barrs/0/value
```
