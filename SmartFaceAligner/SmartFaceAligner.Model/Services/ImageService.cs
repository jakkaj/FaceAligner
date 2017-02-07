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
using StartFaceAligner.FaceSmarts;
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

        public byte[] GetImageFile(string fileName)
        {
            try
            {
                var image = Image.FromFile(fileName);

                var flipper = ImageTools.GetExifOrientationData(image);

                if (flipper != RotateFlipType.Rotate180FlipNone)
                {
                    image.RotateFlip(flipper);
                }

                if (image.Width > 1280)
                {
                    image = ImageTools.ResizeImage(image, 1280, 1024);
                }

               
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Jpeg);
                    return ms.ToArray();
                }
            }
            catch
            {

            }

            return null;
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

            try
            {

                var image = Image.FromFile(fileName);

                var flipper = ImageTools.GetExifOrientationData(image);

                var thum = image.GetThumbnailImage(100, 100, () => false, IntPtr.Zero);

                if (flipper != RotateFlipType.Rotate180FlipNone)
                {
                    thum.RotateFlip(flipper);
                    //image.RotateFlip(flipper);
                    //image.RemovePropertyItem(0x0112);
                    //image.Save(fileName +"_flipped_.jpg");
                }


                using (var ms = new MemoryStream())
                {
                    thum.Save(ms, ImageFormat.Jpeg);
                    var b = ms.ToArray();
                    return await _cacheService.SaveCache(fileName, Constants.Cache.Thumbnail, b);
                }
            }
            catch
            {
                
            }

            return "";


        }
    }
}
