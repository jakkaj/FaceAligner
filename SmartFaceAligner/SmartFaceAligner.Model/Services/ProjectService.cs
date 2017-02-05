using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Entity;
using Contracts.Interfaces;
using Newtonsoft.Json;
using SmartFaceAligner.Processor.Entity;
using XamlingCore.Portable.Model.Response;

namespace SmartFaceAligner.Processor.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IFileRepo _fileRepo;

        public ProjectService(IFileRepo fileRepo)
        {
            _fileRepo = fileRepo;
        }

        public async Task<ProjectFolder> GetFolder(Project p, ProjectFolderTypes folderType)
        {
            var projectPath = await _getProjectFile(p.Name);
            
            //This will create the directory if it doesnt already exist
            var folder = await _fileRepo.GetOffsetFolder(projectPath, folderType.ToString(), ProcessorContstants.FileNames.FolderRoot);

            var projectFolder = new ProjectFolder
            {
                ProjectFolderType = folderType,
                Project = p,
                FolderPath = folder
            };

            return projectFolder;

        }

        async Task<string> _getProjectFile(string projectFile)
        {
            var projectPath = await _fileRepo.GetOffsetFile(projectFile, ProcessorContstants.FileNames.ProjectRoot);
            return projectPath;
        }

        public async Task<Project> CreateProject(string projectName, string projectDirectory)
        {
            var p = await GetProject(projectDirectory);
            p.Name = projectName;
            await SetProject(p);
            return p;
        }

        public async Task<Project>  GetProject(string projectDirectory)
        {
            return await _getProject(projectDirectory);
        }

        public async Task SetProject(Project p)
        { 
            await _fileRepo.Write(p.FilePath, JsonConvert.SerializeObject(p));
        }

        async Task<Project> _getProject(string projectDirectory)
        {
            var projectPath = await _getProjectFile(projectDirectory);

            if (await _fileRepo.FileExists(projectPath))
            {
                var projectData = await _fileRepo.ReadText(projectPath);
               
                var project = JsonConvert.DeserializeObject<Project>(projectData);
                project.FilePath = projectPath;
                return project;
            }

            var newProject = new Project
            {
                Id = Guid.NewGuid(),
                FilePath = projectPath
            };

            await SetProject(newProject);

            return newProject;
        }
    }
}
