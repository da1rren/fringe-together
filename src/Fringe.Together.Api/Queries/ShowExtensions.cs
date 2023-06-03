namespace Fringe.Together.Api.Queries;

using AngleSharp;
using AngleSharp.Dom;
using Fringe.Together.Api.Models;
using Services;

[ExtendObjectType(typeof(Show))]
public class ShowExtensions
{
    public async Task<Availability?> GetAvailability([Service] AvailabilityService availabilityService,
        [Service] IHttpClientFactory factory,
        [Parent] Show show)
    {
        var availability = await availabilityService.GetAvailability(show.Uri);

        if (availability != null)
        {
            return availability;
        }

        var client = factory.CreateClient(WellKnown.Http.Availability);
        var html = await client.GetStringAsync(show.Uri);

        var config = Configuration.Default;
        using var context = BrowsingContext.New(config);
        using var doc = await context.OpenAsync(req => req.Content(html));

        var availableDates = doc.QuerySelectorAll("span[class=available]")
            .Select(x => x.Text().Trim())
            .Where(x => int.TryParse(x, out var _))
            .Select(x => new DateOnly(DateTime.Now.Year, 8, int.Parse(x)));
        
        availability = new Availability(show.Id, availableDates, 60 * 60);
        await availabilityService.UpsertAvailability(availability);
        return availability;
    }
}
