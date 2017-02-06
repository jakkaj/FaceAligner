using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Interfaces;
using XamlingCore.NET.Implementations;

namespace SmartFaceAligner.Processor.Services
{
    public class FileCacheService : IFileCacheService
    {
        private readonly IFileRepo _fileRepo;

        public FileCacheService(IFileRepo fileRepo)
        {
            _fileRepo = fileRepo;
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
            var fCacheFile = await _getFileName(originalFile, type);

            if (await _fileRepo.FileExists(fCacheFile))
            {
                return await _fileRepo.ReadBytes(fCacheFile);
            }
            return null;
        }

        public async Task<string> SaveCache(string originalFile, string type, byte[] data)
        {
            var fCacheFile = await _getFileName(originalFile, type);

            await _fileRepo.Write(fCacheFile, data);

            return fCacheFile;
        }
    }
}
