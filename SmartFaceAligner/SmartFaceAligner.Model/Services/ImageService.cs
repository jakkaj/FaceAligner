using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10, 10);
        static readonly SemaphoreSlim _bigImageSemaphore = new SemaphoreSlim(1, 1);

        public ImageService(IFileRepo fileRepo, IFileCacheService cacheService)
        {
            _fileRepo = fileRepo;
            _cacheService = cacheService;
        }

        public byte[] GetImageFileBytes(string fileName, bool doResize, CancellationToken token = default(CancellationToken))
        {
            var image = GetImageFile(fileName, doResize, token).Item1;

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

        public (Image, double) GetImageFile(string fileName, bool doResize, CancellationToken token = default(CancellationToken))
        {
            try
            {
                try
                {
                    _bigImageSemaphore.Wait(token);
                }
                catch (OperationCanceledException ex)
                {
                    _bigImageSemaphore.Release();
                    return default(ValueTuple<Image, double>);
                }

                if (token.IsCancellationRequested)
                {
                    return default(ValueTuple<Image, double>);
                }

                var image = Image.FromFile(fileName);

                var flipper = ImageTools.GetExifOrientationData(image);

                if (flipper != RotateFlipType.RotateNoneFlipNone)
                {
                    image.RotateFlip(flipper);
                }
                if (!doResize)
                {
                    return (image, 1);
                }

                var result = ImageTools.ResizeImage(image, resize);

                return result;
            }
            catch(Exception ex)
            {
                
            }
            finally
            {
                _bigImageSemaphore.Release();
            }

            return (null, 1);
        }

        public async Task<string> GetThumbFile(string fileName, CancellationToken token)
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
                await _semaphore.WaitAsync(token);
            }
            catch (OperationCanceledException ex)
            {
                _semaphore.Release();
                return null;
            }

            if (token.IsCancellationRequested)
            {
                return null;
            }

            try
            {

                using (var image = Image.FromFile(fileName))
                {
                    var flipper = ImageTools.GetExifOrientationData(image);

                    using (var thum = image.GetThumbnailImage(100, 100, () => false, IntPtr.Zero))
                    {
                        if (flipper != RotateFlipType.RotateNoneFlipNone)
                        {
                            thum.RotateFlip(flipper);
                        }

                        using (var ms = new MemoryStream())
                        {
                            thum.Save(ms, ImageFormat.Jpeg);
                            var b = ms.ToArray();
                            if (token.IsCancellationRequested)
                            {
                                return null;
                            }
                            return await _cacheService.SaveCache(fileName, Constants.Cache.Thumbnail, b);
                        }
                    }


                     
                }
            }
            catch
            {

            }
            finally
            {
                _semaphore.Release();
            }

            return "";


        }
    }
}
