using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Core;

internal class ExpressionCache
{
    private readonly ConcurrentDictionary<string, Func<object, string>> _dictionary = new();

    public string GetString(string dataMemberName, object target)
    {
        // Obtain target metadata.
        var type = target.GetType();
        var fields = type.GetFields();
        var properties = type.GetProperties();

        // Validate request.
        var hasNoDataMember = fields.All(field => field.Name != dataMemberName) &&
                              properties.All(prop => prop.Name != dataMemberName);
        if (hasNoDataMember)
            throw new ArgumentException("Type '" + type.Name + "' does not contain public property or field '" +
                                        dataMemberName + "'");

        var key = type.Name + "." + dataMemberName;

        // Cache hit.
        // Get existing delegate.
        if (_dictionary.TryGetValue(key, out var result)) return result(target);

        // Cache miss.
        // Compile delegate to access object data member with reflection.  
        var parameter = Expression.Parameter(typeof(object), "obj");
        var propertyOrField = Expression.PropertyOrField(Expression.TypeAs(parameter, type), dataMemberName);
        var call = Expression.Call(propertyOrField, "ToString", null, null);
        var lambda = Expression.Lambda<Func<object, string>>(call, parameter);
        result = lambda.Compile(); // (obj)=>obj.<dataMemberName>.ToString()

        // Dictionary can be appended during delegate compilation in another thread.
        result = _dictionary.GetOrAdd(key, result);

        return result(target);
    }
}