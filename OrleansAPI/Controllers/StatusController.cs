
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Telexistence.Interfaces;

namespace Telexistence.Controllers {
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class StatusController : ControllerBase {
        private readonly IGrainFactory _grains;
        public StatusController(IGrainFactory grains) => _grains = grains;

        [HttpGet("{robotId}")]
        public async Task<IActionResult> Get(string robotId) {
            var robot = _grains.GetGrain<IRobotGrain>(robotId);
            return Ok(await robot.GetStatus());
        }
    }
}
