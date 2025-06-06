using Telexistence.Models;

namespace Telexistence.OrleansAPI.Interfaces
{
    public interface ICommandService
    {
        Task<object> SendCommandAsync(RobotCommand command);
        Task<List<RobotCommand>> GetCommandHistoryAsync(string robotId);
        Task<RobotCommand?> GetCommandByIdAsync(string id);
        Task<RobotCommand?> UpdateCommandAsync(string id, RobotCommand updatedCommand);
    }

}
