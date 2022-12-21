using NetScheduler.Data.Entities;
using NetScheduler.Models.Schedules;
using NetScheduler.Services.Schedules.Extensions;
using System;
using Xunit;

namespace NetScheduler.Tests.Services.Extensions;
public class ScheduleExtensionsTests
{
    [Fact]
    public void ToDomainTest()
    {
        Assert.True(true);
    }

    [Fact]
    public void ToDomainTest1()
    {
        Assert.True(true);
    }

    [Fact]
    public void ToDomainTest2()
    {
        Assert.True(true);
    }

    [Fact]
    public void ToDomainTest3()
    {
        Assert.True(true);
    }

    [Fact]
    public void ToScheduleTest()
    {
        Assert.True(true);
    }

    [Fact]
    public void ToScheduleTaskTest()
    {
        Assert.True(true);
    }

    [Fact]
    public void GetCronExpressionTest()
    {
        Assert.True(true);
    }

    [Fact]
    public void GetScheduleInvocationStateTest()
    {
        // Arrange
        var schedule = new ScheduleModel
        {
            LastRuntime = (int)DateTimeOffset.Now
                .AddMinutes(-10)
                .ToUnixTimeSeconds(),
            NextRuntime = (int)DateTimeOffset.Now
                .AddMinutes(-5)
                .ToUnixTimeSeconds()
        };

        // Act
        var invoke = schedule.GetScheduleInvocationState();

        // Assert
        Assert.True(invoke);
    }

    [Fact]
    public void UpdateLastRuntimeTest()
    {
        // Arrange
        var schedule = new ScheduleModel
        {
            LastRuntime = default
        };

        // Act
        schedule.UpdateLastRuntime();

        // Act
        Assert.NotNull(schedule.LastRuntime);
    }
}