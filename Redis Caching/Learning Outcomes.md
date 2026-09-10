# Redis Product Visit Tracking & Background Processing

## Overview

Added a product visit tracking feature using **Redis as a temporary counter** and a **BackgroundService to periodically persist accumulated visits to SQL Server**.

The goal is to avoid updating the database on every product visit while still maintaining a permanent visit count for each product.

---

## Feature Flow

```text
Client
  │
  │ POST /api/products/{id}/view
  ▼
ProductsController
  │
  │ MediatR
  ▼
RecordProductViewCommandHandler
  │
  ▼
IProductViewCounter
  │
  ▼
Redis
  │
  │ Every 10 minutes
  ▼
ProductViewFlushWorker
  │
  │ MediatR
  ▼
FlushProductViewsCommandHandler
  │
  ▼
Product Repository
  │
  ▼
SQL Server
```

---

## Storage Design

The `Product` entity contains a permanent counter:

```csharp
public int TimesVisited { get; set; }
```

Redis stores only the visits that have **not yet been persisted**.

### Redis Key Format

```text
product:views:{productId}
```

Example:

```text
product:views:15 = 27
```

This means Product `15` received **27 visits since the last flush**.

SQL Server might contain:

```text
ProductId = 15
TimesVisited = 100
```

After flushing:

```text
TimesVisited = 100 + 27 = 127
```

The Redis key is then removed.

### Why Redis Does Not Store the Database Total

Redis is being used as a **buffer**, not as the source of truth.

```text
SQL Server → permanent lifetime total
Redis      → temporary unpersisted increments
```

This prevents accidentally adding the database's existing total again during a flush.

---

## Recording a Visit

The endpoint sends a CQRS command:

```http
POST /api/products/15/view
```

The handler calls:

```csharp
await _productViewCounter.IncrementAsync(productId);
```

Redis uses:

```csharp
await database.StringIncrementAsync(key);
```

### Important Redis Behavior

`StringIncrementAsync` automatically creates the key if it does not exist.

For example:

```text
Before:
(no key)

INCR product:views:15

After:
product:views:15 = 1
```

If the key already contains `7`:

```text
7 → 8
```

There is therefore **no need to manually check whether the key exists** or initialize it from the database.

The increment operation is also atomic, making it safe when multiple requests arrive concurrently.

---

## Flushing Redis to SQL Server

A `BackgroundService` runs every 10 minutes.

```csharp
using var timer =
    new PeriodicTimer(TimeSpan.FromMinutes(10));
```

At every interval it sends:

```csharp
new FlushProductViewsCommand()
```

through MediatR.

The handler:

1. Finds the Redis product-view keys.
2. Atomically retrieves and deletes each counter.
3. Gets the corresponding product from SQL Server.
4. Adds the buffered views to `TimesVisited`.
5. Commits the changes.

Example:

```text
Redis:
product:views:15 = 27
product:views:20 = 14

        ↓ Flush

SQL:
Product 15: TimesVisited += 27
Product 20: TimesVisited += 14

        ↓

Redis counters are removed
```

---

## Atomic GET + DELETE

A simple implementation using:

```text
GET
DELETE
```

can lose visits.

For example:

```text
Worker: GET → 10

User:   INCR → 11

Worker: DELETE
```

The user's new visit could be deleted without being persisted.

To avoid this, the implementation uses Redis `GETDEL`:

```csharp
var value = await database.ExecuteAsync(
    "GETDEL",
    key);
```

`GETDEL` retrieves the current value **and deletes the key atomically**.

Therefore:

```text
Redis = 10

GETDEL
  ↓
returns 10
key is deleted

New visit
  ↓
INCR
  ↓
new key = 1
```

The new visit belongs to the next flush instead of being lost.

> `GETDEL` requires Redis 6.2 or later.

---

## BackgroundService and Dependency Injection

`BackgroundService` is registered as a hosted service:

```csharp
builder.Services.AddHostedService<
    ProductViewFlushWorker>();
```

A hosted service has a long application lifetime and should not directly depend on scoped services such as:

