using Telexistence.Models;

public interface ICommandRepository
{
    Task SaveCommandAsync(RobotCommand command);
    Task<List<RobotCommand>> GetCommandHistoryAsync(string robotId);
    Task<RobotCommand?> GetCommandByIdAsync(string id);
    Task UpdateCommandAsync(RobotCommand command);
}
