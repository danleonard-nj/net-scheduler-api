namespace NetScheduler.Data.Models;

using MongoDB.Bson.Serialization.Attributes;

public class IdentityClient : MongoBase
{
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("identity_client_id")]
    public string IdentityClientId { get; set; } = null!;

    [BsonElement("client_id")]
    public string ClientId { get; set; } = null!;

    [BsonElement("client_secret")]
    public string ClientSecret { get; set; } = null!;

    [BsonElement("scopes")]
    public string Scopes { get; set; } = null!;

    [BsonElement("grant_type")]
    public string GrantType { get; set; } = null!;
}
