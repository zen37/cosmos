# Handling Concurrency in Azure Cosmos DB: ETag vs. Locking

When dealing with concurrent updates in Azure Cosmos DB, it's essential to choose the right approach for managing concurrency to ensure data integrity and optimal performance. This guide compares two common approaches: using ETags (Optimistic Concurrency Control) and manual locking with the `lock` statement.

## ETag Approach (Optimistic Concurrency Control)

- **Optimistic Concurrency Control**: Azure Cosmos DB's primary mechanism for concurrency control is based on optimistic principles. ETags (entity tags) are assigned to each document and track the version of the data.

- **High Concurrency**: ETags allow multiple operations to proceed concurrently without locking data until a conflict is detected during validation.

- **Scalability**: ETags align well with Cosmos DB's design for global distribution and scalability, minimizing contention and ensuring better performance.

- **Low Latency**: ETag approach supports Cosmos DB's goal of low-latency access to data by reducing contention and bottlenecks.

- **Simplicity**: Using ETags requires less code and complexity than managing manual locking mechanisms.

- **Global Distribution**: ETags are designed to work seamlessly in globally distributed environments.

## Locking Approach with `lock` Statement

- **Manual Locking**: Using the C# `lock` statement to manage concurrency is an option, but it's not the primary approach in Cosmos DB.

- **Suitable for Single Process**: Manual locking could be suitable for scenarios where you need synchronization within a single process or application.

- **Less Scalable**: Manual locking might introduce performance bottlenecks and scalability challenges in a globally distributed environment.

- **Higher Latency**: Using manual locks could increase latency due to potential contention for the lock.

- **Higher Complexity**: Implementing and managing manual locking can be more complex than relying on built-in mechanisms like ETags.

In conclusion, when working with concurrent updates in Azure Cosmos DB, it's generally recommended to use the ETag approach provided by the platform. ETags offer a balance between high concurrency, low latency, scalability, and simplicity. While the `lock` statement can be used for synchronization within a single application, it's best suited for scenarios where you can't leverage Cosmos DB's built-in mechanisms.