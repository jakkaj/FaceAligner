using System.Threading.Tasks;
using Contracts.Entity;

namespace Contracts.Interfaces
{
    public interface IProjectService
    {
        Task<Project>  GetProject(string projectName);
        Task SetProject(Project p);
    }
}