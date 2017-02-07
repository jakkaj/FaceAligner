using System.Threading.Tasks;
using Contracts.Entity;

namespace Contracts.Interfaces
{
    public interface IProjectService
    {
        Task SetProject(Project p);
       
        Task<Project> CreateProject(string projectName, string projectDirectory, string sourceDirectory);
        Task<Project> OpenProject(string projectFile);


        Task AddImageToPerson(IdentityPerson personGroup, FaceData f);
        Task RemoveImageFromPerson(IdentityPerson personGroup, FaceData f);
        Task<IdentityPerson> AddNewIdentityPerson(Project p, string groupName);
        Task RemoveIdentityPerson(IdentityPerson p);
    }
}