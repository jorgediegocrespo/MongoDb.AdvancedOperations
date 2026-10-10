using MongoDB.Driver;
using MongoDb.AdvancedOperation;

var connectionString = Environment.GetEnvironmentVariable("MONGODB_URI")
    ?? "mongodb://localhost:27017";
var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE")
    ?? "advanced_operations";

var client = new MongoClient(connectionString);
var database = client.GetDatabase(databaseName);
var examples = new MongoDbExamples(database);
var command = args.FirstOrDefault()?.ToLowerInvariant() ?? "all";

try
{
    switch (command)
    {
        case "seed":
            await examples.SeedAsync();
            break;
        case "projection":
            await examples.ProjectionAsync();
            break;
        case "partial-update":
            await examples.PartialUpdateAsync();
            break;
        case "update-operators":
            await examples.UpdateOperatorsAsync();
            break;
        case "conditional-update":
            await examples.ConditionalUpdateAsync();
            break;
        case "concurrency":
            await examples.ConcurrencyAsync();
            break;
        case "bulk":
            await examples.BulkWriteAsync();
            break;
        case "aggregate":
            await examples.AggregateAsync();
            break;
        case "explain":
            await examples.ExplainAggregationAsync();
            break;
        case "benchmark":
            var userCount = args.Length > 1 ? int.Parse(args[1]) : 10_000;
            var sessionsPerUser = args.Length > 2 ? int.Parse(args[2]) : 50;
            var runs = args.Length > 3 ? int.Parse(args[3]) : 3;
            await new RiskLevelBenchmark(database).RunAsync(userCount, sessionsPerUser, runs);
            break;
        case "all":
            await examples.RunAllAsync();
            break;
        default:
            Console.Error.WriteLine(
                "Invalid command. Use: seed, projection, partial-update, update-operators, " +
                "conditional-update, concurrency, bulk, aggregate, explain, benchmark or all.");
            Environment.ExitCode = 1;
            break;
    }
}
catch (MongoException exception)
{
    Console.Error.WriteLine($"MongoDB rejected the operation: {exception.Message}");
    Environment.ExitCode = 1;
}
