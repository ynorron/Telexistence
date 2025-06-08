using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Telexistence.Interfaces;
using Telexistence.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams;

namespace Telexistence.Controllers
{
    [ApiController]
    [Route("robot")]
    [Authorize]
    public class RobotController : ControllerBase
    {
        private readonly IGrainFactory _grains;
        private readonly IClusterClient _orleans;
        public RobotController(IGrainFactory grains, IClusterClient orleans)
        {
            _orleans = orleans;
            _grains = grains;
        }

        [HttpGet("{robotId}/status")]
        public async Task<IActionResult> GetStatus(string robotId)
        {
            var robot = _grains.GetGrain<IRobotGrain>(robotId);
            return Ok(await robot.GetStatus());
        }

        [HttpGet("{robotId}/recent-stream-commands")]
        public async Task<IActionResult> GetRecentStreamCommands(string robotId)
        {
            var robot = _grains.GetGrain<IRobotGrain>(robotId);
            var commands = await robot.GetRecentStreamCommands();
            return Ok(commands);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{robotId}/stream-command")]
        public async Task<IActionResult> SendStreamCommand(string robotId, [FromBody] GranularRobotCommand command)
        {
            var provider = _orleans.GetStreamProvider("RobotStream");
            var stream = provider.GetStream<GranularRobotCommand>(robotId, "RobotControl");
            await stream.OnNextAsync(command);
            return Ok(new { message = "Stream command sent via Orleans stream." });
        }
    }
}
