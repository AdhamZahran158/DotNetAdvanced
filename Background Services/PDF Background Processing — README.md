# Asynchronous PDF Generation with Background Services

This feature demonstrates how to generate order PDFs **asynchronously in the background** using:

- ASP.NET Core `BackgroundService`
- `System.Threading.Channels`
- CQRS
- MediatR
- Clean Architecture / Onion Architecture
- Dependency Injection scopes
- Repository abstraction

The main goal of this feature is to demonstrate **background processing**, rather than making the HTTP request wait for an expensive PDF-generation operation.

---

## 1. Why Background Processing?

Generating a PDF can be an expensive operation.

If the API generates the PDF directly inside the HTTP request:

```text
Client
  │
  │ POST /api/orders/15/generate-pdf
  ▼
Controller
  │
  ▼
Generate PDF
  │
  ▼
Save PDF
  │
  ▼
HTTP Response
```

The client has to wait until the PDF is completely generated.

For a large PDF or a slow PDF-generation library, this can make the request unnecessarily slow.

Instead, the application can queue the job:

```text
Client
  │
  │ POST /api/orders/15/generate-pdf
  ▼
Controller
  │
  ▼
MediatR
  │
  ▼
Queue PDF Job
  │
  ▼
202 Accepted
```

The PDF is then generated independently:

```text
Channel
   │
   ▼
BackgroundService
   │
   ▼
MediatR
   │
   ▼
PDF Handler
   │
   ├── Get Order
   │
   └── Generate PDF
```

This allows the API to respond immediately while the background worker processes the actual PDF-generation task.

---

# 2. Architecture

The complete flow is:

```text
                    HTTP Request
                         │
                         ▼
                ┌─────────────────┐
                │ OrdersController│
                └────────┬────────┘
                         │
                         │ IMediator.Send()
                         ▼
              ┌──────────────────────┐
              │ QueueOrderPdfCommand │
              └──────────┬───────────┘
                         │
                         ▼
            QueueOrderPdfCommandHandler
                         │
                         │ EnqueueAsync()
                         ▼
                ┌─────────────────┐
                │ Channel<Job>    │
                └────────┬────────┘
                         │
                         │ DequeueAsync()
                         ▼
                ┌─────────────────┐
                │ BillPdfCreator  │
                │ BackgroundService│
                └────────┬────────┘
                         │
                         │ IMediator.Send()
                         ▼
             GenerateOrderPdfCommand
                         │
                         ▼
          GenerateOrderPdfCommandHandler
                         │
                 ┌───────┴────────┐
                 ▼                ▼
         IOrderRepository    IPdfGenerator
                 │                │
                 ▼                ▼
               Order             PDF
```

There are therefore **two separate use cases**:

### Queue the PDF

```text
QueueOrderPdfCommand
```

Responsible only for putting a PDF-generation job into the queue.

### Generate the PDF

```text
GenerateOrderPdfCommand
```

Responsible for actually retrieving the order and generating the PDF.

Keeping these operations separate prevents the queueing operation from being coupled to the actual PDF-generation process.

---

# 3. BackgroundService

The main background component is:

```csharp
public class BillPdfCreator : BackgroundService
```

`BackgroundService` is an ASP.NET Core abstraction for implementing long-running background work.

Unlike a normal controller action, it is not triggered by an HTTP request.

The application starts the service when the host starts.

Conceptually:

```text
Application starts
       │
       ▼
BillPdfCreator starts
       │
       ▼
Wait for a job
       │
       ▼
Process job
       │
       ▼
Wait for another job
       │
       ▼
Process job
       │
       ▼
...
```

The worker continues running until the application shuts down.

---

# 4. Why the Worker Uses a Loop

The worker uses:

```csharp
while (!stoppingToken.IsCancellationRequested)
```

because it needs to process multiple jobs during the lifetime of the application.

The important operation is:

```csharp
job = await _queue.DequeueAsync(stoppingToken);
```

If there is no job available, the worker does **not** continuously execute useless work.

It asynchronously waits for a job.

Once a job is added:

```text
Channel
   │
   │ Job available
   ▼
DequeueAsync()
   │
   ▼
Worker wakes up
   │
   ▼
Process PDF
```

