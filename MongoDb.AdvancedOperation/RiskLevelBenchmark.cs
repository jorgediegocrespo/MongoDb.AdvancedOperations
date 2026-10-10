using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDb.AdvancedOperation;

public sealed class RiskLevelBenchmark(IMongoDatabase database)
{
    private const string CollectionName = "users_benchmark";
    private static readonly string[] Devices = ["desktop", "mobile", "tablet"];
    private static readonly string[] Countries = ["ES", "MX", "AR", "CO", "CL"];

    private IMongoCollection<User> Users => database.GetCollection<User>(CollectionName);

    public async Task RunAsync(int userCount, int sessionsPerUser, int runs)
    {
        Console.WriteLine(
            $"Benchmark: {userCount} users, {sessionsPerUser} sessions per user, " +
            $"{runs} runs per scenario (median shown).");

        var scenarios = new (string Name, Func<Task> Run)[]
        {
            ("Full document + individual ReplaceOne operations", FullDocumentOneByOneAsync),
            ("Full document + BulkWrite of ReplaceOne operations", FullDocumentBulkAsync),
            ("Projection + individual partial UpdateOne operations", ProjectionOneByOneAsync),
            ("Projection + BulkWrite of partial UpdateOne operations", ProjectionBulkAsync)
        };

        // Warm-up: connection, JIT, and MongoDB cache.
        await SeedAsync(userCount, sessionsPerUser);
        await ProjectionBulkAsync();

        var results = new List<(string Name, Measurement Measurement)>();
        foreach (var (name, run) in scenarios)
        {
            var measurements = new List<Measurement>();
            for (var i = 0; i < runs; i++)
            {
                await SeedAsync(userCount, sessionsPerUser);
                measurements.Add(await MeasureAsync(run));
            }

            results.Add((name, Measurement.Median(measurements)));
        }

        var stats = await database.RunCommandAsync<BsonDocument>(
            new BsonDocument("collStats", CollectionName));
        Console.WriteLine(
            $"Average document size: {stats["avgObjSize"].ToDouble() / 1024:F1} KB, " +
            $"collection size: {stats["size"].ToDouble() / 1024 / 1024:F1} MB.");
        Console.WriteLine();
        Console.WriteLine(
            "| Scenario | Time (ms) | Received by MongoDB (MB) | Sent by MongoDB (MB) | Allocated memory in .NET (MB) |");
        Console.WriteLine("|---|---:|---:|---:|---:|");
        foreach (var (name, m) in results)
        {
            Console.WriteLine(
                $"| {name} | {m.ElapsedMilliseconds:N0} | {ToMb(m.BytesIn):F1} | " +
                $"{ToMb(m.BytesOut):F1} | {ToMb(m.AllocatedBytes):F1} |");
        }

        await database.DropCollectionAsync(CollectionName);
    }

    private async Task FullDocumentOneByOneAsync()
    {
        var users = await Users.Find(FilterDefinition<User>.Empty).ToListAsync();

        foreach (var user in users)
        {
            var updatedUser = WithRiskLevel(user);
            await Users.ReplaceOneAsync(u => u.Id == user.Id, updatedUser);
        }
    }

    private async Task FullDocumentBulkAsync()
    {
        var users = await Users.Find(FilterDefinition<User>.Empty).ToListAsync();

        var operations = users
            .Select(user => new ReplaceOneModel<User>(
                Builders<User>.Filter.Eq(u => u.Id, user.Id),
                WithRiskLevel(user)))
            .ToList();

        await Users.BulkWriteAsync(operations, new BulkWriteOptions { IsOrdered = false });
    }

    private async Task ProjectionOneByOneAsync()
    {
        var snapshots = await FindSnapshotsAsync();

        foreach (var snapshot in snapshots)
        {
            await Users.UpdateOneAsync(BuildETagFilter(snapshot), BuildRiskUpdate(snapshot));
        }
    }

    private async Task ProjectionBulkAsync()
    {
        var snapshots = await FindSnapshotsAsync();

        var operations = snapshots
            .Select(snapshot => new UpdateOneModel<User>(
                BuildETagFilter(snapshot),
                BuildRiskUpdate(snapshot)))
            .ToList();

        var result = await Users.BulkWriteAsync(
            operations,
            new BulkWriteOptions { IsOrdered = false });

        if (result.MatchedCount != operations.Count)
        {
            Console.WriteLine(
                $"  {operations.Count - result.MatchedCount} documents changed during " +
                "processing and would need to be retried.");
        }
    }

