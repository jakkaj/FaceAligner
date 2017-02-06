using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Entity;
using Contracts.Interfaces;
using Newtonsoft.Json;
using SmartFaceAligner.Processor.Entity;
using XamlingCore.NET.Implementations;
using XamlingCore.Portable.Contract.Entities;

namespace SmartFaceAligner.Processor.Services
{
    public class FaceDataService : IFaceDataService
    {
        private IFileRepo _fileRepo { get; }
        private readonly IProjectService _projectService;
      

        public FaceDataService(IProjectService projectService, IFileRepo fileRepo)
        {
            _fileRepo = fileRepo;
            _projectService = projectService;
        }

        async Task<string> _getFile(Project p, string fileName)
        {
            var folder = await _projectService.GetFolder(p, ProjectFolderTypes.Data);

            return await _fileRepo.GetOffsetFile(folder.FolderPath, _getKey(fileName));
        }

        public async Task SetFaceData(FaceData f)
        {
            var data = JsonConvert.SerializeObject(f);
            await _fileRepo.Write(await _getFile(f.Project, f.FileName), data);
        }

        public async Task<FaceData> GetFaceData(Project p, string fileName)
        {
            var file = await _getFile(p, fileName);

            if (await _fileRepo.FileExists(file))
            {
                var data = await _fileRepo.ReadText(file);
                return JsonConvert.DeserializeObject<FaceData>(data);
            }

            return new FaceData { FileName = fileName, Project = p };
        }

        string _getKey(string fileName)
        {
            return HashHelper.CreateSHA1(fileName + Constants.Cache.FaceData);
        }
    }
}