This is much better than repeatedly polling:

```csharp
while (true)
{
    if (queue.Count > 0)
    {
        // process
    }
}
```

because that approach can waste CPU while waiting.

---

# 5. Channel as the Job Queue

The application uses:

```csharp
Channel<GeneratePdfJob>
```

from:

```csharp
System.Threading.Channels
```

A `Channel<T>` provides an asynchronous producer/consumer mechanism.

In this application:

```text
Producer
   │
   │ WriteAsync()
   ▼
Channel
   │
   │ ReadAsync()
   ▼
Consumer
```

The producer is the application code that requests PDF generation.

The consumer is `BillPdfCreator`.

---

# 6. Why Channel Instead of Queue<T>?

A normal:

```csharp
Queue<T>
```

is primarily a data structure.

It does not automatically provide the synchronization and asynchronous waiting required for a background producer/consumer system.

Using `Queue<T>` safely would require additional synchronization mechanisms such as:

- `lock`
- `SemaphoreSlim`
- manual signaling

`Channel<T>` is specifically designed for asynchronous producer/consumer scenarios.

It provides:

- Thread safety
- Asynchronous reads
- Asynchronous writes
- Cancellation support
- Backpressure when bounded
- Producer/consumer semantics

Therefore:

```text
Queue<T>
    = general FIFO data structure

Channel<T>
    = asynchronous producer/consumer communication mechanism
```

---

# 7. Bounded Channel

The queue is configured as a bounded channel:

```csharp
_queue = Channel.CreateBounded<GeneratePdfJob>(
    new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
        SingleWriter = false
    });
```

This means the queue can contain up to:

```text
100 jobs
```

at a time.

If all 100 positions are occupied, another producer waits until space becomes available.

This is called **backpressure**.

```text
Producer
   │
   ▼
┌─────────────────────┐
│ Channel             │
│                     │
│ Job 1               │
│ Job 2               │
│ ...                 │
│ Job 100             │
└─────────────────────┘
          │
          │ Full
          ▼
     Producer waits
```

Without a bound, an application receiving a very large number of requests could theoretically keep adding jobs to memory.

---

# 8. The Job Object

The queue does not contain the entire PDF-generation operation.

It contains a small job description:

```csharp
public record GeneratePdfJob(
    int OrderId,
    string OutputPath);
```

This represents:

> "Generate the PDF for this order and save it here."

The worker later converts this job into a MediatR command.

```text
GeneratePdfJob
      │
      ▼
GenerateOrderPdfCommand
      │
      ▼
Handler
```

---

# 9. Queue Abstraction

The Application layer defines:

```csharp
public interface IPdfJobQueue
{
    ValueTask EnqueueAsync(
        GeneratePdfJob job,
        CancellationToken cancellationToken = default);

    ValueTask<GeneratePdfJob> DequeueAsync(
        CancellationToken cancellationToken = default);
}
```

The Application layer therefore knows that a PDF job can be queued.

It does **not** know that the implementation uses:

```csharp
Channel<GeneratePdfJob>
```

That implementation detail belongs to Infrastructure.

```text
Application
    │
    │ IPdfJobQueue
    ▼
Infrastructure
    │
    │ PdfJobQueue
    ▼
Channel<T>
```

This follows the Dependency Inversion Principle.

---

# 10. MediatR and CQRS

The controller does not directly access the queue.

Instead:

```text
Controller
    │
    ▼
IMediator
    │
    ▼
QueueOrderPdfCommand
    │
    ▼
QueueOrderPdfCommandHandler
    │
    ▼
IPdfJobQueue
```

The controller therefore only knows about the application use case.

It does not know:

- how the queue works
- whether the queue uses `Channel`
- how the PDF is generated
- which PDF library is used
- how the order is retrieved

This keeps the API layer thin.

---

# 11. Why Two Commands?

There are two different operations.

## QueueOrderPdfCommand

```text
"Please generate a PDF for Order 15."
```

It does not generate the PDF.

It simply creates a job:

```text
QueueOrderPdfCommand
        │
        ▼
IPdfJobQueue.EnqueueAsync()
```

