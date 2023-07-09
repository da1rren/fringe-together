namespace Fringe.Together.Web.Data;

using AngleSharp;
using AngleSharp.Dom;
using Extensions;
using Fringe.Together.Web;
using Fringe.Together.Web.Models;
using Microsoft.Azure.Cosmos;

public class AvailabilityService
{
    private readonly CosmosClient _client;
    private readonly IHttpClientFactory _factory;

    private Container Container => _client.GetContainer(WellKnown.Cosmos.Database,
        WellKnown.Cosmos.Containers.Availability);
    
    public AvailabilityService(CosmosClient client, IHttpClientFactory factory)
    {
        _client = client;
        _factory = factory;
    }
    
    public async Task<IEnumerable<Availability>> ListAvailability()
    {
        var items = Container.GetItemQueryIterator<Availability>();
        var availability = new List<Availability>();

        do
        {
            var next = await items.ReadNextAsync();
            availability.AddRange(next.Resource);
        } while (items.HasMoreResults);

        return availability;
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
            var availability = await ScrapeAvailabilityDetails(uri);
            await Container.UpsertItemAsync(availability, new PartitionKey(availability.Id));
            return availability;
        }
    }

    public async Task UpsertAvailability(Uri uri)
    {
        var isExisting = await GetAvailability(uri);
        if (isExisting != null)
        {
            return;
        }

        var availability = await ScrapeAvailabilityDetails(uri);
        await Container.UpsertItemAsync(availability, new PartitionKey(availability.Id));
    }
    
    public async Task DeleteAll()
    {
        var availability = await ListAvailability();
        var tasks = new List<Task>();
        
        foreach (var toDelete in availability)
        {
            tasks.Add(Container.DeleteItemAsync<Show>(toDelete.Id, new PartitionKey(toDelete.Id)));
        }

        await Task.WhenAll(tasks);
    }

    private async Task<Availability> ScrapeAvailabilityDetails(Uri uri)
    {
        var key = CosmosExtensions.CreateKey(uri);

        var client = _factory.CreateClient(WellKnown.Http.Availability);
        var html = await client.GetStringAsync(uri);

        var config = Configuration.Default;
        using var context = BrowsingContext.New(config);
        using var doc = await context.OpenAsync(req => req.Content(html));

        var freeDates = doc.QuerySelectorAll("span[class=tickets-free]");
        var availableDates = doc.QuerySelectorAll("span[class=available]");

        var ticketsAvailableAt =  freeDates.Concat(availableDates)
            .Select(x => x.Text().Trim())
            .Where(x => int.TryParse(x, out var _))
            .Select(x => new DateOnly(DateTime.Now.Year, 8, int.Parse(x)));
        
        return new Availability(key, ticketsAvailableAt);
    }
}
