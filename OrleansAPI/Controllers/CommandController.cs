using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telexistence.Models;
using Telexistence.OrleansAPI.Interfaces;

namespace Telexistence.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class CommandController : ControllerBase
    {
        private readonly ICommandService _commandService;
        public CommandController(ICommandService commandService)
        {
            _commandService = commandService;
        }

        /// <summary>
        /// Sends a command to control a robot (move, rotate, stop).
        /// </summary>
        /// <remarks>
        /// Example request:<br/>
        /// {<br/>
        ///   "robotId": "TX-010",<br/>
        ///   "commandType": "Move",<br/>
        ///   "axis": "Z",<br/>
        ///   "distance": 5,<br/>
        ///   "user": "test" <br/>
        /// }
        /// </remarks>
        /// <param name="command">Command to send</param>
        /// <returns>Status of the command request</returns>
        /// <response code="200">Command accepted</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> Post([FromBody] RobotCommand command)
        {
            if (command.CommandType == CommandType.Move && command.Axis == null)
                return BadRequest(new { error = "Axis is required for Move commands." });
            else if (command.CommandType == CommandType.Rotate && command.RotateAngle == null)
                return BadRequest(new { error = "Axis is required for Move commands." });
            if (!Enum.IsDefined(typeof(Axis), command.Axis))
                return BadRequest(new { error = "Invalid axis value." });

            var result = await _commandService.SendCommandAsync(command);
            return Ok(result);
        }

        /// <summary>
        /// Gets the command history for a specific robot.
        /// </summary>
        /// <param name="robotId">Robot's unique ID</param>
        /// <returns>List of previous commands</returns>
        [HttpGet("{robotId}/history")]
        [ProducesResponseType(typeof(List<RobotCommand>), 200)]
        public async Task<IActionResult> GetHistory(string robotId)
            => Ok(await _commandService.GetCommandHistoryAsync(robotId));

        /// <summary>
        /// Retrieves the details of a specific command by ID.
        /// </summary>
        /// <param name="id">The unique command ID.</param>
        /// <returns>The command details, or 404 if not found.</returns>
        /// <response code="200">Returns the command details</response>
        /// <response code="404">Command not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RobotCommand), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(string id)
        {
            var command = await _commandService.GetCommandByIdAsync(id);
            if (command == null)
                return NotFound();
            return Ok(command);
        }

        /// <summary>
        /// Updates an existing command by ID.
        /// </summary>
        /// <param name="id">The unique command ID to update.</param>
        /// <param name="updatedCommand">The updated command details.</param>
        /// <returns>The updated command, or 404 if not found.</returns>
        /// <response code="200">Command updated successfully</response>
        /// <response code="404">Command not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(RobotCommand), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Put(string id, [FromBody] RobotCommand updatedCommand)
        {
            var result = await _commandService.UpdateCommandAsync(id, updatedCommand);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
    }
}
