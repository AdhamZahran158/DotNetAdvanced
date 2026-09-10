# Real-Time Features

This document describes the real-time features added to the ECommerce application using **ASP.NET Core SignalR**, **CQRS/MediatR**, **Redis**, and the existing database layer.

The implemented features are:

1. Customer Support Chat
2. Real-Time Order Tracking

---

## 1. Technologies Used

* **ASP.NET Core SignalR** — provides real-time communication between the server and connected clients.
* **MediatR / CQRS** — keeps application operations inside commands and handlers.
* **Redis** — used for temporary/recent chat message storage.
* **SQL Server / Entity Framework Core** — stores chat conversations and messages as permanent data.
* **BackgroundService** — can be used later for asynchronous Redis-to-database message persistence.

---

# 2. Customer Support Chat

The chat feature allows a customer and customer-support staff to communicate through a real-time connection.

## Chat Architecture

```text
Client
   │
   │ SignalR
   ▼
ChatHub
   │
   │ MediatR
   ▼
SendChatMessageCommand
   │
   ▼
Command Handler
   │
   ├── Redis
   │     └── Temporary chat messages
   │
   └── SignalR
         └── Broadcast message
                │
                ▼
        Other conversation clients
```

The application also has database entities for permanently storing:

* Chat conversations
* Chat messages

The database represents the long-term chat history, while Redis can be used for temporary/recent message storage.

---

## 3. ChatConversation Entity

A `ChatConversation` represents a conversation between a customer and support.

It contains:

* Conversation ID
* Customer ID
* Creation date
* Closed/open state
* Collection of messages

Relationship:

```text
Customer
   │
   └── ChatConversations
          │
          ├── ChatConversation
          │      ├── Message
          │      ├── Message
          │      └── Message
          │
          └── ChatConversation
                 ├── Message
                 └── Message
```

A customer can therefore have multiple conversations.

---

# 4. ChatMessage Entity

`ChatMessage` represents an individual message.

It contains:

* Message ID
* Conversation ID
* Sender ID
* Sender type
* Message content
* Sent timestamp

The message is associated with a `ChatConversation`.

This allows the complete conversation history to be stored in SQL.

---

# 5. Redis Chat Storage

Redis is used as a fast temporary storage layer for chat messages.

Messages can be stored using conversation-specific Redis keys such as:

```text
chat:conversation:{conversationId}:messages
```

For example:

```text
chat:conversation:15:messages
```

This keeps messages belonging to different conversations separated.

Redis is not considered the permanent source of truth for chat history.

The database is responsible for permanent storage.

---

# 6. ChatHub

A `ChatHub` provides the real-time communication endpoint for chat.

The hub supports:

### Join Conversation

A client joins the SignalR group associated with a conversation.

```text
conversation-{conversationId}
```

For example:

```text
conversation-15
```

### Leave Conversation

The client can leave the conversation group.

### Send Message

The client sends a message through the hub.

The hub does not contain the actual business logic.

Instead, it sends a `SendChatMessageCommand` through MediatR.

```text
ChatHub
   │
   ▼
IMediator.Send(...)
   │
   ▼
SendChatMessageCommandHandler
```

This keeps the SignalR layer focused on communication while the application layer handles the operation.

---

# 7. Realtime Notifier

A realtime abstraction was added to prevent the Application layer from depending directly on SignalR.

```text
IRealtimeNotifier
```

It provides operations for notifying:

* Chat conversations
* Orders

The Application layer only knows about the abstraction.

The actual SignalR implementation exists in the API layer.

```text
Application
    │
    ▼
IRealtimeNotifier
    ▲
    │
API
    │
    ▼
SignalRRealtimeNotifier
    │
    ├── ChatHub
    └── OrderHub
```

This keeps SignalR-specific code outside the Application layer.

---

# 8. Order Tracking

The second real-time feature is order status tracking.

When an order status changes, connected clients can receive the new status immediately without repeatedly requesting the order from the API.

The flow is:

```text
Client / Admin
      │
      ▼
OrdersController
      │
      ▼
UpdateOrderStatusCommand
      │
      ▼
UpdateOrderStatusCommandHandler
      │
      ├── Update Order in Database
      │
      └── IRealtimeNotifier
                │
                ▼
          SignalR OrderHub
                │
                ▼
          Order Group
```

---

# 9. Order Groups

Each order has its own SignalR group:

```text
order-{orderId}
```

For example:

```text
order-125
```

