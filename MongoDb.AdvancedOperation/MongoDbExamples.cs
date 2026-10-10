using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDb.AdvancedOperation;

public sealed class MongoDbExamples(IMongoDatabase database)
{
    private const string CollectionName = "users";

    private IMongoCollection<User> Users => database.GetCollection<User>(CollectionName);

    public async Task RunAllAsync()
    {
        await SeedAsync();
        await ProjectionAsync();
        await PartialUpdateAsync();
        await UpdateOperatorsAsync();
        await ConditionalUpdateAsync();
        await ConcurrencyAsync();
        await BulkWriteAsync();
        await AggregateAsync();
        await ExplainAggregationAsync();
    }

    public async Task SeedAsync()
    {
        await database.DropCollectionAsync(CollectionName);

        var users = new[]
        {
            new User
            {
                Name = "Jorge",
                Email = "jorge@dicres.dev",
                Role = "admin",
                Status = "inactive",
                Country = "ES",
                LoginCount = 5,
                LegacyCode = "JORGE-OLD",
                FastestSessionSeconds = 180,
                Sessions =
                [
                    new LoginSession
                    {
                        SessionId = "desktop-001",
                        Device = "desktop",
                        StartedAt = new DateTime(2026, 9, 20, 8, 30, 0, DateTimeKind.Utc),
                        DurationSeconds = 180,
                        RiskScore = 20
                    },
                    new LoginSession
                    {
                        SessionId = "mobile-002",
                        Device = "mobile",
                        StartedAt = new DateTime(2026, 9, 21, 18, 15, 0, DateTimeKind.Utc),
                        DurationSeconds = 240,
                        RiskScore = 40
                    },
                    new LoginSession
                    {
                        SessionId = "tablet-legacy",
                        Device = "tablet",
                        StartedAt = new DateTime(2026, 8, 10, 10, 0, 0, DateTimeKind.Utc),
                        DurationSeconds = 600,
                        RiskScore = 90,
                        Revoked = true
                    }
                ]
            },
            new User
            {
                Name = "Laura",
                Email = "laura@example.com",
                Role = "user",
                Status = "active",
                Country = "ES",
                LoginCount = 12,
                LegacyCode = "LAURA-OLD",
                Sessions =
                [
                    new LoginSession
                    {
                        SessionId = "laura-mob-01",
                        Device = "mobile",
                        StartedAt = new DateTime(2026, 9, 22, 10, 0, 0, DateTimeKind.Utc),
                        DurationSeconds = 300,
                        RiskScore = 10
                    },
                    new LoginSession
                    {
                        SessionId = "laura-desk-02",
                        Device = "desktop",
                        StartedAt = new DateTime(2026, 9, 23, 14, 20, 0, DateTimeKind.Utc),
                        DurationSeconds = 1200,
                        RiskScore = 15
                    },
                    new LoginSession
                    {
                        SessionId = "laura-desk-03",
                        Device = "desktop",
                        StartedAt = new DateTime(2026, 9, 24, 9, 15, 0, DateTimeKind.Utc),
                        DurationSeconds = 900,
                        RiskScore = 5
                    },
                    new LoginSession
                    {
                        SessionId = "laura-mob-04",
                        Device = "mobile",
                        StartedAt = new DateTime(2026, 9, 25, 21, 45, 0, DateTimeKind.Utc),
                        DurationSeconds = 150,
                        RiskScore = 10
                    }
                ]
            },
            new User
            {
                Name = "Pepe",
                Email = "pepe@example.com",
                Role = "guest",
                Status = "active",
                Country = "MX",
                LoginCount = 2,
                Sessions =
                [
                    new LoginSession
                    {
                        SessionId = "pepe-web-01",
                        Device = "desktop",
                        StartedAt = new DateTime(2026, 9, 15, 11, 0, 0, DateTimeKind.Utc),
                        DurationSeconds = 450,
                        RiskScore = 30
                    },
                    new LoginSession
                    {
                        SessionId = "pepe-mob-02",
                        Device = "mobile",
                        StartedAt = new DateTime(2026, 9, 18, 16, 30, 0, DateTimeKind.Utc),
                        DurationSeconds = 180,
                        RiskScore = 25
                    }
                ]
            },
            new User
            {
                Name = "Ana",
                Email = "ana@example.com",
                Role = "user",
                Status = "banned",
                Country = "AR",
                LoginCount = 0,
                Sessions =
                [
                    new LoginSession
                    {
                        SessionId = "ana-blocked-01",
                        Device = "desktop",
                        StartedAt = new DateTime(2026, 7, 5, 12, 0, 0, DateTimeKind.Utc),
                        DurationSeconds = 60,
                        RiskScore = 95,
                        Revoked = true
                    }
                ]
            },
            new User
            {
                Name = "Carlos",
                Email = "carlos@example.com",
                Role = "user",
                Status = "active",
                Country = "ES",
                LoginCount = 15,
                Sessions =
                [
                    new LoginSession
                    {
                        SessionId = "carlos-desk-01",
                        Device = "desktop",
                        StartedAt = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc),
                        DurationSeconds = 3600,
                        RiskScore = 5
                    },
                    new LoginSession
                    {
                        SessionId = "carlos-desk-02",
                        Device = "desktop",
                        StartedAt = new DateTime(2026, 9, 2, 8, 30, 0, DateTimeKind.Utc),
                        DurationSeconds = 2800,
                        RiskScore = 10
                    },
                    new LoginSession
                    {
                        SessionId = "carlos-tab-03",
                        Device = "tablet",
                        StartedAt = new DateTime(2026, 9, 5, 19, 0, 0, DateTimeKind.Utc),
                        DurationSeconds = 600,
                        RiskScore = 15
                    },
                    new LoginSession
                    {
                        SessionId = "carlos-mob-04",
                        Device = "mobile",
                        StartedAt = new DateTime(2026, 9, 10, 22, 10, 0, DateTimeKind.Utc),
                        DurationSeconds = 300,
                        RiskScore = 20
                    },
                    new LoginSession
                    {
                        SessionId = "carlos-desk-05",
                        Device = "desktop",
                        StartedAt = new DateTime(2026, 9, 20, 15, 0, 0, DateTimeKind.Utc),
                        DurationSeconds = 1800,
                        RiskScore = 5
                    }
                ]
            },
            new User
            {
                Name = "Lucía",
                Email = "lucia@example.com",
                Role = "user",
                Status = "active",
                Country = "MX",
                LoginCount = 11,
                Sessions =
                [
                    new LoginSession
                    {
                        SessionId = "lucia-mob-01",
                        Device = "mobile",
                        StartedAt = new DateTime(2026, 9, 10, 9, 0, 0, DateTimeKind.Utc),
                        DurationSeconds = 500,
                        RiskScore = 10
                    },
                    new LoginSession
                    {
                        SessionId = "lucia-mob-02",
                        Device = "mobile",
                        StartedAt = new DateTime(2026, 9, 12, 11, 30, 0, DateTimeKind.Utc),
                        DurationSeconds = 420,
                        RiskScore = 10
                    },
                    new LoginSession
                    {
                        SessionId = "lucia-desk-03",
                        Device = "desktop",
                        StartedAt = new DateTime(2026, 9, 15, 16, 0, 0, DateTimeKind.Utc),
                        DurationSeconds = 2100,
                        RiskScore = 5
                    },
                    new LoginSession
                    {
                        SessionId = "lucia-tab-04",
                        Device = "tablet",
                        StartedAt = new DateTime(2026, 9, 18, 20, 15, 0, DateTimeKind.Utc),
                        DurationSeconds = 800,
                        RiskScore = 15
                    },
                    new LoginSession
                    {
                        SessionId = "lucia-mob-05",
                        Device = "mobile",
                        StartedAt = new DateTime(2026, 9, 22, 8, 45, 0, DateTimeKind.Utc),
                        DurationSeconds = 350,
                        RiskScore = 5
                    },
                    new LoginSession
                    {
                        SessionId = "lucia-desk-06",
                        Device = "desktop",
                        StartedAt = new DateTime(2026, 9, 26, 17, 30, 0, DateTimeKind.Utc),
                        DurationSeconds = 1500,
                        RiskScore = 10
                    }
                ]
            }
        };

