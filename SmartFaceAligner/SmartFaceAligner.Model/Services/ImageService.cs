﻿using System;
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

        private const int resize = 1280;

        public ImageService(IFileRepo fileRepo, IFileCacheService cacheService)
        {
            _fileRepo = fileRepo;
            _cacheService = cacheService;
        }

        public byte[] GetImageFileBytes(string fileName)
        {
            var image = GetImageFile(fileName).Item1;

            if (image == null)
            {
                return null;
            }

            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        public (Image, double) GetImageFile(string fileName)
        {
            try
            {
                var image = Image.FromFile(fileName);

                var flipper = ImageTools.GetExifOrientationData(image);

                if (flipper != RotateFlipType.RotateNoneFlipNone)
                {
                    image.RotateFlip(flipper);
                }

                double scale = 1;

                if (image.Width > resize || image.Height > resize)
                {
                    var imageHeight = image.Height;
                    var imageWidth = image.Width;

                    if (image.Height > image.Width)
                    {
                        var aspect = Convert.ToDouble(image.Height) / Convert.ToDouble(image.Width);

                        image = ImageTools.ResizeImage(image, Convert.ToInt32(resize / aspect), resize);
                        scale = Convert.ToDouble(imageHeight) / Convert.ToDouble(resize) ;
                    }
                    else
                    {
                        var aspect = Convert.ToDouble(image.Width) / Convert.ToDouble(image.Height);
                        image = ImageTools.ResizeImage(image, resize, Convert.ToInt32(resize / aspect));
                        scale = Convert.ToDouble(imageWidth) / Convert.ToDouble(resize);
                    }
                }

                return (image, scale);

            }
            catch
            {

            }

            return (null, 1);
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


                if (flipper != RotateFlipType.RotateNoneFlipNone)
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
