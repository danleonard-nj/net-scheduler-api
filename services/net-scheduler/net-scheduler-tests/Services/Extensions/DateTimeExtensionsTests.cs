using NetScheduler.Services.Extensions;
using System;
using Xunit;

namespace NetScheduler.Tests.Services.Extensions;
public class DateTimeExtensionsTests
{
    [Fact]
    public void ToLocalDateTimeTest()
    {
        var timestamp = 1655657845;
        var localTime = new DateTime(
            2022, 6, 19, 9, 57, 25);

        var result = timestamp.ToLocalDateTime();

        Assert.Equal(localTime, result);
    }
}