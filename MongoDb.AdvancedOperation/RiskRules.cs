namespace MongoDb.AdvancedOperation;

public static class RiskRules
{
    public static string GetRiskLevel(int maxRiskScore) => maxRiskScore switch
    {
        >= 80 => "high",
        >= 40 => "medium",
        _ => "low"
    };
}
