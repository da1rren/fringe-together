namespace Fringe.Together.Api.Services;

using AngleSharp;
using AngleSharp.Dom;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Extensions;
using Microsoft.Azure.Cosmos;
using Models;

public class ShowService
{
    private readonly CosmosClient _client;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IHttpClientFactory _factory;

    private Container Container => _client.GetContainer(WellKnown.Cosmos.Database,
        WellKnown.Cosmos.Containers.Shows);
    
    public ShowService(CosmosClient client, BlobServiceClient blobServiceClient, IHttpClientFactory factory)
    {
        _client = client;
        _blobServiceClient = blobServiceClient;
        _factory = factory;
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
    
    public async Task<Show> ScrapShowDetails(Uri uri)
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
