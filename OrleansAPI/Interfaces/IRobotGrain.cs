using Orleans;
using System.Threading.Tasks;
using Telexistence.Models;

namespace Telexistence.Interfaces
{
    public interface IRobotGrain : IGrainWithStringKey
    {
        Task ExecuteCommand(RobotCommand command);
        Task<RobotStatus> GetStatus();
        Task<IReadOnlyCollection<GranularRobotCommand>> GetRecentStreamCommands();
    }
}
