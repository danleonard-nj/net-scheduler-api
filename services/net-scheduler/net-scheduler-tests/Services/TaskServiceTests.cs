//using AutoFixture;
//using Microsoft.Extensions.Logging.Abstractions;
//using Moq;
//using NetScheduler.Clients.Abstractions;
//using NetScheduler.Data.Abstractions;
//using NetScheduler.Data.Models;
//using NetScheduler.Models;
//using NetScheduler.Tests;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Xunit;

//namespace NetScheduler.Services.Tests;
//public class TaskServiceTests : IClassFixture<WebApplicationFixture>
//{
//    private readonly Mock<ITaskRepository> _mockTaskRepository;
//    private readonly Mock<ITaskClient> _mockTaskClient;

//    private readonly Fixture _autoFixture;
//    private readonly TaskService _taskService;

//    public TaskServiceTests(WebApplicationFixture fixture)
//    {
//        _autoFixture = fixture.AutoFixture;

//        _mockTaskRepository = new Mock<ITaskRepository>();
//        _mockTaskClient = new Mock<ITaskClient>();

//        _taskService = new TaskService(
//            _mockTaskRepository.Object,
//            _mockTaskClient.Object,
//            new NullLogger<TaskService>());
//    }

//    [Fact]
//    public void GetTaskTest()
//    {
//        Assert.True(true);
//    }

//    [Fact]
//    public void CreateTaskTest()
//    {
//        Assert.True(true);
//    }

//    [Fact]
//    public void GetTasksTest()
//    {
//        Assert.True(true);
//    }

//    [Fact]
//    public void UpsertTaskTest()
//    {
//        Assert.True(true);
//    }

//    [Fact]
//    public void DeleteTaskTest()
//    {
//        Assert.True(true);
//    }

//    [Fact]
//    public async Task ExecuteTask_GivenExistingTask_ReturnsSuccess()
//    {
//        // Arrange
//        var mockTaskId = Guid
//            .NewGuid()
//            .ToString();

//        var mockTask = new ScheduleTask
//        {
//            TaskId = mockTaskId,
//            Payload = null,
//            Method = "GET",
//            Endpoint = "http://test/"
//        };

//        _mockTaskRepository
//            .Setup(x => x.Get(
//                It.IsAny<string>(),
//                It.IsAny<CancellationToken>()))
//            .ReturnsAsync(mockTask);

//        // Act
//        var result = await _taskService.ExecuteTask(
//            mockTaskId,
//            default);

//        // Assert
//        _mockTaskClient.Verify(
//            x => x.ExecuteTask(
//                It.Is<ScheduleTaskModel>(x => x.TaskId == mockTaskId),
//                It.IsAny<CancellationToken>()),
//                Times.Once());

//        Assert.Equal(mockTaskId, result.TaskId);
//    }
//}