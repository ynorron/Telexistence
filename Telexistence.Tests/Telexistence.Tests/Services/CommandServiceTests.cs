using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Orleans;
using Telexistence.Models;
using Telexistence.Interfaces;
using Telexistence.Repositories;

namespace Telexistence.Tests.Services
{
    public class CommandServiceTests
    {
        [Fact]
        public async Task SendCommandAsync_SavesAndExecutes()
        {
            var mockGrainFactory = new Mock<IGrainFactory>();
            var mockRepo = new Mock<ICommandRepository>();
            var mockGrain = new Mock<IRobotGrain>();

            mockGrainFactory
                .Setup(f => f.GetGrain<IRobotGrain>("TX-010", null))
                .Returns(mockGrain.Object);

            var service = new CommandService(mockGrainFactory.Object, mockRepo.Object);
            var command = new RobotCommand
            {
                RobotId = "TX-010",
                CommandType = CommandType.Move,
                Axis = "X",
                Distance = 5,
                User = "tester"
            };

            await service.SendCommandAsync(command);

            mockRepo.Verify(r => r.SaveCommandAsync(It.IsAny<RobotCommand>()), Times.Once);
            mockGrainFactory.Verify(f => f.GetGrain<IRobotGrain>("TX-010", null), Times.Once);
            mockGrain.Verify(g => g.ExecuteCommand(It.IsAny<RobotCommand>()), Times.Once);
        }

        [Fact]
        public async Task GetCommandHistoryAsync_ReturnsHistory()
        {
            var mockRepo = new Mock<ICommandRepository>();
            mockRepo.Setup(r => r.GetCommandHistoryAsync("robot1")).ReturnsAsync(
                new List<RobotCommand> {
                    new RobotCommand { RobotId = "robot1", CommandType = CommandType.Stop, User = "tester" }
                });

            var service = new CommandService(Mock.Of<IGrainFactory>(), mockRepo.Object);
            var result = await service.GetCommandHistoryAsync("robot1");

            Assert.Single(result);
            Assert.Equal("robot1", result[0].RobotId);
        }

        [Fact]
        public async Task GetCommandByIdAsync_ReturnsCommand()
        {
            var mockRepo = new Mock<ICommandRepository>();
            mockRepo.Setup(r => r.GetCommandByIdAsync("cmd1")).ReturnsAsync(
                new RobotCommand { Id = "cmd1", RobotId = "robot2", CommandType = CommandType.Rotate, User = "tester" });

            var service = new CommandService(Mock.Of<IGrainFactory>(), mockRepo.Object);
            var result = await service.GetCommandByIdAsync("cmd1");

            Assert.NotNull(result);
            Assert.Equal("cmd1", result.Id);
        }

        [Fact]
        public async Task UpdateCommandAsync_ReturnsNull_WhenNotFound()
        {
            var mockRepo = new Mock<ICommandRepository>();
            mockRepo.Setup(r => r.GetCommandByIdAsync("nope")).ReturnsAsync((RobotCommand?)null);

            var service = new CommandService(Mock.Of<IGrainFactory>(), mockRepo.Object);
            var result = await service.UpdateCommandAsync("nope", new RobotCommand
            {
                RobotId = "robotZ",
                CommandType = CommandType.Move,
                User = "tester"
            });

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateCommandAsync_Updates_WhenFound()
        {
            var existing = new RobotCommand { Id = "cmd2", RobotId = "old", CommandType = CommandType.Stop, User = "a" };
            var mockRepo = new Mock<ICommandRepository>();
            mockRepo.Setup(r => r.GetCommandByIdAsync("cmd2")).ReturnsAsync(existing);
            mockRepo.Setup(r => r.UpdateCommandAsync(It.IsAny<RobotCommand>())).Returns(Task.CompletedTask);

            var service = new CommandService(Mock.Of<IGrainFactory>(), mockRepo.Object);
            var update = new RobotCommand { Id = "cmd2", RobotId = "robotB", CommandType = CommandType.Rotate, User = "b" };
            var result = await service.UpdateCommandAsync("cmd2", update);

            Assert.NotNull(result);
            Assert.Equal("cmd2", result.Id);
            Assert.Equal("robotB", result.RobotId);
        }
    }
}
