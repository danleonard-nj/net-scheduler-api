//using AutoFixture;
//using AutoFixture.AutoMoq;
//using Microsoft.Extensions.Caching.Distributed;
//using Microsoft.Extensions.Logging.Abstractions;
//using Moq;
//using NetScheduler.Clients.Abstractions;
//using NetScheduler.Data.Abstractions;
//using NetScheduler.Data.Entities;
//using NetScheduler.Models.Schedules;
//using NetScheduler.Services.Schedules;
//using NetScheduler.Services.Schedules.Extensions;
//using NetScheduler.Services.Tasks.Abstractions;
//using System;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Xunit;

//namespace NetScheduler.Tests.Services
//{
//    public class ScheduleServiceTests : IClassFixture<WebApplicationFixture>
//    {
//        private readonly Mock<IScheduleRepository> _mockScheduleRepository;
//        private readonly Mock<IFeatureClient> _mockFeatureClient;
//        private readonly Mock<ITaskService> _mockTaskService;
//        private readonly Mock<IDistributedCache> _mockDistributedCache;

//        private readonly Fixture AutoFixture = (Fixture)new Fixture()
//            .Customize(new AutoMoqCustomization());

//        private readonly Fixture _autoFixture;
//        private readonly ScheduleService _scheduleService;

//        public ScheduleServiceTests(WebApplicationFixture webApplicationFixture)
//        {
//            _autoFixture = webApplicationFixture.AutoFixture;

//            _mockFeatureClient = new Mock<IFeatureClient>();
//            _mockScheduleRepository = new Mock<IScheduleRepository>();
//            _mockTaskService = new Mock<ITaskService>();
//            _mockDistributedCache = new Mock<IDistributedCache>();

//            _scheduleService = new ScheduleService(
//                _mockScheduleRepository.Object,
//                _mockTaskService.Object,
//                _mockFeatureClient.Object,
//                _mockDistributedCache.Object,
//                new NullLogger<ScheduleService>());
//        }

//        [Fact]
//        public async Task GetSchedule_GivenValidScheduleId_ReturnsSchedule()
//        {
//            // Arrange
//            var mockSchedule = _autoFixture.Create<ScheduleItem>();

//            _mockScheduleRepository
//                .Setup(x => x.Get(
//                    It.IsAny<string>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(mockSchedule);

//            _mockFeatureClient
//                .Setup(x => x.EvaluateFeature(
//                    It.IsAny<string>()))
//                .ReturnsAsync(true);

//            // Act
//            var result = await _scheduleService.GetSchedule(
//                Guid.NewGuid().ToString(),
//                default);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(mockSchedule.ScheduleId, result.ScheduleId);
//        }

//        [Fact]
//        public async Task CreateSchedule_GivenValidSchedule_ReturnsSuccess()
//        {
//            // Arrange
//            var createScheduleModel = _autoFixture
//                .Create<CreateScheduleModel>();

//            // Act
//            var scheduleModel = await _scheduleService.CreateSchedule(
//                createScheduleModel,
//                default);

//            // Assert
//            Assert.NotNull(scheduleModel);
//            Assert.Equal(scheduleModel.ScheduleName, createScheduleModel.ScheduleName);
//            Assert.Equal(scheduleModel.Cron, createScheduleModel.Cron);
//        }

//        [Fact]
//        public async Task GetSchedules_ReturnsSchedules()
//        {
//            // Arrange
//            var mockSchedules = _autoFixture
//                .CreateMany<ScheduleItem>();

//            _mockScheduleRepository
//                .Setup(x => x.GetAll(
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(mockSchedules);

//            // Act
//            var schedules = await _scheduleService.GetSchedules(
//                default);

//            // Assert
//            Assert.NotNull(schedules);
//            Assert.NotEmpty(schedules);
//            Assert.Equal(
//                mockSchedules.Count(),
//                schedules.Count());
//        }

//        [Fact]
//        public async void UpsertSchedule_GivenNonExistingSchedule_InsertsSchedule()
//        {
//            // Arrange
//            var mockScheduleModel = _autoFixture
//                .Create<ScheduleModel>();

