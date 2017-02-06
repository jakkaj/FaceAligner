using System.Threading.Tasks;
using Contracts.Entity;

namespace Contracts.Interfaces
{
    public interface IProjectService
    {
        Task SetProject(Project p);
        Task<ProjectFolder> GetFolder(Project p, ProjectFolderTypes folderType);
        Task<Project> CreateProject(string projectName, string projectDirectory, string sourceDirectory);
        Task<Project> OpenProject(string projectFile);
    }
}