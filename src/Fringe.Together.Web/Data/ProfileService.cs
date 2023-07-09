namespace Fringe.Together.Web.Data;

using Microsoft.Azure.Cosmos;
using Models;

public class ProfileService
{
    public const string ProfileIdKey = "ProfileId";

    private readonly CosmosClient _client;

    private Container Container => _client.GetContainer(WellKnown.Cosmos.Database,
        WellKnown.Cosmos.Containers.Profile);

    public ProfileService(CosmosClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<Profile>> ListProfiles()
    {
        var items = Container.GetItemQueryIterator<Profile>();
        var profiles = new List<Profile>();

        do
        {
            var next = await items.ReadNextAsync();
            profiles.AddRange(next.Resource);
        } while (items.HasMoreResults);

        return profiles;
    }

    public async Task<Profile> GetProfile(string id)
    {
        var profile = await Container.ReadItemAsync<Profile>(id, new PartitionKey(id));
        return profile.Resource;
    }
    
    public async Task<Profile> CreateProfile(string name)
    {
        var id = Guid.NewGuid().ToString();
        var response = await Container.CreateItemAsync(new Profile(id, name), new PartitionKey(id));
        return response.Resource;
    }
}
