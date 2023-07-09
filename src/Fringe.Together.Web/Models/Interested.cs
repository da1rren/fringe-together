namespace Fringe.Together.Web.Models;

using Extensions;

public record Interested(Uri Uri, string UserId)
{
    public string Id { get; init; } = GenerateId(Uri, UserId);
    
    public static string GenerateId(Uri uri, string userId)
    {
        return (uri.ToString().ToLower() + userId.ToLower()).CreateMD5();
    }
}
