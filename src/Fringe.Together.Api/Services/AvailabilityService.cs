namespace Fringe.Together.Api.Services;

using Extensions;
using Microsoft.Azure.Cosmos;
using Models;

public class AvailabilityService
{
    private readonly CosmosClient _client;

    private Container Container => _client.GetContainer(WellKnown.Cosmos.Database,
        WellKnown.Cosmos.Containers.Availability);
    
    public AvailabilityService(CosmosClient client)
    {
        _client = client;
    }
    
    public async Task<Availability?> GetAvailability(Uri uri)
    {
        var key = CosmosExtensions.CreateKey(uri);

        try
        {
            return await Container.ReadItemAsync<Availability>(key, new PartitionKey(key));
        }
        catch (CosmosException)
        {
            return null;
        }
    }

    public async Task UpsertAvailability(Availability availability)
    {
        await Container.UpsertItemAsync(availability, new PartitionKey(availability.Id));
    }
}
