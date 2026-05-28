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
10. Handle Idempotency.
11. Handle basic message re-try.

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
    - Idempotency checks to avoid duplicate payments.
3. Notification Service
    - Expose REST endpoint to get notifications.
    - Simulate sending a notification.
    - Store notification data in an in-memory database.
    - Listen to `PaymentSucceededEvent` event on payment creation.
4. Shared.Contracts Library
    - Define common contracts for the services.
    - Define event models for `OrderCreatedEvent` and `PaymentSucceededEvent`.
  
## Trade-offs
1. Used in-memory database instead of a persistent database for simplicity and ease of setup.
2. Kept all services in a single repository for easier review, though in real-world scenarios, they would be in separate repositories.
3. Limited error handling and validation to essential checks to keep the implementation straightforward.

## Future improvements
1. Implement authentication and authorization for secure access to APIs.
2. Add pagination support for GET endpoints.
3. Use a persistent database like SQL Server or PostgreSQL instead of in-memory databases.
4. Implement better retry policies and error handling for resilience.
5. Add more comprehensive logging and monitoring.


## Tech Stack
.NET 9, ASP.NET Core Web API, MassTransit, RabbitMQ, Docker & Docker Compose, Serilog (structured logging), Scalar API documentation

## Setup Instructions

### Prerequisites
- **.NET 9 SDK** 
- **Docker Desktop**
- **Git**

### Steps to Run the Application

#### Clone the Repository
- ```git clone https://github.com/sajithdilhan/dotnet-microservices-takehome-sajithd.git```
- ```cd dotnet-microservices-takehome-sajithd```

#### Run the System with Docker
Start all services and infrastructure using Docker Compose.

```docker-compose up --build -d```

#### Open API endpoints
- Order Service: ```http://localhost:5001/scalar/```
- Payment Service: ```http://localhost:5002/scalar/```
- Notification Service: ```http://localhost:5003/scalar/```

[![.NET CI](https://github.com/sajithdilhan/dotnet-microservices-takehome-sajithd/actions/workflows/dotnet-ci.yml/badge.svg?branch=master)](https://github.com/sajithdilhan/dotnet-microservices-takehome-sajithd/actions/workflows/dotnet-ci.yml)
