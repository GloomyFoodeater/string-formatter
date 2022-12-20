using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace StringFormatter.Core;

internal class ExpressionCache
{
    private readonly ConcurrentDictionary<string, Func<object, string>> _dictionary = new();

    public string GetString(string dataMemberName, object target)
    {
        // Obtain target metadata
        var type = target.GetType();
        var fields = type.GetFields();
        var properties = type.GetProperties();
        var hasNoDataMember = fields.All(field => field.Name != dataMemberName) &&
                              properties.All(prop => prop.Name != dataMemberName);

        // Validate request
        if (hasNoDataMember)
            throw new ArgumentException("Type '" + type.Name + "' does not contain property or field '" +
                                        dataMemberName + "'");
        
        var key = type.Name + "." + dataMemberName;
        
        // Get existing delegate
        if (_dictionary.TryGetValue(key, out var result)) return result(target);
        
        // Compile delegate to access object data member with reflection  
        var parameter = Expression.Parameter(typeof(object), "obj");
        var propertyOrField = Expression.PropertyOrField(Expression.TypeAs(parameter, type), dataMemberName);
        var call = Expression.Call(propertyOrField, "ToString", null, null);
        var lambda = Expression.Lambda<Func<object, string>>(call, parameter);
        result = lambda.Compile();
        
        // Dictionary can be appended during delegate compilation in another thread
        _dictionary.GetOrAdd(key, result);

        return result(target);
    }
}