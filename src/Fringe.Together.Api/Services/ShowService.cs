namespace Fringe.Together.Api.Services;

using Extensions;
using Microsoft.Azure.Cosmos;
using Models;

public class ShowService
{
    private readonly CosmosClient _client;

    private Container Container => _client.GetContainer(WellKnown.Cosmos.Database,
        WellKnown.Cosmos.Containers.Shows);
    
    public ShowService(CosmosClient client)
    {
        _client = client;
    }
    
    public async Task<Show?> GetShow(Uri uri)
    {
        var key = CosmosExtensions.CreateKey(uri);

        try
        {
            return await Container.ReadItemAsync<Show>(key, new PartitionKey(key));
        }
        catch (CosmosException)
        {
            return null;
        }
    }

    public async Task UpsertShow(Show show)
    {
        await Container.UpsertItemAsync(show, new PartitionKey(show.Id));
    }
}
