namespace NetScheduler.Clients.Models;

using Newtonsoft.Json;
using System.Text.Json.Serialization;

public class IdentityResponse
{
    [JsonPropertyName("access_token")]
    [JsonProperty("access_token")]
    public string Token { get; set; }
}

