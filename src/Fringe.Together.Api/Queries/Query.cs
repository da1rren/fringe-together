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
        
        show = await service.ScrapShowDetails(uri);
        await service.UpsertShow(show);

        return show;
    }

}
