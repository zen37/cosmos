using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

class Program
{
    static readonly object updateLock = new object(); // Lock object for synchronization

    static async Task Main(string[] args)
    {
        // Set your Cosmos DB connection settings
        string cosmosEndpoint = "your-cosmosdb-endpoint";
        string cosmosKey = "your-cosmosdb-key";
        string databaseName = "your-database-name";
        string containerName = "your-container-name";
        string itemId = "item-id-to-update";

        // Create Cosmos client
        CosmosClient cosmosClient = new CosmosClient(cosmosEndpoint, cosmosKey);
        var container = cosmosClient.GetContainer(databaseName, containerName);

        int maxRetries = 3; // Maximum number of retries
        int retryCount = 0;
        bool updateSucceeded = false;

        while (!updateSucceeded && retryCount < maxRetries)
        {
            Item itemToUpdate = null;

            // Use a lock to ensure only one thread at a time enters this section
            lock (updateLock)
            {
                // Retrieve the item
                ItemResponse<Item> response = await container.ReadItemAsync<Item>(itemId, new PartitionKey(itemId));

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    itemToUpdate = response.Resource;

                    // Store the current ETag value
                    string currentETag = itemToUpdate.ETag;

                    // Update the counter
                    itemToUpdate.Counter += 1;
                }
                else
                {
                    Console.WriteLine("Item not found.");
                    break;
                }
            }

            if (itemToUpdate != null)
            {
                // Update the item with ETag concurrency control
                AccessCondition accessCondition = new AccessCondition
                {
                    Condition = itemToUpdate.ETag,
                    Type = AccessConditionType.IfMatch
                };

                ItemResponse<Item> updateResponse = await container.ReplaceItemAsync(itemToUpdate, itemId, new PartitionKey(itemId), accessCondition);

                if (updateResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    updateSucceeded = true;
                    Console.WriteLine($"Counter updated to: {itemToUpdate.Counter}");
                }
                else
                {
                    Console.WriteLine("Update failed due to concurrency. Retrying...");
                    retryCount++;
                }
            }
        }

        if (!updateSucceeded)
        {
            Console.WriteLine("Update failed after maximum retries.");
        }
    }
}

class Item
{
    public string Id { get; set; }
    public string PartitionKey { get; set; }
    public int Counter { get; set; }
    public string ETag { get; set; }
}
