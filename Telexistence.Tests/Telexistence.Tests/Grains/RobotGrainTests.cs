using System;
using System.Threading.Tasks;
using Xunit;
using Orleans.TestingHost;
using Telexistence.Interfaces;
using Telexistence.Models;
namespace Telexistence.Tests.Grains
{
    public class RobotGrainClusterTests : IClassFixture<ClusterFixture>
    {
        private readonly TestCluster _cluster;

        public RobotGrainClusterTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
        }

        [Fact]
        public async Task ExecuteCommand_MoveX_UpdatesX()
        {
            var grain = _cluster.GrainFactory.GetGrain<IRobotGrain>("TX-010");
            var command = new RobotCommand
            {
                RobotId = "TX-010",
                CommandType = CommandType.Move,
                Axis = Axis.X,
                Distance = 5,
                User = "tester"
            };
            await grain.ExecuteCommand(command);
            var status = await grain.GetStatus();
            Assert.Equal(5, status.X);
            Assert.Equal("Moving X", status.Task);
            Assert.Equal("Moving", status.Status);
        }

        [Fact]
        public async Task ExecuteCommand_MoveY_UpdatesY()
        {
            var grain = _cluster.GrainFactory.GetGrain<IRobotGrain>("TX-011");
            var command = new RobotCommand
            {
                RobotId = "TX-011",
                CommandType = CommandType.Move,
                Axis = Axis.Y,
                Distance = 10,
                User = "tester"
            };
            await grain.ExecuteCommand(command);
            var status = await grain.GetStatus();
            Assert.Equal(10, status.Y);
        }

        [Fact]
        public async Task ExecuteCommand_MoveZ_Negative_UpdatesZ()
        {
            var grain = _cluster.GrainFactory.GetGrain<IRobotGrain>("TX-012");
            var command = new RobotCommand
            {
                RobotId = "TX-012",
                CommandType = CommandType.Move,
                Axis = Axis.Z,
                Distance = -3,
                User = "tester"
            };
            await grain.ExecuteCommand(command);
            var status = await grain.GetStatus();
            Assert.Equal(-3, status.Z);
        }

        [Fact]
        public async Task ExecuteCommand_Rotate_360WrapsToZero()
        {
            var grain = _cluster.GrainFactory.GetGrain<IRobotGrain>("TX-020");
            var command = new RobotCommand
            {
                RobotId = "TX-020",
                CommandType = CommandType.Rotate,
                RotateAngle = 360,
                User = "tester"
            };
            await grain.ExecuteCommand(command);
            var status = await grain.GetStatus();
            Assert.Equal(0, status.Rotation);
            Assert.Equal("Rotating", status.Task);
            Assert.Equal("Rotating", status.Status);
        }

        [Fact]
        public async Task ExecuteCommand_Rotate_NegativeAngle_WrapsCorrectly()
        {
            var grain = _cluster.GrainFactory.GetGrain<IRobotGrain>("TX-021");
            var command = new RobotCommand
            {
                RobotId = "TX-021",
                CommandType = CommandType.Rotate,
                RotateAngle = -90,
                User = "tester"
            };
            await grain.ExecuteCommand(command);
            var status = await grain.GetStatus();
            Assert.Equal(270, status.Rotation);
        }

        [Fact]
        public async Task ExecuteCommand_Stop_SetsIdle()
        {
            var grain = _cluster.GrainFactory.GetGrain<IRobotGrain>("TX-030");
            var move = new RobotCommand
            {
                RobotId = "TX-030",
                CommandType = CommandType.Move,
                Axis = Axis.Y,
                Distance = 7,
                User = "tester"
            };
            await grain.ExecuteCommand(move);

            var stop = new RobotCommand
            {
                RobotId = "TX-030",
                CommandType = CommandType.Stop,
                User = "tester"
            };
            await grain.ExecuteCommand(stop);
            var status = await grain.GetStatus();
            Assert.Equal("Stopped", status.Task);
            Assert.Equal("Idle", status.Status);
        }

        [Fact]
        public async Task ExecuteCommand_Move_UnknownAxis_DoesNotChangePosition()
        {
            var grain = _cluster.GrainFactory.GetGrain<IRobotGrain>("TX-040");
            var command = new RobotCommand
            {
                RobotId = "TX-040",
                CommandType = CommandType.Move,
                Axis = (Axis)(-1), // Assigning an invalid axis value explicitly  
                Distance = 10,
                User = "tester"
            };
            await grain.ExecuteCommand(command);
            var status = await grain.GetStatus();
            Assert.Equal(0, status.X);
            Assert.Equal(0, status.Y);
            Assert.Equal(0, status.Z);
        }
    }

    // Orleans test cluster fixture 
    public class ClusterFixture : IDisposable
    {
        public TestCluster Cluster { get; }

        public ClusterFixture()
        {
            var builder = new TestClusterBuilder();
            builder.AddSiloBuilderConfigurator<TestSiloConfigurator>();
            Cluster = builder.Build();
            Cluster.Deploy();
        }

        public void Dispose()
        {
            Cluster.StopAllSilos();
        }

        // Configurator class to add in-memory storage as "Default"
        private class TestSiloConfigurator : ISiloConfigurator
        {
            public void Configure(ISiloBuilder siloBuilder)
            {
                // Register in-memory storage with the name "Default"
                siloBuilder.AddMemoryGrainStorage("Default");
            }
        }
    }
}