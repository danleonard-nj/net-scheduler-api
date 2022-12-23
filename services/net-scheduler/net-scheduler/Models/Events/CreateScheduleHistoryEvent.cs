namespace NetScheduler.Models.Events;

public class CreateScheduleHistoryEvent : ApiEvent
{
    private const string Segment = "/History";

    public CreateScheduleHistoryEvent(
        string applicationBaseUrl,
        object headers,
        object? body)
    {
        Method = "POST";
        Body = body;
        Headers = headers;
        EventKey = "SchedulerHistoryApiEvent";

        Endpoint = new Flurl.Url(applicationBaseUrl)
            .AppendPathSegment(Segment);
    }
}