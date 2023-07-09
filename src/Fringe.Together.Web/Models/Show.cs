namespace Fringe.Together.Web.Models;

public record Show(string Id, Uri Uri, string? Title, string ImgUri, string? Location, string? Time, 
    string? Duration, string? Date, string? Description)
{
    public Booking? Booking { get; set; }
    
    public string ShortDescription => Description.Length > 250 ? 
        Description.Substring(0, 250).TrimEnd() + "..." : 
        Description;
    
    public int Ttl { get; set; } = 60 * 60 * 24 * 60;
}

public record Booking(DateOnly Date)
{
    private string GetDaySuffix(int day)
    {
        switch (day)
        {
            case 1:
            case 21:
            case 31:
                return "st";
            case 2:
            case 22:
                return "nd";
            case 3:
            case 23:
                return "rd";
            default:
                return "th";
        }
    }

    public override string ToString()
    {
        return Date.Day + GetDaySuffix(Date.Day);
    }
}
