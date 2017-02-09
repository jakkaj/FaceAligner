using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Contracts.Entity;
using Contracts.Interfaces;
using StartFaceAligner.FaceSmarts;
using XCoreLite.View;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Threading;
using Autofac;
using System.Windows.Input;
using SmartFaceAligner.Util;

namespace SmartFaceAligner.View.Face
{
    public class FaceItemViewModel : ViewModel, IDisposable
    {
        private readonly IImageService _imageService;
        private bool? _hasFace = null;
        private string _fileName;
        private string _thumbnail;
        public FaceData FaceData { get; set; }

        
        public Func<double, double, FaceView.UIScaleHelper> CalculateUIScale { get; set; }

        private FaceView.UIScaleHelper _scaleHelper = null;

        public ObservableCollection<FaceDotViewModel> FaceDots { get; private set; }

        public ICommand DoFaceAnimation => Command(() => UpdateDots(true));

        private bool _hasDoneAnimation = false;

        private BitmapImage _cacheBm;

        private CancellationTokenSource _thumbToken;
        private CancellationTokenSource _bitmapToken;

        static object _lock = new object();

        public BitmapImage BitmapSource
        {
            get { return _getBitmap(); }
        }

        private (System.Drawing.Image, double) _image;


        public FaceItemViewModel(IImageService imageService)
        {
            _imageService = imageService;
            FaceDots = new ObservableCollection<FaceDotViewModel>();
            
          
        }

        BitmapImage _getBitmap()
        {
            lock (_lock)
            {

                if (FaceData?.FileName == null)
                {
                    return null;
                }

                if (_cacheBm != null)
                {
                    return _cacheBm;
                }

                _bitmapToken = new CancellationTokenSource();
                var img = _imageService.GetImageFile(FaceData.FileName,true, _bitmapToken.Token);
                
                if (img.Item1 == null)
                {
                    return null;
                }

                if (CalculateUIScale != null)
                {
                    _image = img;

                    UpdateDots(false);
                }

                using (var msOuter = new MemoryStream())
                {
                    img.Item1.Save(msOuter, ImageFormat.Jpeg);

                    using (var ms = new MemoryStream(msOuter.ToArray()))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        _cacheBm = bitmap;
                        _bitmapToken = null;
                        return bitmap;
                    }
                }
            }
        }

        async void _getThumbnail()
        {
            await Task.Yield();
            var thumb = "";

            _thumbToken = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                thumb = await _imageService.GetThumbFile(FaceData.FileName, _thumbToken.Token);
                _thumbToken = null;
            }).ConfigureAwait(true);

            Thumbnail = thumb;
        }

        public async void UpdateDots(bool doAnimation)
        {
            if (!_hasDoneAnimation && !doAnimation)
            {
                return;
            }

            FaceDots.Clear();

            if (CalculateUIScale == null || _image.Item1 == null)
            {
                return;
            }

            _scaleHelper = CalculateUIScale(_image.Item1.Width, _image.Item1.Height);

            if (FaceData?.ParsedFaces == null || FaceData.ParsedFaces.Length == 0)
            {
                return;
            }

            foreach (var faceData in FaceData.ParsedFaces)
            {
                var points = FaceHomography.GetPointsFeatures(new ImageAligner.ProcessFaceData{ParsedFace = faceData.Face});

                foreach (var p in points.ToList())
                {
                    var x = _scaleHelper.Left + (((p.X / _image.Item2))  * _scaleHelper.ScaleX);
                    var y = _scaleHelper.Top + (((p.Y / _image.Item2)) * _scaleHelper.ScaleY);

                    var pointVm = Scope.Resolve<FaceDotViewModel>();
                    pointVm.DoAnimation = doAnimation;
                    pointVm.X = x;
                    pointVm.Y = y;

                    if (!_hasDoneAnimation)
                    {
                        await Task.Delay(30);
                    }

                    FaceDots.Add(pointVm);
                }

            }

            _hasDoneAnimation = true;
        }

      
       

        public async Task CheckHasFace()
        {
            if (_hasFace.HasValue)
            {
                return;
            }

            var result = false;

            await Task.Run(() =>
            {
                result = LocalFaceDetector.HasFace(FaceData.FileName);
            }).ConfigureAwait(true);

            HasFace = result;
        }

        public override void Dispose()
        {
            _thumbToken?.Cancel();
            _bitmapToken?.Cancel();
            _image.Item1?.Dispose();
            _image.Item1 = null;
            base.Dispose();
        }


        public string Thumbnail
        {
            get
            {
                if (_thumbnail == null && FaceData.FileName != null)
                {
                    _getThumbnail();
                    return null;
                }
                return _thumbnail;
            }
            set
            {
                _thumbnail = value;
                OnPropertyChanged();
            }
        }


        public bool? HasFace
        {
            get { return _hasFace; }
            set
            {
                _hasFace = value;
                OnPropertyChanged();
            }
        }

    }
}
