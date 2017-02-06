using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Interfaces;
using ImageMagick;
using SmartFaceAligner.Processor.Entity;
using XamlingCore.NET.Implementations;

namespace SmartFaceAligner.Processor.Services
{
    public class ImageService : IImageService
    {
        private readonly IFileRepo _fileRepo;
        private readonly IFileCacheService _cacheService;

        public ImageService(IFileRepo fileRepo, IFileCacheService cacheService)
        {
            _fileRepo = fileRepo;
            _cacheService = cacheService;
        }

        public async Task<string> GetThumbFile(string fileName)
        {
            if (!await _fileRepo.FileExists(fileName))
            {
                return null;
            }
            
            var isCached = await _cacheService.ExistsInCache(fileName, Constants.Cache.Thumbnail);

            if (isCached)
            {
                return await _cacheService.GetCacheFileName(fileName, Constants.Cache.Thumbnail);
            }

            var image = Image.FromFile(fileName);
            var thum = image.GetThumbnailImage(100, 100, () => false, IntPtr.Zero);
            
            using (var ms = new MemoryStream())
            {
                thum.Save(ms, ImageFormat.Jpeg);
                var b = ms.ToArray();
                return await _cacheService.SaveCache(fileName, Constants.Cache.Thumbnail, b);
            }

        }
    }
}
