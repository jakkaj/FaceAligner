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
        private readonly IFileManagementService _fileManagementService;
        private IFileRepo _fileRepo { get; }

        public FaceDataService(IFileRepo fileRepo, IFileManagementService fileManagementService)
        {
            _fileManagementService = fileManagementService;
            _fileRepo = fileRepo;
        }

        async Task<string> _getFile(Project p, string fileName)
        {
            var folder = await _fileManagementService.GetFolder(p, ProjectFolderTypes.Data);

            return await _fileRepo.GetOffsetFile(folder.FolderPath, await _getKey(fileName));
        }

        public async Task SetFaceData(FaceData f)
        {
            var data = JsonConvert.SerializeObject(f);
            await _fileRepo.Write(await _getFile(f.Project, f.FileName), data);
        }

        public async Task<List<FaceData>> GetFaceData(Project p )
        {
            var files = await _fileManagementService.GetSourceFiles(p);
            var result = new List<FaceData>();
            foreach (var f in files)
            {
                result.Add(await GetFaceData(p, f));
            }

            return result;
        }

        public async Task<FaceData> GetFaceData(Project p, string fileName)
        {
            var file = await _getFile(p, fileName);

            if (await _fileRepo.FileExists(file))
            {
                var data = await _fileRepo.ReadText(file);
                var fd = JsonConvert.DeserializeObject<FaceData>(data);
                fd.Project = p;
                return fd;
            }

            return new FaceData { FileName = fileName, Project = p };
        }

        async Task<string> _getKey(string fileName)
        {
            return await _fileRepo.GetHash(fileName);
        }
    }
}
