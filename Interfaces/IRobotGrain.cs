
using Orleans;
using Telexistence.Models;
using System.Threading.Tasks;

namespace Telexistence.Interfaces {
    public interface IRobotGrain : IGrainWithStringKey {
        Task ExecuteCommand(RobotCommand robotCommand);
        Task<RobotStatus> GetStatus();
    }
}
