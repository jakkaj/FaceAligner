using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Interfaces;
using XamlingCore.NET.Implementations;

namespace SmartFaceAligner.Processor.Services
{
    public class FileCacheService : IFileCacheService
    {
        private readonly IFileRepo _fileRepo;

        private readonly SemaphoreSlim _semaphore;

        public FileCacheService(IFileRepo fileRepo)
        {
            _fileRepo = fileRepo;
             _semaphore = new SemaphoreSlim(1, 1);
        }

        async Task<string> _getFileName(string originalFile, string type)
        {
            var hash = HashHelper.CreateSHA1(originalFile);

            var fCacheFile = await _fileRepo.GetLocalStoragePath(hash + type);

            return fCacheFile;
        }

        public async Task<string> GetCacheFileName(string originalFile, string type)
        {
            var fCacheFile = await _getFileName(originalFile, type);

            return fCacheFile;
        }

        public async Task<bool> ExistsInCache(string originalFile, string type)
        {
            var fCacheFile = await _getFileName(originalFile, type);

            return await _fileRepo.FileExists(fCacheFile);
        }

        public async Task<byte[]> ReadCache(string originalFile, string type)
        {
            await _semaphore.WaitAsync();
            var fCacheFile = await _getFileName(originalFile, type);

            if (await _fileRepo.FileExists(fCacheFile))
            {
                var data = await _fileRepo.ReadBytes(fCacheFile);
                _semaphore.Release();
                return data;
            }

            return null;
        }

        public async Task<string> SaveCache(string originalFile, string type, byte[] data)
        {
            await _semaphore.WaitAsync();
            var fCacheFile = await _getFileName(originalFile, type);

            await _fileRepo.Write(fCacheFile, data);
            _semaphore.Release();
            return fCacheFile;
        }
    }
}
