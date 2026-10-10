# MongoDb.AdvancedOperation

Console application with the .NET examples from the blog post about advanced
MongoDB operations.

## Requirements

- .NET 10 SDK
- MongoDB 8, local or remote

To start a local instance:

```bash
docker compose up -d
```

If port `27017` is already in use, you can expose MongoDB on a different port:

```bash
MONGODB_PORT=27018 docker compose up -d
```

## Running the examples

The application uses `mongodb://localhost:27017` and the `advanced_operations`
database by default. You can override these settings with the `MONGODB_URI`
and `MONGODB_DATABASE` environment variables.

```bash
dotnet run --project MongoDb.AdvancedOperation -- all
```

`all` recreates the `users` collection and runs every example. You can also
run each operation separately:

```bash
dotnet run --project MongoDb.AdvancedOperation -- seed
dotnet run --project MongoDb.AdvancedOperation -- projection
dotnet run --project MongoDb.AdvancedOperation -- partial-update
dotnet run --project MongoDb.AdvancedOperation -- update-operators
dotnet run --project MongoDb.AdvancedOperation -- conditional-update
dotnet run --project MongoDb.AdvancedOperation -- concurrency
dotnet run --project MongoDb.AdvancedOperation -- bulk
dotnet run --project MongoDb.AdvancedOperation -- aggregate
dotnet run --project MongoDb.AdvancedOperation -- explain
```

### Concurrency

After running `seed`, the `concurrency` command compares two scenarios with ten
concurrent logins each. It displays the number of concurrent actions and the
initial, expected, and actual `loginCount`, along with the number of lost increments.
Each action reads the document, adds one to `loginCount` in memory, and saves it
using `ReplaceOneAsync`. All reads finish before the writes are launched with
`Task.WhenAll`, so every action starts from the same counter and the conflict is
reproducible:

- **Without eTag:** all ten actions save `initial + 1`. One increment is preserved
  and nine are lost.
- **With eTag and retries:** each write requires the eTag to match the value that
  was read and replaces it with a new one. On conflict, the action reads the
  document again and recalculates the increment before retrying (up to ten
  attempts per action). All ten increments are preserved, and the total number
  of detected conflicts is displayed.

Both scenarios use the same type of write to isolate the effect of the eTag.
For a simple counter, `$inc` would be an atomic alternative that does not require
this read-write cycle.

Each scenario calculates its expected value from its own initial counter.
The command modifies the example user and can be run again without recreating
the collection.

## Benchmark

The `benchmark` command uses a separate collection, `users_benchmark`, and
compares four ways to recalculate the risk level of all users: full documents
versus projections, and individual writes versus `BulkWriteAsync`. It optionally
accepts the number of users, the number of sessions per user, and the number of
runs per scenario:

```bash
dotnet run -c Release --project MongoDb.AdvancedOperation -- benchmark 10000 50 3
```

The collection is recreated before each run and dropped when the benchmark finishes.
