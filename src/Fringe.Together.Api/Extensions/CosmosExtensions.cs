namespace Fringe.Together.Api.Extensions;

public static class CosmosExtensions
{
    public static string CreateKey(Uri uri)
    {
        return uri.ToString()
            .TrimEnd('/')
            .ToLower()
            .Replace('/', '_')
            .Replace('\\', '_')
            .Replace('?', '_')
            .Replace('#', '_');
    }
}
