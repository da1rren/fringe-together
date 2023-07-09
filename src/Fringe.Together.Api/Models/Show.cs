namespace Fringe.Together.Api.Models;

public record Show(string Id, Uri Uri, string? Title, string? ImgUri, string? Location, string? Time, 
    string? Duration, string? Date, string? Description)
{
    [GraphQLIgnore] public int Ttl { get; set; } = 60 * 60 * 24;
}
