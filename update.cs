using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

class Program
{
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
            // Retrieve the item
            ItemResponse<Item> response = await container.ReadItemAsync<Item>(itemId, new PartitionKey(itemId));

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Item item = response.Resource;

                // Store the current ETag value
                string currentETag = item.ETag;

                // Update the counter
                item.Counter += 1;

                // Update the item with ETag concurrency control
                AccessCondition accessCondition = new AccessCondition
                {
                    Condition = currentETag,
                    Type = AccessConditionType.IfMatch
                };

                ItemResponse<Item> updateResponse = await container.ReplaceItemAsync(item, itemId, new PartitionKey(itemId), accessCondition);

                if (updateResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    updateSucceeded = true;
                    Console.WriteLine($"Counter updated to: {item.Counter}");
                }
                else
                {
                    Console.WriteLine("Update failed due to concurrency. Retrying...");
                    retryCount++;
                }
            }
            else
            {
                Console.WriteLine("Item not found.");
                break;
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
