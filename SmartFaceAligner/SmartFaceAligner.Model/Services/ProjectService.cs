using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly IFileManagementService _fileManagementService;
        private readonly IFaceDataService _faceDataService;

        public ProjectService(IFileRepo fileRepo, IFileManagementService fileManagementService, IFaceDataService faceDataService)
        {
            _fileRepo = fileRepo;
            _fileManagementService = fileManagementService;
            _faceDataService = faceDataService;
        }

        public async Task<Project> OpenProject(string projectFile)
        {
            return await _getProject(projectFile);
        }

        public async Task AddImageToPerson(IdentityPerson personGroup, FaceData f)
        {
            var p = personGroup.Project;
            await _fileManagementService.CopyTo(f.FileName, p, ProjectFolderTypes.RecPerson, personGroup.PersonName);
            await _initIdentityPersons(p);
        }

        public async Task RemoveImageFromPerson(IdentityPerson personGroup, FaceData f)
        {
            await _fileRepo.DeleteFile(f.FileName);
            await _initIdentityPersons(personGroup.Project);
        }

        public async Task<IdentityPerson> AddNewIdentityPerson(Project p, string groupName)
        {
            if (p.IdentityPeople == null)
            {
                return null;
            }

            if(p.IdentityPeople.Any(_=>_.PersonName.ToLower() == groupName.ToLower()))
            {
                return null;
            }

            var ip = new IdentityPerson
            {
                PersonName = groupName
            };

            p.IdentityPeople.Add(ip);

            await SetProject(p);
            await _initIdentityPersons(p);

            return ip;
        }

        public async Task RemoveIdentityPerson(IdentityPerson p)
        {
            var project = p.Project;
            project.IdentityPeople.Remove(p);
            await SetProject(project);
            await _initIdentityPersons(project);

            var baseGroupFolder = await _fileManagementService.GetSubFolder(project, ProjectFolderTypes.RecPerson, p.PersonName);
            await _fileRepo.DeleteDirectory(baseGroupFolder);
        }

        async Task _initIdentityPersons(Project p)
        {
            if (p.IdentityPeople == null)
            {
                p.IdentityPeople = new List<IdentityPerson>();
            }

            var baseGroupFolder = await _fileManagementService.GetFolder(p, ProjectFolderTypes.RecPerson);

            foreach (var g in p.IdentityPeople)
            {
                g.Faces = new List<FaceData>();

                var folder = await _fileRepo.GetOffsetFolder(baseGroupFolder.FolderPath, g.PersonName);

                g.FolderPath = folder;
                g.Project = p;

                var files = await _fileRepo.GetFiles(folder);

                foreach (var f in files)
                {
                    var data = await _faceDataService.GetFaceData(p, f);
                    g.Faces.Add(data);
                }
            }
        }

        async Task<string> _getProjectFile(string projectFile)
        {
            var attr = File.GetAttributes(projectFile);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var projectPath = await _fileRepo.GetOffsetFile(projectFile, Constants.FileNames.ProjectRoot);
                return projectPath;
            }

            return projectFile;
        }

        public async Task<Project> CreateProject(string projectName, string projectDirectory, string sourceDirectory)
        {
            var p = await _getProject(projectDirectory);
            p.Name = projectName;
            p.SourceDirectory = sourceDirectory;
            await _initIdentityPersons(p);
            await SetProject(p);
            return p;
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
                await _initIdentityPersons(project);
                return project;
            }

            var newProject = new Project
            {
                Id = Guid.NewGuid(),
                FilePath = projectPath
            };

            await _initIdentityPersons(newProject);
            await SetProject(newProject);

            return newProject;
        }
    }
}
