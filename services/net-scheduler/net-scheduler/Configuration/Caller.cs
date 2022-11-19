namespace NetScheduler.Configuration;
using System.Runtime.CompilerServices;

public static class Caller
{
    public static string GetName([CallerMemberName] string? memberName = null)
    {
        return memberName ?? string.Empty;
    }
}