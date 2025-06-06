using Orleans;
using Orleans.Runtime;
using Telexistence.Models;
using Telexistence.Interfaces;

namespace Telexistence.Grains
{
    public class RobotGrain : Grain, IRobotGrain
    {
        private readonly IPersistentState<RobotStatus> _status;

        // Constructor injection of the persistent state, name "robotState", storage provider "Default"
        public RobotGrain([PersistentState("robotState", "Default")] IPersistentState<RobotStatus> status)
        {
            _status = status;
        }
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            await _status.WriteStateAsync();
            await base.OnDeactivateAsync(reason, cancellationToken);
        }

        public Task ExecuteCommand(RobotCommand command)
        {
            switch (command.CommandType)
            {
                case CommandType.Move:
                    _status.State.Task = $"Moving {command.Axis}";
                    _status.State.Status = "Moving";
                    Move(command.Axis, command.Distance);
                    break;
                case CommandType.Rotate:
                    _status.State.Task = "Rotating";
                    _status.State.Status = "Rotating";
                    Rotate(command.RotateAngle);
                    break;
                case CommandType.Stop:
                    _status.State.Task = "Stopped";
                    _status.State.Status = "Idle";
                    break;
            }
            _status.WriteStateAsync();
            return Task.CompletedTask;
        }

        private void Move(string axis, int distance)
        {
            switch (axis.ToUpperInvariant())
            {
                case "X":
                    _status.State.X += distance;
                    break;
                case "Y":
                    _status.State.Y += distance;
                    break;
                case "Z":
                    _status.State.Z += distance;
                    break;
                default:
                    // Unknown axis, do nothing or handle error
                    break;
            }
        }

        private void Rotate(int angle)
        {
            _status.State.Rotation = ((_status.State.Rotation + angle) % 360 + 360) % 360;
        }
        public Task<RobotStatus> GetStatus() => Task.FromResult(_status.State);


    }
}