* `DbContext`
* Repository instances
* Other scoped application services

Instead, the worker injects:

```csharp
IServiceScopeFactory
```

and creates a scope for each flush:

```csharp
using var scope =
    _scopeFactory.CreateScope();

var mediator =
    scope.ServiceProvider.GetRequiredService<IMediator>();
```

This allows the CQRS handler to safely resolve its scoped repository and `DbContext`.

---

## Redis Connection Lifetime

`IConnectionMultiplexer` is registered as a singleton:

```csharp
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        redisConnectionString));
```

The connection multiplexer is designed to be shared and reused rather than creating a new Redis connection for every request.

---

## CQRS Responsibilities

Two commands were introduced:

### RecordProductViewCommand

Responsible for recording a single visit.

```text
Controller
    ↓
RecordProductViewCommand
    ↓
Handler
    ↓
Redis counter
```

### FlushProductViewsCommand

Responsible for moving buffered Redis counters into SQL Server.

```text
BackgroundService
    ↓
FlushProductViewsCommand
    ↓
Handler
    ↓
Redis
    ↓
Repository
    ↓
SQL Server
```

This keeps the controller thin and separates the two different use cases.

---

## Key Redis Concepts Learned

### 1. Redis as a Counter

Redis can efficiently maintain counters using atomic increment operations:

```csharp
StringIncrementAsync(key)
```

### 2. Missing Keys

Incrementing a non-existing numeric key creates it with the increment value.

```text
INCR missing-key
→ 1
```

### 3. Atomic Operations

Operations such as `INCR` and `GETDEL` are atomic, which is important when multiple requests access the same key concurrently.

### 4. Key Naming

A consistent prefix provides logical organization:

```text
product:views:15
product:views:20
product:views:25
```

### 5. Redis as a Temporary Buffer

Redis does not always need to contain the final source-of-truth data. It can be used to buffer high-frequency operations before persisting them to a relational database.

---

## Key BackgroundService Concepts Learned

### 1. Periodic Background Work

`PeriodicTimer` can be used for recurring asynchronous work:

```csharp
using var timer =
    new PeriodicTimer(TimeSpan.FromMinutes(10));
```

### 2. Scoped Dependencies

A `BackgroundService` should create a scope when it needs scoped dependencies.

```text
BackgroundService
       ↓
IServiceScopeFactory
       ↓
Scope
       ↓
MediatR / DbContext / Repositories
```

### 3. Cancellation

The worker uses the application's `CancellationToken` so it can stop cleanly when the application shuts down.

### 4. Error Isolation

The flush operation is wrapped in exception handling so an error in one execution does not permanently terminate the background worker.

---

## Architecture Summary

```text
                ┌──────────────┐
                │    Client    │
                └──────┬───────┘
                       │
                       ▼
                ┌──────────────┐
                │ API Endpoint │
                └──────┬───────┘
                       │
                       ▼
                ┌──────────────┐
                │    MediatR   │
                └──────┬───────┘
                       │
                       ▼
                ┌──────────────┐
                │ View Handler │
                └──────┬───────┘
                       │
                       ▼
                ┌──────────────┐
                │    Redis     │
                │   Counter    │
                └──────┬───────┘
                       │
                 Every 10 min
                       │
                       ▼
                ┌──────────────┐
                │ Background   │
                │    Worker    │
                └──────┬───────┘
                       │
                       ▼
                ┌──────────────┐
                │ Flush Handler│
                └──────┬───────┘
                       │
                       ▼
                ┌──────────────┐
                │ SQL Server   │
                │ TimesVisited │
                └──────────────┘
```

## Result

The feature demonstrates how Redis can be used to absorb frequent write operations while SQL Server remains the persistent source of truth.

It also provides practical experience with:

* Redis counters
* Atomic Redis operations
* Redis key design
* `IConnectionMultiplexer`
* ASP.NET Core `BackgroundService`
* `PeriodicTimer`
* Scoped dependency handling in background workers
* CQRS + MediatR
* Repository-based persistence
* Graceful cancellation and background error handling