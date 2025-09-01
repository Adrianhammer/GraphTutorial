
namespace GraphTutorial.Services;

public class CalendarService
{
    public static async Task ListCalendarEventsAsync()
    {
        _ =  GraphAuthService._userClient ??
             throw new NullReferenceException("Graph has not been initialized for user auth");

        var result = await GraphAuthService._userClient.Me.Calendar.Events.GetAsync(config =>
        {
            config.QueryParameters.Select = new[] { "subject", "start", "end" };
            config.QueryParameters.Top = 5;
        });

        if (result == null)
        {
            Console.WriteLine("No calendar events found");
        }

        foreach (var events in result.Value)
        {
            Console.WriteLine($"Subject: {events.Subject}");
            Console.WriteLine($"    Start time: {events.Start.DateTime}");
            Console.WriteLine($"    End time: {events.End.DateTime}");
        }
    }
}