A client interested in order `125` joins:

```text
order-125
```

When the order status changes, the server sends the update to that group.

This prevents unrelated clients from receiving the update.

---

# 10. Order Status Event

When the status changes, the SignalR event is:

```text
OrderStatusChanged
```

The event contains information such as:

```text
OrderId
Status
```

For example:

```json
{
    "orderId": 125,
    "status": "Shipped"
}
```

The client can use this event to immediately update the displayed order status.

---

# 11. SignalR Configuration

SignalR is registered in the API project and the hubs are mapped to endpoints.

The application exposes separate hubs for the two features:

```text
/hubs/chat
/hubs/orders
```

This keeps chat communication separate from order-tracking communication.

---

# 12. CQRS Integration

The real-time features follow the existing CQRS architecture.

For chat:

```text
ChatHub
   ↓
SendChatMessageCommand
   ↓
SendChatMessageCommandHandler
   ↓
Redis / Database
   ↓
IRealtimeNotifier
   ↓
SignalR
```

For order tracking:

```text
OrdersController
   ↓
UpdateOrderStatusCommand
   ↓
UpdateOrderStatusCommandHandler
   ↓
OrderRepository
   ↓
Database
   ↓
IRealtimeNotifier
   ↓
SignalR
```

This means SignalR does not replace CQRS. It acts as the real-time communication mechanism around the existing application architecture.

---

# 13. Important Current Limitation

The application does not currently have authentication and authorization.

Therefore, joining a conversation or order group using only an ID is **not secure** in the current implementation.

For example, a client could potentially attempt to join:

```text
order-125
```

without the server currently verifying that the client actually owns or is authorized to view order `125`.

This is acceptable for the current learning/development stage, but it must be addressed when authentication is introduced.

A future authentication implementation should verify:

* Which customer is connected
* Which conversations belong to that customer
* Which orders belong to that customer
* Which users are support/admin users
* Whether the connected user is allowed to join the requested SignalR group

---

# 14. Current Data Responsibilities

The current design separates the responsibilities of each technology.

| Component         | Responsibility                                                      |
| ----------------- | ------------------------------------------------------------------- |
| SignalR           | Real-time communication                                             |
| ChatHub           | Chat connection and group management                                |
| OrderHub          | Order group management                                              |
| MediatR           | Dispatching application commands                                    |
| Application       | Business/use-case logic                                             |
| Redis             | Fast temporary/recent chat storage                                  |
| SQL Server        | Permanent chat/order data                                           |
| EF Core           | Database access                                                     |
| BackgroundService | Existing mechanism that can be extended for asynchronous processing |

---

# 15. Future Enhancement: Redis → Database Background Service

A possible future improvement is to add a dedicated `BackgroundService` that transfers chat messages from Redis to the database after they have remained in Redis for a defined period.

For example:

```text
Chat Message
     │
     ▼
   Redis
     │
     │ remains temporarily
     │
     ▼
BackgroundService
     │
     ▼
 SQL Server
     │
     ▼
Permanent Chat History
```

The service could periodically:

1. Find messages that have reached the configured retention period.
2. Read the messages from Redis.
3. Insert them into `ChatMessage` in SQL Server.
4. Confirm that the database operation succeeded.
5. Remove the successfully persisted messages from Redis.

A unique message identifier should be used so that if the background service fails after inserting a message but before removing it from Redis, the same message will not be inserted twice.

For example:

```text
Redis
  │
  │ Message A
  ▼
Background Service
  │
  ▼
SQL Server
  │
  ├── Success
  │
  ▼
Remove Message A from Redis
```

This enhancement would allow Redis to act as a temporary message buffer while SQL Server remains the permanent storage layer.

---

# 16. Summary

The application now has two real-time capabilities:

### Customer Support Chat

```text
Customer / Support
       ↕
   SignalR ChatHub
       ↕
    MediatR
       ↕
 Redis / Database
```

### Order Tracking

```text
Order Status Change
       ↓
   MediatR Handler
       ↓
    SQL Server
       ↓
   SignalR OrderHub
       ↓
 Interested Clients
```

The architecture keeps the responsibilities separated:

* **SignalR** handles real-time communication.
* **CQRS/MediatR** handles application operations.
* **Redis** provides temporary/fast chat storage.
* **SQL Server** provides permanent storage.
* **BackgroundService** can later handle delayed Redis-to-database persistence.
* **Authentication/authorization** will be required before exposing these features securely to real users.
