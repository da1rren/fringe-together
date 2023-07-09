namespace Fringe.Together.Web.Data;

using Microsoft.Azure.Cosmos;
using Models;

public class InterestedService
{
    private readonly CosmosClient _client;

    private Container Container => _client.GetContainer(WellKnown.Cosmos.Database,
        WellKnown.Cosmos.Containers.Interested);

    public InterestedService(CosmosClient client)
    {
        _client = client;
    }

    public async Task<Interested?> GetInterest(Uri uri, string userId)
    {
        try
        {
            var id = Models.Interested.GenerateId(uri, userId);
            var item = await Container.ReadItemAsync<Interested>(id, new PartitionKey(id));

            return item.Resource;
        }
        catch(CosmosException)
        {
            return null;
        }
    }
    
    public async Task<Interested> Interested(Uri uri, string userId)
    {
        var id = Models.Interested.GenerateId(uri, userId);
        var item = await Container.CreateItemAsync(new Interested(uri, userId), new PartitionKey(id));
        return item.Resource;
    }

    public async Task Uninterested(Uri uri, string userId)
    {
        await Container.DeleteItemAsync<Interested>(Models.Interested.GenerateId(uri, userId), new PartitionKey(userId));
    }
}
