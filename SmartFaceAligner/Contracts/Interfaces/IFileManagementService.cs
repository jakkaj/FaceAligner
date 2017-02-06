using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Entity;

namespace Contracts.Interfaces
{
    public interface IFileManagementService
    {
        Task<int> ImportFolder(Project p, string sourceFolder);
        Task<bool> HasFiles(Project p, ProjectFolderTypes folderType);
        Task DeleteFiles(Project p, ProjectFolderTypes folderType);
        Task<List<string>> GetFiles(Project p, ProjectFolderTypes folderType);
    }
}