//            var mockSchedule = mockScheduleModel
//                .ToEntity();

//            _mockScheduleRepository
//                .Setup(x => x.Get(
//                    It.IsAny<string>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(mockSchedule);

//            _mockScheduleRepository
//                .Setup(x => x.Replace(
//                    It.IsAny<ScheduleItem>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(mockSchedule);

//            // Act
//            var result = await _scheduleService.UpdateSchedule(
//                mockScheduleModel,
//                default);

//            // Assert
//            _mockScheduleRepository
//                .Verify(x => x.Replace(
//                    It.IsAny<ScheduleItem>(),
//                    It.IsAny<CancellationToken>()),
//                        Times.Once());
//        }

//        [Fact]
//        public async void DeleteSchedule_GivenExistingSchedule_ReturnsSuccess()
//        {
//            // Arrange
//            var fakeId = Guid.NewGuid()
//                .ToString();

//            var mockSchedule = _autoFixture
//                .Create<ScheduleItem>();

//            _mockScheduleRepository
//                .Setup(x => x.Get(
//                    It.IsAny<string>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(mockSchedule);

//            // Act
//            await _scheduleService.DeleteSchedule(
//                fakeId,
//                default);

//            // Assert
//            _mockScheduleRepository
//                .Verify(x => x.Delete(
//                    It.IsAny<string>(),
//                    It.IsAny<CancellationToken>()),
//                        Times.Once());
//        }

//        //[Fact]
//        //public async Task RunSchedule_GivenValidSchedule_ReturnsSuccess()
//        //{
//        //    // Arrange
//        //    var scheduleId = Guid.NewGuid()
//        //        .ToString();

//        //    var links = _autoFixture
//        //        .CreateMany<string>(10);

//        //    var mockScheduleToRun = _autoFixture
//        //        .Build<ScheduleItem>()
//        //        .With(x => x.Links, links)
//        //        .Create();

//        //    _mockScheduleRepository
//        //        .Setup(x => x.Get(
//        //            It.IsAny<string>(),
//        //            It.IsAny<CancellationToken>()))
//        //        .ReturnsAsync(mockScheduleToRun);

//        //    // Act
//        //    await _scheduleService.RunSchedule(
//        //        scheduleId,
//        //        default);

//        //    // Assert
//        //    _mockTaskService
//        //        .Verify(x => x.ExecuteTasksAsync(
//        //            It.IsAny<string>(),
//        //            It.IsAny<string>(),
//        //            It.IsAny<CancellationToken>()),
//        //                Times.AtLeastOnce());
//        //}

//        [Fact]
//        public async Task PollScheduleTest_GivenSchedulesToExecute_ExecutesSchedules()
//        {
//            // Arrange
//            var nextRuntime = DateTimeOffset
//                .UtcNow
//                .AddMinutes(-10)
//                .ToUnixTimeSeconds();

//            var futureRuntime = DateTimeOffset
//                .UtcNow
//                .AddMinutes(10)
//                .ToUnixTimeSeconds();

//            var schedulesToRun = _autoFixture
//                .Build<ScheduleItem>()
//                    .With(x => x.NextRuntime, (int?)nextRuntime)
//                    .With(x => x.Cron, "* * * * *")
//                    .With(x => x.IncludeSeconds, false)
//                    .With(x => x.Links, AutoFixture
//                        .CreateMany<string>(1))
//                .CreateMany(5)
//                .ToList();

//            var schedulesNotToRun = _autoFixture
//                .Build<ScheduleItem>()
//                    .With(x => x.NextRuntime, (int?)futureRuntime)
//                    .With(x => x.Cron, "* * * * *")
//                    .With(x => x.IncludeSeconds, false)
//                    .With(x => x.Links, AutoFixture
//                        .CreateMany<string>(1))
//                .CreateMany(5)
//                .ToList();

//            var schedules = schedulesToRun.Concat(
//                schedulesNotToRun);

//            _mockScheduleRepository
//                .Setup(x => x.GetAll(
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(schedules);

