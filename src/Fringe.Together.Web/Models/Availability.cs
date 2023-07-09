namespace Fringe.Together.Web.Models;

using System.Globalization;

public record Availability(string Id, IEnumerable<DateOnly> AvailableDates)
{
    public IReadOnlyList<DateTime> AvailableDateTimes => AvailableDates
        .Select(x => x.ToDateTime(TimeOnly.MinValue))
        .ToList();
    
    public int Ttl { get; set; } = 60 * 60;

    public string ToString(int take)
    {
        var formattedDates = AvailableDates.Order().Take(take).Select(x => 
                $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(x.Month)} {x.Day}")
            .ToArray();

        if (AvailableDates.Count() > formattedDates.Length)
        {
            var moreDates = AvailableDates.Count() - formattedDates.Length;
            return string.Join(' ', formattedDates) + $"... {moreDates} more";
        }

        return string.Join(' ', formattedDates);
    }
}
