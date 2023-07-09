namespace Fringe.Together.Web.Models;

public record Profile(string Id, string Name)
{
    public int Ttl { get; set; } = 60 * 60 * 24 * 180;
}
