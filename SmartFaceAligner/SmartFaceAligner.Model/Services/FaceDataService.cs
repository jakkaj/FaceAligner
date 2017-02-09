using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Entity;
using Contracts.Interfaces;
using ExifLib;
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

        static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public FaceDataService(IFileRepo fileRepo, IFileManagementService fileManagementService)
        {
            _fileManagementService = fileManagementService;
            _fileRepo = fileRepo;
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

            var current = list.FirstOrDefault(_ => _.Hash == f.Hash);
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
            var result = new BlockingCollection<FaceData>();

            async Task LoaderOptimization(string f)
            {
                var data = await GetFaceData(p, f);
                if (data == null)
                {
                    return;
                }

                result.Add(data);
            }

            var tasks = new Queue<Func<Task>>();

            foreach (var f in files)
            {
                var fClose = f;
                tasks.Enqueue(()=>LoaderOptimization(fClose));
            }

            await tasks.Parallel(20);

            var returnList = result.OrderBy(_ => _.DateTaken).ToList();

            await _save(p);

            return returnList;
        }

        public async Task<FaceData> GetFaceData(Project p, string fileName)
        {
            var list = await _init(p);

            var hash = await _fileRepo.GetHash(fileName);

            var existing = list.FirstOrDefault(_ => _.Hash == hash);

            if (existing != null)
            {
                if (existing.DateTaken == DateTime.MinValue)
                {
                    existing.DateTaken = _getExifDateTime(fileName);
                }
                
                existing.Project = p;
                existing.FileName = fileName;
                return existing;
            }

            var faceData = new FaceData
            {
                FileName = fileName,
                Project = p,
                Hash = hash,
                DateTaken = _getExifDateTime(fileName)
            };


            return faceData;
        }

        DateTime _getExifDateTime(string fileName)
        {
            try
            {
                var exif = new ExifReader(fileName);
                DateTime datePictureTaken;
                if (exif.GetTagValue<DateTime>(ExifTags.DateTimeDigitized,
                    out datePictureTaken))
                {
                    return datePictureTaken;
                }
            }
            catch
            {
            }


            return DateTime.MaxValue;
        }

        async Task<string> _getKey(string fileName)
        {
            return await _fileRepo.GetHash(fileName);
        }
    }
}