    private Task<List<UserRiskSnapshot>> FindSnapshotsAsync()
    {
        return Users
            .Find(FilterDefinition<User>.Empty)
            .Project(user => new UserRiskSnapshot(
                user.Id,
                user.ETag,
                user.Sessions.Max(session => session.RiskScore)))
            .ToListAsync();
    }

    private static User WithRiskLevel(User user)
    {
        return user with
        {
            RiskLevel = RiskRules.GetRiskLevel(user.Sessions.Max(session => session.RiskScore)),
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static FilterDefinition<User> BuildETagFilter(UserRiskSnapshot snapshot)
    {
        return Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(user => user.Id, snapshot.Id),
            Builders<User>.Filter.Eq(user => user.ETag, snapshot.ETag));
    }

    private static UpdateDefinition<User> BuildRiskUpdate(UserRiskSnapshot snapshot)
    {
        return Builders<User>.Update
            .Set(user => user.RiskLevel, RiskRules.GetRiskLevel(snapshot.MaxRiskScore))
            .Set(user => user.ETag, User.NewETag())
            .CurrentDate(user => user.UpdatedAt);
    }

    private async Task<Measurement> MeasureAsync(Func<Task> run)
    {
        var (bytesInBefore, bytesOutBefore) = await GetNetworkBytesAsync();
        var allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);
        var stopwatch = Stopwatch.StartNew();

        await run();

        stopwatch.Stop();
        var allocatedAfter = GC.GetTotalAllocatedBytes(precise: true);
        var (bytesInAfter, bytesOutAfter) = await GetNetworkBytesAsync();

        return new Measurement(
            stopwatch.ElapsedMilliseconds,
            bytesInAfter - bytesInBefore,
            bytesOutAfter - bytesOutBefore,
            allocatedAfter - allocatedBefore);
    }

    private async Task<(long BytesIn, long BytesOut)> GetNetworkBytesAsync()
    {
        var status = await database.RunCommandAsync<BsonDocument>(
            new BsonDocument("serverStatus", 1));
        var network = status["network"].AsBsonDocument;
        return (network["bytesIn"].ToInt64(), network["bytesOut"].ToInt64());
    }

    private async Task SeedAsync(int userCount, int sessionsPerUser)
    {
        await database.DropCollectionAsync(CollectionName);

        var random = new Random(42);
        var startDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        const int batchSize = 1_000;

        for (var offset = 0; offset < userCount; offset += batchSize)
        {
            var batch = Enumerable
                .Range(offset, Math.Min(batchSize, userCount - offset))
                .Select(i => new User
                {
                    Name = $"User {i}",
                    Email = $"user{i}@example.com",
                    Role = i % 10 == 0 ? "admin" : "user",
                    Status = i % 7 == 0 ? "inactive" : "active",
                    Country = Countries[i % Countries.Length],
                    LoginCount = random.Next(0, 500),
                    Sessions = Enumerable
                        .Range(0, sessionsPerUser)
                        .Select(s => new LoginSession
                        {
                            SessionId = $"user{i}-session-{s:D4}",
                            Device = Devices[random.Next(Devices.Length)],
                            StartedAt = startDate.AddMinutes(random.Next(0, 400_000)),
                            DurationSeconds = random.Next(30, 7_200),
                            RiskScore = random.Next(0, 100),
                            Revoked = random.Next(0, 20) == 0
                        })
                        .ToArray()
                });

            await Users.InsertManyAsync(batch);
        }
    }

    private static double ToMb(long bytes) => bytes / 1024d / 1024d;

    private sealed record Measurement(
        long ElapsedMilliseconds,
        long BytesIn,
        long BytesOut,
        long AllocatedBytes)
    {
        public static Measurement Median(IReadOnlyList<Measurement> measurements)
        {
            return new Measurement(
                MedianOf(measurements.Select(m => m.ElapsedMilliseconds)),
                MedianOf(measurements.Select(m => m.BytesIn)),
                MedianOf(measurements.Select(m => m.BytesOut)),
                MedianOf(measurements.Select(m => m.AllocatedBytes)));
        }

        private static long MedianOf(IEnumerable<long> values)
        {
            var sorted = values.Order().ToArray();
            return sorted[sorted.Length / 2];
        }
    }
}
