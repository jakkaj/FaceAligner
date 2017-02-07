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
        private readonly IFileRepo _fileRepo;

        public FileManagementService(IFileRepo fileRepo)
        {
            _fileRepo = fileRepo;
        }


        public async Task<ProjectFolder> GetFolder(Project p, ProjectFolderTypes folderType)
        {
            //This will create the directory if it doesnt already exist
            var folder = await _fileRepo.GetOffsetFolder(await _fileRepo.GetParentFolder(p.FilePath), folderType.ToString());

            var projectFolder = new ProjectFolder
            {
                ProjectFolderType = folderType,
                Project = p,
                FolderPath = folder
            };

            return projectFolder;
        }

        public async Task<string> GetSubFolder(Project p, ProjectFolderTypes folderType, params string[] subFolder)
        {
            //This will create the directory if it doesnt already exist
            var folder = await GetFolder(p, folderType);

            var s = new List<string>(subFolder);
            s.Insert(0, folder.FolderPath);

            var deeperFolder = await _fileRepo.GetOffsetFolder(s.ToArray());
            return deeperFolder;
        }

        public async Task<string> GetSubFile(Project p, ProjectFolderTypes folderType, params string[] subFolder)
        {
            //This will create the directory if it doesnt already exist
            var folder = await GetFolder(p, folderType);

            var s = new List<string>(subFolder);
            s.Insert(0, folder.FolderPath);

            var deeperFolder = await _fileRepo.GetOffsetFile(s.ToArray());
            return deeperFolder;

        }

        /// <summary>
        /// Copies the file to the subPath provided. Do not include target file name in the subPath... it uses the source filename. 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="p"></param>
        /// <param name="folderType"></param>
        /// <param name="subPath"></param>
        /// <returns></returns>
        public async Task CopyTo(string source, Project p, ProjectFolderTypes folderType, params string[] subPath)
        {
            var f = await GetSubFile(p, folderType, subPath);
           
            await _fileRepo.CopyFile(source, f);
        }

        public async Task<List<string>> GetSourceFiles(Project p)
        {
            var folder = p.SourceDirectory;

            if (!await _fileRepo.DirectoryExists(folder))
            {
                return null;
            }

            return await _fileRepo.GetFiles(p.SourceDirectory);
        }

        public async Task<bool> CopyToFolder(Project p, string fileName, ProjectFolderTypes folderType)
        {
            var baseFolder = await GetFolder(p, folderType);
           
            return await _fileRepo.CopyFile(fileName, baseFolder.FolderPath);
        }

        public async Task<int> ImportFolder(Project p, string sourceFolder)
        {
            if (!await _fileRepo.DirectoryExists(sourceFolder))
            {
                return -1;
            }

            var target = await GetFolder(p, ProjectFolderTypes.Staging);
            var result = await _fileRepo.CopyFolder(sourceFolder, target.FolderPath, new List<string>{".jpg", ".jpeg", ".png"});

            return result;
        }

        public async Task<bool> HasFiles(Project p, ProjectFolderTypes folderType)
        {
            var d = await GetFolder(p, folderType);
            return await _fileRepo.HasFiles(d.FolderPath);
        }

        public async Task DeleteFiles(Project p, ProjectFolderTypes folderType)
        {
            var d = await GetFolder(p, folderType);
            await _fileRepo.DeleteFiles(d.FolderPath);
        }
    }
}
