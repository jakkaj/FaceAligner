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
        public async Task<Project>  GetProject(string projectName)
        {
            return await _getProject(projectName);
        }

        public async Task SetProject(Project p)
        {
            var projectPath = await _fileRepo.GetOffsetFile(p.Name, ProcessorContstants.FileNames.ProjectRoot);
            await _fileRepo.Write(projectPath, JsonConvert.SerializeObject(p));
        }

        async Task<Project> _getProject(string projectName)
        {
            var projectPath = await _fileRepo.GetOffsetFile(projectName,ProcessorContstants.FileNames.ProjectRoot);
            if (await _fileRepo.FileExists(projectPath))
            {
                var projectData = await _fileRepo.ReadText(projectPath);
                var project = JsonConvert.DeserializeObject<Project>(projectData);
                return project;
            }

            var newProject = new Project
            {
                Id = Guid.NewGuid(),
                Name = projectName
            };

            await SetProject(newProject);

            return newProject;
        }
    }
}
