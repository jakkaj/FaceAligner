using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private readonly ILogService _logService;

        private Dictionary<Guid, List<FaceData>> _faceData = new Dictionary<Guid, List<FaceData>>();
        private Dictionary<Guid, ConcurrentBag<string>> _fileHash = new Dictionary<Guid, ConcurrentBag<string>>();

        private IFileRepo _fileRepo { get; }

        static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public FaceDataService(IFileRepo fileRepo, IFileManagementService fileManagementService, ILogService logService)
        {
            _fileManagementService = fileManagementService;
            _logService = logService;
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

        public async Task Save(Project p)
        {
            if (_faceData.ContainsKey(p.Id))
            {
                var file = await _fileManagementService.GetSubFile(p, ProjectFolderTypes.Data, Constants.Cache.FaceData);

                var ser = JsonConvert.SerializeObject(_faceData[p.Id]);

                await _fileRepo.Write(file, ser);
            }
        }



        public async Task SetFaceData(FaceData f, bool save = true)
        {
            await _semaphore.WaitAsync();
            var list = await _init(f.Project);

            var current = list.FirstOrDefault(_ => _.Hash == f.Hash);
            if (current != null)
            {
                list.Remove(current);
            }

            list.Add(f);
            if (save)
            {
                await Save(f.Project);
            }

            _semaphore.Release();
        }

        bool _hasFileHash(Project p, string hash)
        {
            if (!_fileHash.ContainsKey(p.Id))
            {
                _fileHash[p.Id] = new ConcurrentBag<string>();
            }

            var l = _fileHash[p.Id];
            if (l.Contains(hash))
            {
                return true;
            }

            l.Add(hash);
            return false;
        }

        public async Task<List<FaceData>> GetFaceData(Project p)
        {
            _fileHash.Clear();
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
                tasks.Enqueue(() => LoaderOptimization(fClose));
            }

            await tasks.Parallel(20);

            var returnList = result.OrderBy(_ => _.DateTaken).ToList();

            await Save(p);

            return returnList;
        }



        public async Task<FaceData> GetFaceData(Project p, string fileName)
        {
            //_logService.Log($"Reading: {fileName}", false);
            var list = await _init(p);

            var hash = await _fileRepo.GetHash(fileName);

            if (_hasFileHash(p, fileName))
            {
                return null;
            }
            await _semaphore.WaitAsync();
            var existing = list.FirstOrDefault(_ => _.Hash == hash);
            _semaphore.Release();
            if (existing != null)
            {
                if (existing.DateTaken == DateTime.MinValue)
                {
                    existing.DateTaken = _getExifDateTime(fileName);
                }

                existing.Project = p;
                existing.FileName = fileName;
                await SetFaceData(existing, false);
                return existing;
            }

            var faceData = new FaceData
            {
                FileName = fileName,
                Project = p,
                Hash = hash,
                DateTaken = _getExifDateTime(fileName)
            };

            await SetFaceData(faceData, false);

            return faceData;
        }

        private static Regex r = new Regex(":");

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

            //Thanks http://stackoverflow.com/questions/180030/how-can-i-find-out-when-a-picture-was-actually-taken-in-c-sharp-running-on-vista
            try
            {

                var f = new FileInfo(fileName);
                if (f.Exists)
                {
                    var d = f.LastWriteTime;
                   // Debug.WriteLine($"\r\n{d} - {fileName}");
                    return d;
                }

                //using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                //using (var myImage = Image.FromStream(fs, false, false))
                //{

                //    if (myImage.PropertyIdList.Contains(36867))
                //    {
                //        var propItem = myImage.GetPropertyItem(36867);
                //        if (propItem == null)
                //        {
                //            return DateTime.MaxValue;
                //        }
                //        var dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                //        Debug.WriteLine($"{dateTaken} - {fileName}");
                //        return DateTime.Parse(dateTaken);
                //    }

                //}


            }
            catch { }
            return DateTime.MaxValue;
        }


        async Task<string> _getKey(string fileName)
        {
            return await _fileRepo.GetHash(fileName);
        }
    }
}
