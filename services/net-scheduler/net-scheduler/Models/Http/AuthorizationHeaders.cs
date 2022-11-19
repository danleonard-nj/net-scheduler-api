namespace NetScheduler.Models.Http;

using System.Text.Json;

public class AuthorizationHeaders
{
    public string Authorization { get; set; }

    public AuthorizationHeaders(string authToken)
    {
        Authorization = $"Bearer {authToken}";
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}
