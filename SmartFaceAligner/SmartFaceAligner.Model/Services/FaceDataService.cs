using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        private Dictionary<Guid, List<FaceData>> _faceData = new Dictionary<Guid, List<FaceData>>();

        private IFileRepo _fileRepo { get; }

        private SemaphoreSlim _semaphore;

        public FaceDataService(IFileRepo fileRepo, IFileManagementService fileManagementService)
        {
            _fileManagementService = fileManagementService;
            _fileRepo = fileRepo;
            _semaphore = new SemaphoreSlim(1,1);
        }

        async Task<List<FaceData>> _init(Project p)
        {
            if (!_faceData.ContainsKey(p.Id))
            {
                var file = await _fileManagementService.GetSubFile(p, ProjectFolderTypes.Data, Constants.Cache.FaceData);

                if (await _fileRepo.FileExists(file))
                {
                    var dLoaded = await _fileRepo.ReadText(file);
                    _faceData[p.Id] = JsonConvert.DeserializeObject<List<FaceData>>(dLoaded);
                }
                else
                {
                    _faceData[p.Id] = new List<FaceData>();
                }
            }

            return _faceData[p.Id];
        }

        async Task _save(Project p)
        {
            if (_faceData.ContainsKey(p.Id))
            {
                var file = await _fileManagementService.GetSubFile(p, ProjectFolderTypes.Data, Constants.Cache.FaceData);
               
                var ser = JsonConvert.SerializeObject(_faceData[p.Id]);
               
                await _fileRepo.Write(file, ser);
            }
        }

       

        public async Task SetFaceData(FaceData f)
        {
            await _semaphore.WaitAsync();
            var list = await _init(f.Project);
            var current = list.FirstOrDefault(_ => _.FileName == f.FileName);
            if (current != null)
            {
                list.Remove(current);
            }

            list.Add(f);
            await _save(f.Project);
            _semaphore.Release();
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
            var list = await _init(p);

            var existing = list.FirstOrDefault(_ => _.FileName == fileName);

            if (existing != null)
            {
                return existing;
            }

            return new FaceData { FileName = fileName, Project = p };
        }

        async Task<string> _getKey(string fileName)
        {
            return await _fileRepo.GetHash(fileName);
        }
    }
}