//            _mockFeatureClient
//                .Setup(x => x.EvaluateFeature(
//                    It.IsAny<string>()))
//                .ReturnsAsync(true);

//            // Act
//            var result = await _scheduleService.Poll(
//                default);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(5, result.Count());
//        }

//        [Fact]
//        public async Task PollScheduleTest_GivenScheduleToExecute_UpdatesScheduleValues()
//        {
//            // Arrange
//            var now = DateTimeOffset.UtcNow;
//            var lastRuntime = now.AddMinutes(-10);

//            var nextRuntime = now
//                .AddMinutes(-5)
//                .ToUnixTimeSeconds();

//            var schedule = _autoFixture
//                .Build<ScheduleItem>()
//                    .With(x => x.Cron, "* * * * *")
//                    .With(x => x.IncludeSeconds, false)
//                    .With(x => x.NextRuntime, (int?)nextRuntime)
//                    .With(x => x.Links, AutoFixture
//                        .CreateMany<string>(5))
//                .Create();

//            _mockFeatureClient
//                .Setup(x => x.EvaluateFeature(
//                    It.IsAny<string>()))
//                .ReturnsAsync(true);

//            _mockScheduleRepository
//                .Setup(x => x.GetAll(
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(new[] { schedule });

//            // Act
//            var result = await _scheduleService.Poll(
//                default);

//            await Task.Delay(1500);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(5, schedule.Queue.Count());
//            Assert.Equal(schedule.NextRuntime, schedule.Queue.FirstOrDefault());

//            var offsetNow = DateTimeOffset.Now.ToUnixTimeSeconds();

//            Assert.True(
//                schedule.LastRuntime < DateTimeOffset.Now
//                    .ToUnixTimeSeconds());
//            Assert.True(
//                schedule.NextRuntime > DateTimeOffset.Now
//                    .ToUnixTimeSeconds());

//            _mockScheduleRepository
//                .Verify(x => x.Replace(
//                    It.IsAny<ScheduleItem>(),
//                    It.IsAny<CancellationToken>()),
//                Times.Once());
//        }

//        [Fact]
//        public async Task PollSchedules_GivenNullNextRuntime_UpdatesScheduleValues()
//        {
//            // Arrange
//            var schedule = new ScheduleItem
//            {
//                ScheduleId = Guid.NewGuid().ToString(),
//                ScheduleName = Guid.NewGuid().ToString(),
//                IncludeSeconds = false,
//                Cron = "* * * * *",
//                Links = Enumerable.Empty<string>(),
//                LastRuntime = default,
//                NextRuntime = default,
//                Queue = Enumerable.Empty<int>(),
//            };

//            _mockScheduleRepository
//                .Setup(x => x.GetAll(
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(new[] { schedule });

//            _mockFeatureClient
//               .Setup(x => x.EvaluateFeature(
//                   It.IsAny<string>()))
//               .ReturnsAsync(true);

//            // Act
//            var result = await _scheduleService.Poll(default);

//            // Assert
//            _mockScheduleRepository.Verify(x => x.Replace(
//                It.Is<ScheduleItem>(x => x.NextRuntime != null),
//                It.IsAny<CancellationToken>()),
//                    Times.Once());
//        }

//        [Fact]
//        public async Task PollSchedules_GivenScheduleNotInvoked_ReturnsSucces()
//        {
//            // Arrange
//            var now = DateTimeOffset.UtcNow;

//            var nextRuntime = now
//                .AddMinutes(60)
//                .ToUnixTimeSeconds();

//            var schedule = new ScheduleItem
//            {
//                ScheduleId = Guid.NewGuid().ToString(),
//                IncludeSeconds = false,
//                Cron = "* * * * *",
//                NextRuntime = (int)nextRuntime
//            };

//            _mockFeatureClient
//                .Setup(x => x.EvaluateFeature(
//                    It.IsAny<string>()))
//                .ReturnsAsync(true);

//            _mockScheduleRepository
//                .Setup(x => x.GetAll(
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(new[] { schedule });

//            // Act
//            var result = await _scheduleService.Poll(default);

//            Assert.Empty(result);
//        }
//    }
//}