namespace Telexistence.OrleansAPI.Hubs
{
    using Microsoft.AspNetCore.SignalR;
    using Orleans;
    using Orleans.Streams;
    using Telexistence.Models;

    public class RobotControlHub : Hub
    {
        private readonly IClusterClient _orleans;

        public RobotControlHub(IClusterClient orleans)
        {
            _orleans = orleans;
        }

        // Called by client to send movement/command in real time
        public async Task SendCommand(string robotId, GranularRobotCommand command)
        {
            var provider = _orleans.GetStreamProvider("RobotStream");
            var stream = provider.GetStream<GranularRobotCommand>(robotId, "RobotControl");
            await stream.OnNextAsync(command);
        }
    }
}
