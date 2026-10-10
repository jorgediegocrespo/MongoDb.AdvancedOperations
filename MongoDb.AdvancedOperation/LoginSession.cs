using MongoDB.Bson.Serialization.Attributes;

namespace MongoDb.AdvancedOperation;

public sealed class LoginSession
{
    [BsonElement("sessionId")]
    public required string SessionId { get; init; }

    [BsonElement("device")]
    public required string Device { get; init; }

    [BsonElement("startedAt")]
    public DateTime StartedAt { get; init; }

    [BsonElement("durationSeconds")]
    public int DurationSeconds { get; init; }

    [BsonElement("riskScore")]
    public int RiskScore { get; init; }

    [BsonElement("revoked")]
    public bool Revoked { get; init; }
}
