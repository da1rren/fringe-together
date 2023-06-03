namespace Fringe.Together.Api.Models;

public record Availability(string Id, IEnumerable<DateOnly> AvailableDates)
{
    [GraphQLIgnore] public int Ttl { get; set; } = 60 * 60;
}
