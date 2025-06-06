using Orleans;
using Telexistence.Interfaces;
using Telexistence.Repositories;
using Telexistence.Models;
using Telexistence.OrleansAPI.Interfaces;

public class CommandService : ICommandService
{
    private readonly IGrainFactory _grains;
    private readonly ICommandRepository _repository;

    public CommandService(IGrainFactory grains, ICommandRepository repository)
    {
        _grains = grains;
        _repository = repository;
    }

    public async Task<object> SendCommandAsync(RobotCommand command)
    {
        command.Id = Guid.NewGuid().ToString();
        // todo: verify it robotId is valid
        await _repository.SaveCommandAsync(command);
        var robot = _grains.GetGrain<IRobotGrain>(command.RobotId);
        await robot.ExecuteCommand(command);
        // todo: if command fails generate a follow up command or update existing command in DB 
        return new { status = "command accepted" };
    }

    public async Task<List<RobotCommand>> GetCommandHistoryAsync(string robotId)
        => await _repository.GetCommandHistoryAsync(robotId);

    public async Task<RobotCommand?> GetCommandByIdAsync(string id)
        => await _repository.GetCommandByIdAsync(id);

    public async Task<RobotCommand?> UpdateCommandAsync(string id, RobotCommand updatedCommand)
    {
        var existing = await _repository.GetCommandByIdAsync(id);
        if (existing == null)
            return null;

        updatedCommand.Id = id;
        await _repository.UpdateCommandAsync(updatedCommand);
        return updatedCommand;
    }
}
