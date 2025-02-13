using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonPointer;

public static class JsonPointerHelper
{
    private static string GetSerializedPropertyName(MemberInfo member, JsonSerializerOptions? options)
    {
        var jsonProperty = member.GetCustomAttribute<JsonPropertyNameAttribute>();
        if (jsonProperty != null)
        {
            return jsonProperty.Name;
        }

        // Apply the naming policy if set
        var propertyName = member.Name;
        return options?.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;
    }

    public static string GetJsonPointer<T>(Expression<Func<T, object>> expression,
        JsonPointerRepresentation pointerRepresentation)
    {
        if (pointerRepresentation == JsonPointerRepresentation.Normal)
            throw new NotSupportedException("Normal");

        var pathParts = GetJsonPointerParts(expression, null);

        if (pathParts.Count == 0)
            return pointerRepresentation == JsonPointerRepresentation.UriFragment ? "#" : "";

        var result = pointerRepresentation == JsonPointerRepresentation.UriFragment
            ? "#/" + string.Join('/', pathParts.Select(Uri.EscapeDataString))
            : "/" + string.Join('/', pathParts);

        return result;
    }

    public static string GetJsonPointer<T>(Expression<Func<T, object>> expression,
        JsonSerializerOptions? options = null)
    {
        var pathParts = GetJsonPointerParts(expression, options);
        return "/" + string.Join("/", pathParts);
    }

    private static Stack<string> GetJsonPointerParts<T>(Expression<Func<T, object>> expression,
        JsonSerializerOptions? options)
    {
        const string safetyFallback = "INVALID_EXPRESSION";
        Stack<string> pathParts = new();

        var currentExpression = expression.Body;
        while (currentExpression is not ParameterExpression)
        {
            switch (currentExpression)
            {
                case MemberExpression memberExpression:
                    // Member access: fetch serialized name and pop
                    pathParts.Push(GetSerializedPropertyName(memberExpression.Member, options));
                    currentExpression = memberExpression.Expression;
                    break;
                case BinaryExpression { Right: ConstantExpression arrayIndexConstantExpression } binaryExpression and
                    { NodeType: ExpressionType.ArrayIndex }:
                    var item = arrayIndexConstantExpression.Value?.ToString();
                    if (item is null)
                        return new Stack<string>([safetyFallback]);
                    // Array index
                    pathParts.Push(item);
                    currentExpression = binaryExpression.Left;
                    break;
                case MethodCallExpression
                    {
                        Arguments:
                        [
                            ConstantExpression
                            {
                                Type.Name: nameof(Int32)
                            } listIndexConstantExpression
                        ],
                        Method.Name: "get_Item"
                    } callExpression
                    when callExpression.Method.DeclaringType.GetInterfaces().Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)):
                    // IReadOnlyList index of other type
                    var index = listIndexConstantExpression.Value?.ToString();
                    if (index is null)
                        return new Stack<string>([safetyFallback]);
                    pathParts.Push(index);
                    currentExpression = callExpression.Object;
                    break;
                default:
                    throw new InvalidOperationException(
                        $"{currentExpression?.GetType().Name} (at {currentExpression}) not supported");
            }
        }

        return pathParts;
    }
}

/// <summary>
/// Values that specify the representation of a JSON pointer.
/// </summary>
public enum JsonPointerRepresentation
{
    /// <summary>
    /// The representation specified in RFC 6901, Sec. 3.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// The JSON string representation specified in RFC 6901, Sec. 5.
    /// </summary>
    JsonString,

    /// <summary>
    /// The URI fragment identifier representation specified in RFC 6901, Sec. 6.
    /// </summary>
    UriFragment
}