using Orleans.Streams;
using Telexistence.Interfaces;
using Telexistence.Models;

namespace Telexistence.Grains
{
    /// <summary>
    /// Represents a robot grain that manages robot state and streams commands/status using Orleans.
    /// Handles both direct commands and real-time VR streaming via SignalR->Orleans stream.
    /// </summary>
    public class RobotGrain : Grain, IRobotGrain, IAsyncObserver<GranularRobotCommand>
    {
        // Persistent state for this robot (MongoDB-backed via Orleans)
        private readonly IPersistentState<RobotStatus> _status;

        // Orleans stream provider for publishing/subscribing to streams
        private readonly IStreamProvider _streamProvider;

        // Stream for publishing robot status to clients
        private IAsyncStream<RobotStatus>? _statusStream;

        // Stream for subscribing to granular (VR) commands
        private IAsyncStream<GranularRobotCommand>? _controlStream;

        // Keeps a bounded history of the most recent granular stream commands for diagnostics/monitoring
        private readonly Queue<GranularRobotCommand> _recentStreamCommands = new();
        private const int StreamCommandHistoryLimit = 20;

        // Tracks last time a VR command was received (for control arbitration)
        private DateTime _lastVRHeartbeat = DateTime.MinValue;

        // If a VR command was received within this timeout, reject manual commands
        private readonly TimeSpan _vrTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Constructor for RobotGrain. Injects persistent state and stream provider.
        /// </summary>
        public RobotGrain(
            [PersistentState("robotState", "Mongo")] IPersistentState<RobotStatus> status,
            [FromKeyedServices("RobotStream")] IStreamProvider streamProvider)
        {
            _status = status;
            _streamProvider = streamProvider;
        }

        /// <summary>
        /// On activation, initialize streams and subscribe to control (VR command) stream.
        /// </summary>
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _statusStream = _streamProvider.GetStream<RobotStatus>(this.GetPrimaryKeyString(), "RobotStatus");
            _controlStream = _streamProvider.GetStream<GranularRobotCommand>(this.GetPrimaryKeyString(), "RobotControl");
            await _controlStream.SubscribeAsync(this); // Subscribe this grain as observer for incoming commands
            _status.State.RobotId = this.GetPrimaryKeyString();
        }

        /// <summary>
        /// On deactivation, persist robot state.
        /// </summary>
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            await _status.WriteStateAsync();
            await base.OnDeactivateAsync(reason, cancellationToken);
        }

        /// <summary>
        /// Executes a high-level command (e.g. Move, Rotate, Stop).
        /// If the robot is currently under VR control, throws an exception.
        /// </summary>
        public async Task ExecuteCommand(RobotCommand command)
        {
            if ((DateTime.UtcNow - _lastVRHeartbeat) < _vrTimeout)
                throw new InvalidOperationException("Robot is under VR control.");

            switch (command.CommandType)
            {
                case CommandType.Move:
                    _status.State.Task = $"Moving {command.Axis} axis";
                    _status.State.Status = "Moving";
                    Move(command.Axis, command.Distance);
                    break;
                case CommandType.Rotate:
                    _status.State.Task = $"Rotating {command.RotateAngle} degrees";
                    _status.State.Status = "Rotating";
                    Rotate(command.RotateAngle);
                    break;
                case CommandType.Stop:
                    _status.State.Task = "Stopped";
                    _status.State.Status = "Idle";
                    break;
            }
            _status.State.LastUpdate = DateTime.UtcNow;
            await _status.WriteStateAsync();
            await PublishStatusAsync();
        }

        /// <summary>
        /// Handles incoming granular (VR) commands from the control stream.
        /// Updates robot state, tracks stream command history, and publishes status.
        /// </summary>
        public async Task OnNextAsync(GranularRobotCommand cmd, StreamSequenceToken? token = null)
        {
            bool updateMongoStatus = false;
            if (!_status.State.Task.Contains("Stream")) updateMongoStatus = true; // if the stream is active, update status only once

            _lastVRHeartbeat = DateTime.UtcNow;
            _recentStreamCommands.Enqueue(cmd);
            if (_recentStreamCommands.Count > StreamCommandHistoryLimit)
                _recentStreamCommands.Dequeue();

            _status.State.X = (int)cmd.X;
            _status.State.Y = (int)cmd.Y;
            _status.State.Z = (int)cmd.Z;
            _status.State.Rotation = ((int)cmd.Rotation % 360 + 360) % 360;
            _status.State.Task = "VR Streaming";
            _status.State.Status = "VR Moving";
            _status.State.LastUpdate = DateTime.UtcNow;

            if(updateMongoStatus) await _status.WriteStateAsync();
            await PublishStatusAsync();
        }

        /// <summary>
        /// Handles stream completion notification (no-op).
        /// </summary>
        public async Task OnCompletedAsync()
        {
            // Save the recent stream commands to state
            _status.State.RecentStreamCommands = _recentStreamCommands.ToList();
            await _status.WriteStateAsync();
        }

        /// <summary>
        /// Handles stream error notification (no-op).
        /// </summary>
        public Task OnErrorAsync(Exception ex) => Task.CompletedTask;

        /// <summary>
        /// Gets the current robot status.
        /// </summary>
        public Task<RobotStatus> GetStatus() => Task.FromResult(_status.State);

        /// <summary>
        /// Gets the most recent granular stream commands.
        /// </summary>
        public Task<IReadOnlyCollection<GranularRobotCommand>> GetRecentStreamCommands()
            => Task.FromResult((IReadOnlyCollection<GranularRobotCommand>)_recentStreamCommands.ToArray());

        /// <summary>
        /// Applies a move command to the specified axis.
        /// </summary>
        private void Move(Axis axis, int distance)
        {
            switch (axis)
            {
                case Axis.X: _status.State.X += distance; break;
                case Axis.Y: _status.State.Y += distance; break;
                case Axis.Z: _status.State.Z += distance; break;
            }
        }

        /// <summary>
        /// Applies a rotation command to the robot.
        /// </summary>
        private void Rotate(int angle)
        {
            _status.State.Rotation = ((_status.State.Rotation + angle) % 360 + 360) % 360;
        }

        /// <summary>
        /// Publishes the current robot status to the status stream for real-time client updates.
        /// </summary>
        private async Task PublishStatusAsync()
        {
            if (_statusStream != null)
                await _statusStream.OnNextAsync(_status.State);
        }
    }
}
