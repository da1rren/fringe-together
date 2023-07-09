namespace Fringe.Together.Web.Data;

using AngleSharp;
using AngleSharp.Dom;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Fringe.Together.Web;
using Fringe.Together.Web.Extensions;
using Fringe.Together.Web.Models;
using Microsoft.Azure.Cosmos;

public class ShowService
{
    private readonly CosmosClient _client;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IHttpClientFactory _factory;
    private readonly AppState _appState;

    private Container Container => _client.GetContainer(WellKnown.Cosmos.Database,
        WellKnown.Cosmos.Containers.Shows);
    
    public ShowService(CosmosClient client, BlobServiceClient blobServiceClient, IHttpClientFactory factory,
        AppState appState)
    {
        _client = client;
        _blobServiceClient = blobServiceClient;
        _factory = factory;
        _appState = appState;
    }

    public async Task<IEnumerable<Show>> ListShows()
    {
        var items = Container.GetItemQueryIterator<Show>();
        var shows = new List<Show>();

        do
        {
            var next = await items.ReadNextAsync();
            shows.AddRange(next.Resource);
        } while (items.HasMoreResults);

        return shows;
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

    public async Task Book(Uri uri, DateOnly bookingDate)
    {
        var show = await GetShow(uri);

        if (show == null)
        {
            return;
        }
        
        show.Booking = new Booking(bookingDate);
        await Container.UpsertItemAsync(show, new PartitionKey(show.Id));
        await _appState.NotifyStateChanged();
    }

    public async Task CancelBooking(Uri uri)
    {
        var show = await GetShow(uri);
        
        if (show == null)
        {
            return;
        }
        
        show.Booking = null;
        await Container.UpsertItemAsync(show, new PartitionKey(show.Id));
        await _appState.NotifyStateChanged();
    }

    public async Task UpsertShow(Uri uri)
    {
        var isExisting = await GetShow(uri);

        if (isExisting != null)
        {
            return;
        }
        
        var show = await ScrapShowDetails(uri);
        
        await Container.UpsertItemAsync(show, new PartitionKey(show.Id));
    }

    public async Task DeleteAll()
    {
        var shows = await ListShows();
        var tasks = new List<Task>();
        
        foreach (var show in shows)
        {
            tasks.Add(Container.DeleteItemAsync<Show>(show.Id, new PartitionKey(show.Id)));
        }

        await Task.WhenAll(tasks);
    }
    
    private async Task<Show> ScrapShowDetails(Uri uri)
    {
        var config = Configuration.Default.WithDefaultLoader();
        
        using var context = BrowsingContext.New(config);
        using var document = await context.OpenAsync(uri.ToString());

        var title = document.QuerySelector("h1")?
            .Text()
            .Trim();

        var img = document.QuerySelector("img[itemprop=image]")?
            .Attributes["src"]
            ?.Value;

        if (!string.IsNullOrEmpty(img))
        {
            var client = _factory.CreateClient();
            var stream = await client.GetStreamAsync(img);
            var name = uri.ToString().CreateMD5();

            var blobClient = _blobServiceClient.GetBlobContainerClient("imgs")
                .GetBlobClient(name);

            if (!(await blobClient.ExistsAsync()))
            {
                await blobClient.UploadAsync(stream);
            }
            
            var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.Now.AddYears(1));
            
            img = sasUri.ToString();
        }
        
        var location = document.QuerySelector("li[itemprop=location]")?
            .Text()
            .Replace("\n", " ")
            .Trim()
            .SanitizeWhitespace();
        
        var time = document.QuerySelector("li[title=Time]")?
            .Text()
            .Trim();
        
        var date = document.QuerySelector("li[title=Date]")?
            .Text()
            .Trim();
        
        var duration = document.QuerySelector("li[title=Duration]")?
            .Text()
            .Trim();
        
        var description = document.QuerySelector("span[itemprop=description]")?
            .Text()
            .Trim();
        
        var id = CosmosExtensions.CreateKey(uri);
        return new Show(id, uri, title, img, location, time, duration, date, description);
    }
}
