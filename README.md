# Order Processing System
## Thought Process

### Main Tasks
1. Build three independant microservices.
2. Implement RESTful APIs for services.
4. Use event-driven communication between services (cross-service communication).
5. Containerize all services.

### Assumptions
1. Should the services have authentication and authorization? => Assumed no for simplicity.
2. Should the services support pagination => Assumed no for simplicity.

### Highlevel Technical Design
1. Use ASP.NET Core Web API projects (.NET 9).
2. Use single GitHub repo, single solution with four projects(Not real world style - just to make it easier to clone, run and review the solution).
3. Use EF Core with in-memory database for data storage.
4. Use MassTransit.RabbitMQ for events handling.
6. Add a shared class library to keep common contracts. 
7. Add unit tests projects to cover all three projects.
8. Add Logging to log important events by each services.
8. Use Docker for containarisation with docker-compose.
9. Add OpenApi + scalar support for APIs.

### Highlevel Functional Design
1. Order Service
    - Expose REST endpoints to create and get orders.
    - Validate order input.
    - Store order data in an in-memory database.
    - Publish `OrderCreatedEvent` event to RabbitMQ on order creation.
2. Payment Service
    - Expose REST endpoint to get payments.
    - Listen to `OrderCreatedEvent` event on payment creation.
    - Simulate a payment process.
    - Store payment data in an in-memory database.
    - Publish `PaymentSucceededEvent` event to RabbitMQ on payment creation.
3. Notification Service
    - Expose REST endpoint to get notifications.
    - Simulate sending a notification.
    - Store notification data in an in-memory database.
    - Listen to `PaymentSucceededEvent` event on payment creation.
4. Shared.Contracts Library
    - Define common models for User and Order.
    - Define event models for `UserCreated` and `OrderCreated`.
    - Implement Kafka producer and consumer utilities.
