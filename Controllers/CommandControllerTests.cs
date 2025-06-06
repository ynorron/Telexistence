using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telexistence.Controllers;
using Telexistence.Interfaces;
using Telexistence.Models;
using Telexistence.OrleansAPI.Interfaces;

namespace Telexistence.Tests.Controllers
{
    public class CommandControllerTests
    {
        [Fact]
        public async Task Post_ReturnsOk()
        {
            var mockService = new Mock<ICommandService>();
            mockService.Setup(s => s.SendCommandAsync(It.IsAny<RobotCommand>()))
                .ReturnsAsync(new { status = "command accepted" });

            var controller = new CommandController(mockService.Object);

            var cmd = new RobotCommand
            {
                RobotId = "TX-010",
                CommandType = CommandType.Move,
                Axis = "Z",
                Distance = 5,
                User = "test"
            };

            var result = await controller.Post(cmd);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("command accepted", ((dynamic)ok.Value).status);
        }

        [Fact]
        public async Task GetHistory_ReturnsCommands()
        {
            var mockService = new Mock<ICommandService>();
            mockService.Setup(s => s.GetCommandHistoryAsync("robotZ")).ReturnsAsync(
                new List<RobotCommand> {
                    new RobotCommand { RobotId = "robotZ", CommandType = CommandType.Move, User = "userA" }
                });

            var controller = new CommandController(mockService.Object);

            var result = await controller.GetHistory("robotZ");

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsAssignableFrom<List<RobotCommand>>(ok.Value);
            Assert.Single(value);
            Assert.Equal("robotZ", value[0].RobotId);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenMissing()
        {
            var mockService = new Mock<ICommandService>();
            mockService.Setup(s => s.GetCommandByIdAsync("nope")).ReturnsAsync((RobotCommand?)null);

            var controller = new CommandController(mockService.Object);

            var result = await controller.Get("nope");
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Get_ReturnsOk_WhenFound()
        {
            var mockService = new Mock<ICommandService>();
            mockService.Setup(s => s.GetCommandByIdAsync("cmdX")).ReturnsAsync(
                new RobotCommand { Id = "cmdX", RobotId = "r", CommandType = CommandType.Rotate, User = "userB" });

            var controller = new CommandController(mockService.Object);

            var result = await controller.Get("cmdX");
            var ok = Assert.IsType<OkObjectResult>(result);
            var cmd = Assert.IsType<RobotCommand>(ok.Value);
            Assert.Equal("cmdX", cmd.Id);
        }

        [Fact]
        public async Task Put_ReturnsNotFound_WhenMissing()
        {
            var mockService = new Mock<ICommandService>();
            mockService.Setup(s => s.UpdateCommandAsync("nope", It.IsAny<RobotCommand>()))
                .ReturnsAsync((RobotCommand?)null);

            var controller = new CommandController(mockService.Object);

            var result = await controller.Put("nope", new RobotCommand
            {
                RobotId = "nope",
                CommandType = CommandType.Stop,
                User = "test"
            });
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Put_ReturnsOk_WhenFound()
        {
            var update = new RobotCommand
            {
                Id = "c2",
                RobotId = "r2",
                CommandType = CommandType.Move,
                User = "u2"
            };
            var mockService = new Mock<ICommandService>();
            mockService.Setup(s => s.UpdateCommandAsync("c2", It.IsAny<RobotCommand>()))
                .ReturnsAsync(update);

            var controller = new CommandController(mockService.Object);

            var result = await controller.Put("c2", update);
            var ok = Assert.IsType<OkObjectResult>(result);
            var cmd = Assert.IsType<RobotCommand>(ok.Value);
            Assert.Equal("c2", cmd.Id);
        }
    }
}
