using MongoDB.Bson;

namespace MongoDb.AdvancedOperation;

public sealed record UserRiskSnapshot(ObjectId Id, string ETag, int MaxRiskScore);