        await Users.InsertManyAsync(users);

        var emailIndex = new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(user => user.Email),
            new CreateIndexOptions { Unique = true, Name = "ux_users_email" });

        await Users.Indexes.CreateOneAsync(emailIndex);
        Console.WriteLine($"Seed: {users.Length} users and a unique index created.");
    }

    public async Task PartialUpdateAsync()
    {
        var filter = Builders<User>.Filter.Eq(user => user.Email, "jorge@dicres.dev");
        var update = Builders<User>.Update
            .Set(user => user.Status, "active")
            .Set(user => user.UpdatedAt, DateTime.UtcNow)
            .Set(user => user.ETag, User.NewETag())
            .Inc(user => user.LoginCount, 1);

        var result = await Users.UpdateOneAsync(filter, update);
        Console.WriteLine(
            $"Partial update: matched={result.MatchedCount}, " +
            $"modified={result.ModifiedCount}.");
    }

    public async Task UpdateOperatorsAsync()
    {
        var filter = Builders<User>.Filter.Eq(user => user.Email, "jorge@dicres.dev");

        var unsetResult = await Users.UpdateOneAsync(
            filter,
            Builders<User>.Update.Unset(user => user.LegacyCode));

        var mulResult = await Users.UpdateOneAsync(
            filter,
            Builders<User>.Update.Mul("sessions.$[].riskScore", 0.5));

        var currentDateResult = await Users.UpdateOneAsync(
            filter,
            Builders<User>.Update.CurrentDate(user => user.UpdatedAt));

        var renameResult = await Users.UpdateOneAsync(
            Builders<User>.Filter.Eq(user => user.Email, "laura@example.com"),
            Builders<User>.Update.Rename(user => user.LegacyCode, "legacyId"));

        var newSession = CreateSession(
            "tablet-003",
            "tablet",
            new DateTime(2026, 9, 23, 10, 0, 0, DateTimeKind.Utc),
            150,
            25);
        var pushResult = await Users.UpdateOneAsync(
            filter,
            Builders<User>.Update.Push(user => user.Sessions, newSession));

        var newSessions = new[]
        {
            CreateSession("mobile-004", "mobile", DateTime.UtcNow, 300, 10),
            CreateSession("desktop-005", "desktop", DateTime.UtcNow, 600, 5)
        };
        var pushEachResult = await Users.UpdateOneAsync(
            filter,
            Builders<User>.Update.PushEach(user => user.Sessions, newSessions));

        var existingSession = CreateSession(
            "desktop-001",
            "desktop",
            new DateTime(2026, 9, 20, 8, 30, 0, DateTimeKind.Utc),
            180,
            20);
        var addToSetResult = await Users.UpdateOneAsync(
            filter,
            Builders<User>.Update.AddToSet(user => user.Sessions, existingSession));

        var revokedSession = Builders<LoginSession>.Filter.Eq(
            session => session.Revoked,
            true);
        var pullResult = await Users.UpdateOneAsync(
            filter,
            Builders<User>.Update.PullFilter(user => user.Sessions, revokedSession));

        var minResult = await Users.UpdateOneAsync(
            filter,
            Builders<User>.Update.Min(user => user.FastestSessionSeconds, 120));

        var maxResult = await Users.UpdateOneAsync(
            filter,
            Builders<User>.Update.Max("sessions.$[mobile].riskScore", 75),
            new UpdateOptions
            {
                ArrayFilters =
                [
                    new BsonDocumentArrayFilterDefinition<LoginSession>(
                        new BsonDocument("mobile.device", "mobile"))
                ]
            });

        Console.WriteLine(
            "Other operators: " +
            $"unset={unsetResult.ModifiedCount}, " +
            $"mul={mulResult.ModifiedCount}, " +
            $"currentDate={currentDateResult.ModifiedCount}, " +
            $"rename={renameResult.ModifiedCount}, " +
            $"push={pushResult.ModifiedCount}, " +
            $"pushEach={pushEachResult.ModifiedCount}, " +
            $"addToSet={addToSetResult.ModifiedCount}, " +
            $"pull={pullResult.ModifiedCount}, " +
            $"min={minResult.ModifiedCount}, " +
            $"max={maxResult.ModifiedCount}.");
    }

    private static LoginSession CreateSession(
        string sessionId,
        string device,
        DateTime startedAt,
        int durationSeconds,
        int riskScore,
        bool revoked = false)
    {
        return new LoginSession
        {
            SessionId = sessionId,
            Device = device,
            StartedAt = startedAt,
            DurationSeconds = durationSeconds,
            RiskScore = riskScore,
            Revoked = revoked
        };
    }

    public async Task ConditionalUpdateAsync()
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(user => user.Email, "jorge@dicres.dev"),
            Builders<User>.Filter.Eq(user => user.Status, "inactive"));
        var update = Builders<User>.Update
            .Set(user => user.Status, "active")
            .Set(user => user.UpdatedAt, DateTime.UtcNow)
            .Set(user => user.ETag, User.NewETag());

        var result = await Users.UpdateOneAsync(filter, update);
        Console.WriteLine(
            $"Conditional update: matched={result.MatchedCount}, " +
            $"modified={result.ModifiedCount}.");
    }

    public async Task ProjectionAsync()
    {
        var contactProjection = Builders<User>.Projection
            .Include(user => user.Name)
            .Include(user => user.Email)
            .Exclude(user => user.Id);

        var contacts = await Users
            .Find(user => user.Status == "active")
            .Project(contactProjection)
            .ToListAsync();

        Console.WriteLine("Projection with Include/Exclude:");
        foreach (var contact in contacts)
        {
            Console.WriteLine($"  {contact.ToJson()}");
        }

        var snapshots = await Users
            .Find(FilterDefinition<User>.Empty)
            .Project(user => new UserRiskSnapshot(
                user.Id,
                user.ETag,
                user.Sessions.Max(session => session.RiskScore)))
            .ToListAsync();

        Console.WriteLine("Typed projection:");
        foreach (var snapshot in snapshots)
        {
            Console.WriteLine(
                $"  {snapshot.Id}: eTag={snapshot.ETag}, maxRiskScore={snapshot.MaxRiskScore}");
        }
    }

    public async Task ConcurrencyAsync()
    {
        const int concurrentActions = 10;
        var filter = Builders<User>.Filter.Eq(user => user.Email, "jorge@dicres.dev");

        foreach (var useETag in new[] { false, true })
        {
            // All actions read before writing to guarantee a conflict.
            var snapshots = await Task.WhenAll(
                Enumerable.Range(0, concurrentActions)
                    .Select(_ => Users.Find(filter).FirstAsync()));
            var initialLoginCount = snapshots[0].LoginCount;
            var expectedLoginCount = checked(initialLoginCount + concurrentActions);

            var conflicts = await Task.WhenAll(
                snapshots.Select(snapshot =>
                    RegisterLoginWithRetryAsync(snapshot, useETag, concurrentActions)));

            var afterLogins = await Users.Find(filter).FirstAsync();
            var scenario = useETag ? "Replace with eTag and retries" : "Replace without eTag";
            Console.WriteLine(
                $"{scenario}: concurrent actions={concurrentActions}, " +
                $"initial loginCount={initialLoginCount}, expected={expectedLoginCount}, " +
                $"actual={afterLogins.LoginCount}, " +
                $"lost increments={expectedLoginCount - afterLogins.LoginCount}, " +
                $"detected conflicts={conflicts.Sum()}.");
        }
    }

    private async Task<int> RegisterLoginWithRetryAsync(
        User snapshot,
        bool useETag,
        int maxAttempts)
    {
        var idFilter = Builders<User>.Filter.Eq(user => user.Id, snapshot.Id);
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var writeFilter = useETag
                ? Builders<User>.Filter.And(
                    idFilter,
                    Builders<User>.Filter.Eq(user => user.ETag, snapshot.ETag))
                : idFilter;
            var updatedUser = snapshot with
            {
                LoginCount = checked(snapshot.LoginCount + 1),
                UpdatedAt = DateTime.UtcNow,
                ETag = User.NewETag()
            };

            var result = await Users.ReplaceOneAsync(writeFilter, updatedUser);
            if (result.MatchedCount == 1)
            {
                return attempt - 1;
            }

            if (!useETag)
            {
                throw new InvalidOperationException(
                    $"Could not register the login: user {snapshot.Id} no longer exists.");
            }

            if (attempt < maxAttempts)
            {
                snapshot = await Users.Find(idFilter).FirstAsync();
            }
        }

        throw new InvalidOperationException(
            $"Could not register the login after {maxAttempts} attempts.");
    }

    public async Task BulkWriteAsync()
    {
        var operations = new WriteModel<User>[]
        {
            new InsertOneModel<User>(
                new User
                {
                    Name = "Marta",
                    Email = "marta@example.com",
                    Role = "admin",
                    Status = "active",
                    Country = "ES",
                    LoginCount = 0
                }),
            new UpdateOneModel<User>(
                Builders<User>.Filter.Eq(user => user.Email, "pepe@example.com"),
                Builders<User>.Update
                    .Set(user => user.Status, "inactive")
                    .Set(user => user.UpdatedAt, DateTime.UtcNow)
                    .Set(user => user.ETag, User.NewETag())),
            new DeleteManyModel<User>(
                Builders<User>.Filter.Eq(user => user.Status, "banned"))
        };

        var result = await Users.BulkWriteAsync(
            operations,
            new BulkWriteOptions { IsOrdered = false });

        Console.WriteLine(
            $"Bulk: inserted={result.InsertedCount}, modified={result.ModifiedCount}, " +
            $"deleted={result.DeletedCount}.");
    }

    public async Task AggregateAsync()
    {
        var results = await Users.Aggregate()
            .Match(user => user.Status == "active")
            .Group(
                user => user.Country,
                group => new
                {
                    Country = group.Key,
                    TotalUsers = group.Count(),
                    TotalLogins = group.Sum(user => user.LoginCount)
                })
            .Match(result => result.TotalLogins > 10)
            .SortByDescending(result => result.TotalUsers)
            .ThenByDescending(result => result.TotalLogins)
            .Project(result => new CountryStats
            {
                Country = result.Country,
                TotalUsers = result.TotalUsers,
                TotalLogins = result.TotalLogins
            })
            .ToListAsync();

        Console.WriteLine("Aggregation:");
        foreach (var result in results)
        {
            Console.WriteLine(
                $"  {result.Country}: users={result.TotalUsers}, " +
                $"logins={result.TotalLogins}");
        }
    }

    public async Task ExplainAggregationAsync()
    {
        var command = new BsonDocument
        {
            {
                "explain",
                new BsonDocument
                {
                    { "aggregate", CollectionName },
                    {
                        "pipeline",
                        new BsonArray
                        {
                            new BsonDocument("$match", new BsonDocument("status", "active")),
                            new BsonDocument(
                                "$group",
                                new BsonDocument
                                {
                                    { "_id", "$country" },
                                    { "totalUsers", new BsonDocument("$sum", 1) }
                                })
                        }
                    },
                    { "cursor", new BsonDocument() }
                }
            },
            { "verbosity", "executionStats" }
        };

        var result = await database.RunCommandAsync<BsonDocument>(command);
        Console.WriteLine($"Explain:{Environment.NewLine}{result.ToJson()}");
    }

}