namespace Fringe.Together.Api.Models;

public record Show(string Id, Uri Uri, string? Title, string? Location, string? Time, string? Duration, string? Date, string? Description, int Ttl);
