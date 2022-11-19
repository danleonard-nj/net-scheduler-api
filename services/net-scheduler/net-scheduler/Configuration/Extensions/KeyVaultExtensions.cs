namespace NetScheduler.Configuration.Extensions;
using MongoDB.Driver;
using NetScheduler.Configuration.Attributes;
using System.Reflection;

public static class KeyVaultExtensions
{
    public static T InjectSecrets<T>(this T obj, IDictionary<string, string> keyValuePairs)
        where T : class
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        var props = obj.GetType().GetProperties();

        foreach (var prop in props)
        {
            if (Attribute.IsDefined(prop, typeof(KeyVaultSecretAttribute)))
            {
                var secretName = prop.GetCustomAttribute<KeyVaultSecretAttribute>()!.Secret;

                if (keyValuePairs.TryGetValue(secretName, out var secret))
                {
                    prop.SetValue(obj, secret);

                }
            }
        }

        return obj;
    }

    public static bool HasKeyVaultAttributes(this object obj)
    {
        return obj.GetType()
            .GetProperties()
            .Any(prop => prop
            .GetCustomAttribute<KeyVaultSecretAttribute>() != null);
    }
}
