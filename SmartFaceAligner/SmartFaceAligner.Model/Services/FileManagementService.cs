using System;
using System.Collections.Generic;
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
    }
}
