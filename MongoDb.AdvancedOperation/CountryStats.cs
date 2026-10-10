using MongoDB.Bson.Serialization.Attributes;

namespace MongoDb.AdvancedOperation;

public sealed class CountryStats
{
    [BsonElement("country")]
    public required string Country { get; init; }

    [BsonElement("totalUsers")]
    public int TotalUsers { get; init; }

    [BsonElement("totalLogins")]
    public int TotalLogins { get; init; }
}
