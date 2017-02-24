using System.Threading.Tasks;
using Contracts.Entity;

namespace Contracts.Interfaces
{
    public interface IVideoService
    {
        Task<bool> Produce(Project project, ProjectFolderTypes folder, VideoProductionSettings settings);
    }
}