## GenerateOrderPdfCommand

```text
"Actually generate the PDF for Order 15."
```

This is executed by the background worker.

```text
GenerateOrderPdfCommand
        │
        ▼
GenerateOrderPdfCommandHandler
        │
        ├── IOrderRepository
        │
        └── IPdfGenerator
```

This separation keeps the asynchronous boundary explicit.

---

# 12. Dependency Injection Scope

One of the most important parts of this implementation is the DI scope.

`BackgroundService` is registered as a hosted service and effectively lives for the lifetime of the application.

However, services such as:

```text
DbContext
Repositories
```

are normally scoped.

Therefore, the worker should **not** directly inject a scoped repository.

Instead, it receives:

```csharp
IServiceScopeFactory
```

and creates a scope for each job:

```csharp
using var scope = _scopeFactory.CreateScope();
```

Then it resolves MediatR from that scope:

```csharp
var mediator =
    scope.ServiceProvider
        .GetRequiredService<IMediator>();
```

The result is:

```text
BackgroundService
      │
      │ Singleton / long-lived
      ▼
IServiceScopeFactory
      │
      ▼
CreateScope()
      │
      ├── IMediator
      ├── DbContext
      ├── Repositories
      └── Other scoped services
```

After the job completes:

```csharp
scope.Dispose();
```

and the scoped dependencies are disposed.

The next job receives a fresh scope.

---

# 13. Why a New Scope for Every Job?

This is important.

Do not create one scope when the worker starts and reuse it forever.

Bad approach:

```text
Worker starts
    │
    ▼
Create one scope
    │
    ▼
Reuse DbContext for hours
    │
    ▼
Process hundreds of jobs
```

Instead:

```text
Job 1
  │
  ▼
Scope 1
  │
  ▼
Process
  │
  ▼
Dispose

Job 2
  │
  ▼
Scope 2
  │
  ▼
Process
  │
  ▼
Dispose
```

This prevents scoped services such as `DbContext` from living for the entire lifetime of the application.

---

# 14. Controller

The controller remains thin and uses MediatR:

```csharp
[HttpPost("{id:int}/generate-pdf")]
public async Task<IActionResult> GeneratePdf(
    int id,
    CancellationToken cancellationToken)
{
    var outputDirectory =
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "GeneratedPdfs");

    Directory.CreateDirectory(outputDirectory);

    var outputPath =
        Path.Combine(
            outputDirectory,
            $"Order-{id}.pdf");

    await _mediator.Send(
        new QueueOrderPdfCommand(
            id,
            outputPath),
        cancellationToken);

    return Accepted(new
    {
        Message = "PDF generation has been queued.",
        OrderId = id
    });
}
```

The important response is:

```http
202 Accepted
```

rather than:

```http
200 OK
```

because the request has been accepted for processing, but the PDF may not have been generated yet.

---

# 15. End-to-End Example

Suppose the client sends:

```http
POST /api/orders/15/generate-pdf
```

### Step 1 — Controller

The controller sends:

```text
QueueOrderPdfCommand
OrderId = 15
```

### Step 2 — Queue Handler

The handler creates:

```text
GeneratePdfJob
OrderId = 15
OutputPath = GeneratedPdfs/Order-15.pdf
```

and puts it into the Channel.

### Step 3 — HTTP Response

The API immediately returns:

```http
202 Accepted
```

The client does not wait for PDF generation.

### Step 4 — Background Worker

`BillPdfCreator` receives the job:

```text
GeneratePdfJob
      │
      ▼
OrderId = 15
```

### Step 5 — MediatR

The worker sends:

```text
GenerateOrderPdfCommand
```

### Step 6 — Handler

The handler retrieves the order:

```text
IOrderRepository
       │
       ▼
Order #15
```

### Step 7 — PDF Generation

The handler calls:

```text
IPdfGenerator
       │
       ▼
Order-15.pdf
```

### Step 8 — Job Completion

The PDF job is finished.

The worker goes back to:

```csharp
await _queue.DequeueAsync(stoppingToken);
```

and waits for the next job.

---

# 16. Important Limitation: In-Memory Queue

The current implementation uses:

```csharp
Channel<GeneratePdfJob>
```

