namespace Fringe.Together.Api.Models;

public record Availability(string Id, IEnumerable<DateOnly> AvailableDates, int Ttl);
