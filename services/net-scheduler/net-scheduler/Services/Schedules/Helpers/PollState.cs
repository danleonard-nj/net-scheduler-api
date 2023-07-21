namespace NetScheduler.Services.Schedules.Helpers;

public static class PollState
{
    public static bool IsPolling = false;

    public static void SetPolling(bool isPolling)
    {
        IsPolling = isPolling;
    }
}