which means the queue exists only in application memory.

Therefore:

```text
Application running
    │
    ▼
Channel contains jobs
```

If the application crashes:

```text
Application crashes
    │
    ▼
Memory lost
    │
    ▼
Queued jobs lost
```

The same applies when the application is restarted.

This implementation is therefore excellent for demonstrating the **BackgroundService + producer/consumer architecture**, but it is not a durable job-processing system.

For production systems requiring guaranteed job persistence, a durable message broker/job system would normally be used.

Examples include:

- RabbitMQ
- Azure Service Bus
- Amazon SQS
- Hangfire with persistent storage

The important distinction is:

```text
Channel<T>
    = in-memory background queue

Message Broker / Persistent Job Store
    = durable background queue
```

---

# 17. BackgroundService vs Individual Job

An important concept is that the `BackgroundService` itself does **not** terminate after one PDF.

The worker is long-lived:

```text
BackgroundService
──────────────────────────────────────►
    │       │       │       │
   Job 1   Job 2   Job 3   Job 4
```

Each PDF generation is an individual job:

```text
Worker
  │
  ├── Process Job 1 → finished
  │
  ├── Process Job 2 → finished
  │
  ├── Process Job 3 → finished
  │
  └── Wait for Job 4
```

Therefore:

> The worker is persistent; the jobs are temporary.

This is the fundamental concept behind this implementation.

---

# 18. Clean Architecture Responsibilities

| Component | Responsibility |
|---|---|
| `OrdersController` | Receives HTTP request |
| `QueueOrderPdfCommand` | Represents the queue-PDF use case |
| `QueueOrderPdfCommandHandler` | Enqueues the job |
| `IPdfJobQueue` | Application abstraction for the queue |
| `PdfJobQueue` | Channel-based queue implementation |
| `BillPdfCreator` | Background worker |
| `GenerateOrderPdfCommand` | Represents actual PDF generation |
| `GenerateOrderPdfCommandHandler` | Coordinates PDF generation |
| `IOrderRepository` | Retrieves order data |
| `IPdfGenerator` | Application abstraction for PDF generation |
| `PdfGenerator` | Actual PDF-generation implementation |

The resulting dependency direction is:

```text
API
 │
 ▼
Application
 │
 │ abstractions
 ▼
Infrastructure
```

Infrastructure implements the abstractions defined by Application.

---

# 19. Key Concepts Demonstrated

This feature demonstrates several important backend concepts:

### Background Processing

Moving expensive work outside the HTTP request lifecycle.

### Producer/Consumer Pattern

One component produces jobs while another consumes them.

```text
Producer → Queue → Consumer
```

### `Channel<T>`

A thread-safe asynchronous producer/consumer mechanism.

### Backpressure

Preventing unlimited in-memory queue growth using a bounded channel.

### Hosted Services

Running long-lived background work inside an ASP.NET Core application.

### Dependency Injection Scopes

Creating proper scoped lifetimes inside a long-lived background worker.

### CQRS

Separating:

```text
Queue PDF
```

from:

```text
Generate PDF
```

### MediatR

Decoupling controllers/workers from concrete handlers.

### Dependency Inversion

Application defines abstractions such as:

```text
IPdfJobQueue
IPdfGenerator
IOrderRepository
```

while Infrastructure provides their implementations.

---

# 20. Summary

The main lesson of this feature is that an expensive operation does not always need to execute inside the HTTP request.

Instead:

```text
HTTP Request
     │
     ▼
Create Command
     │
     ▼
Queue Job
     │
     ▼
202 Accepted
```

Then independently:

```text
Channel
   │
   ▼
BackgroundService
   │
   ▼
Create DI Scope
   │
   ▼
MediatR
   │
   ▼
Command Handler
   │
   ├── Repository
   │
   └── PDF Generator
   │
   ▼
PDF Created
```

This provides a simple foundation for understanding **asynchronous background processing in ASP.NET Core**.

For this project, `Channel<T>` is intentionally used because it provides a lightweight, dependency-free way to learn the producer/consumer and `BackgroundService` concepts. In a production environment where jobs must survive application restarts, a persistent queue or job-processing system should be considered.