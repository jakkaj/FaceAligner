using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Entity;
using Contracts.Interfaces;

namespace SmartFaceAligner.Processor.Services
{
    public class FileManagementService : IFileManagementService
    {
        private readonly IProjectService _projectService;
        private readonly IFileRepo _fileRepo;

        public FileManagementService(IProjectService projectService, IFileRepo fileRepo)
        {
            _projectService = projectService;
            _fileRepo = fileRepo;
        }

        public async Task<List<string>> GetFiles(Project p, ProjectFolderTypes folderType)
        {
            var folder = await _projectService.GetFolder(p, ProjectFolderTypes.Staging);
            return await _fileRepo.GetFiles(folder.FolderPath);
        }

        public async Task<int> ImportFolder(Project p, string sourceFolder)
        {
            if (!await _fileRepo.DirectoryExists(sourceFolder))
            {
                return -1;
            }

            var target = await _projectService.GetFolder(p, ProjectFolderTypes.Staging);
            var result = await _fileRepo.CopyFolder(sourceFolder, target.FolderPath);

            return result;
        }

        public async Task<bool> HasFiles(Project p, ProjectFolderTypes folderType)
        {
            var d = await _projectService.GetFolder(p, folderType);
            return await _fileRepo.HasFiles(d.FolderPath);
        }

        public async Task DeleteFiles(Project p, ProjectFolderTypes folderType)
        {
            var d = await _projectService.GetFolder(p, folderType);
            await _fileRepo.DeleteFiles(d.FolderPath);
        }
    }
}
