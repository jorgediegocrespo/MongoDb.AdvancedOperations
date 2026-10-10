using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDb.AdvancedOperation;

public sealed record User
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("name")]
    public required string Name { get; init; }

    [BsonElement("email")]
    public required string Email { get; init; }

    [BsonElement("role")]
    public required string Role { get; init; }

    [BsonElement("status")]
    public required string Status { get; init; }

    [BsonElement("country")]
    public required string Country { get; init; }

    [BsonElement("loginCount")]
    public int LoginCount { get; init; }

    [BsonElement("updatedAt")]
    [BsonIgnoreIfDefault]
    public DateTime? UpdatedAt { get; init; }

    [BsonElement("legacyCode")]
    [BsonIgnoreIfDefault]
    public string? LegacyCode { get; init; }

    [BsonElement("legacyId")]
    [BsonIgnoreIfDefault]
    public string? LegacyId { get; init; }

    [BsonElement("riskLevel")]
    [BsonIgnoreIfDefault]
    public string? RiskLevel { get; init; }

    [BsonElement("eTag")]
    public string ETag { get; init; } = NewETag();

    [BsonElement("sessions")]
    public LoginSession[] Sessions { get; init; } = [];

    [BsonElement("fastestSessionSeconds")]
    [BsonIgnoreIfDefault]
    public int FastestSessionSeconds { get; init; }

    public static string NewETag() => Guid.NewGuid().ToString("N");
}
