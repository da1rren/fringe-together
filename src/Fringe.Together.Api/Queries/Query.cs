namespace Fringe.Together.Api.Queries;

using AngleSharp;
using AngleSharp.Dom;
using Extensions;
using Models;
using Services;

public class Query
{
    public async Task<Show?> GetShow([Service] ShowService service, Uri uri)
    {
        // Prevent SSRF
        if (!uri.Host.Equals("tickets.edfringe.com", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var show = await service.GetShow(uri);

        if (show != null)
        {
            return show;
        }

        show = await GetShowDetails(uri);
        await service.UpsertShow(show);

        return show;
    }

    private async Task<Show> GetShowDetails(Uri uri)
    {
        var config = Configuration.Default.WithDefaultLoader();
        
        using var context = BrowsingContext.New(config);
        using var document = await context.OpenAsync(uri.ToString());

        var title = document.QuerySelector("h1")?
            .Text()
            .Trim();
        
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
        return new Show(id, uri, title, location, time, duration, date, description);
    }
}
