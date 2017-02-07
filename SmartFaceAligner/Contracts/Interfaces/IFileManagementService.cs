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
        
        Task<bool> CopyToFolder(Project p, string fileName, ProjectFolderTypes folderType);

        /// <summary>
        /// Copies the file to the subPath provided. Do not include target file name in the subPath... it uses the source filename. 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="p"></param>
        /// <param name="folderType"></param>
        /// <param name="subPath"></param>
        /// <returns></returns>
        Task CopyTo(string source, Project p, ProjectFolderTypes folderType, params string[] subPath);

        Task<ProjectFolder> GetFolder(Project p, ProjectFolderTypes folderType);
        Task<string> GetSubFolder(Project p, ProjectFolderTypes folderType, params string[] subFolder);
        Task<string> GetSubFile(Project p, ProjectFolderTypes folderType, params string[] subFolder);
        Task<List<string>> GetSourceFiles(Project p);
    }
}