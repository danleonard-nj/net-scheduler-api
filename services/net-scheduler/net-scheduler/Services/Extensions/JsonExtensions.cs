namespace NetScheduler.Services.Extensions;
using System.Text.Json;

public static class JsonExtensions
{
    public static string Serialize(this object obj)
    {
        return JsonSerializer.Serialize(obj);
    }
}
