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
using Autofac;

namespace SmartFaceAligner.View.Face
{
    public class FaceItemViewModel : ViewModel
    {
        private readonly IImageService _imageService;
        private bool? _hasFace = null;
        private string _fileName;
        private string _thumbnail;
        public FaceData FaceData { get; set; }

        
        public Func<double, double, FaceView.UIScaleHelper> CalculateUIScale { get; set; }

        private FaceView.UIScaleHelper _scaleHelper = null;

        public ObservableCollection<FaceDotViewModel> FaceDots { get; private set; }

        public BitmapImage BitmapSource
        {
            get { return _getBitmap(); }
        }


        public FaceItemViewModel(IImageService imageService)
        {
            _imageService = imageService;
            FaceDots = new ObservableCollection<FaceDotViewModel>();
          
        }

        BitmapImage _getBitmap()
        {
            if (FaceData?.FileName == null)
            {
                return null;
            }

            var img = _imageService.GetImageFile(FaceData.FileName);
            if (img == null)
            {
                return null;
            }

            if (CalculateUIScale != null)
            {
                _scaleHelper = CalculateUIScale(img.Width, img.Height);
                _updateDots();
            }

            using (var msOuter = new MemoryStream())
            {
                img.Save(msOuter, ImageFormat.Jpeg);

                using (var ms = new MemoryStream(msOuter.ToArray()))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
        }

        void _updateDots()
        {
            FaceDots.Clear();

            if (_scaleHelper == null)
            {
                return;
            }

            if (FaceData?.ParsedFaces == null || FaceData.ParsedFaces.Length == 0)
            {
                return;
            }

            foreach (var faceData in FaceData.ParsedFaces)
            {
                var detail = faceData.Face.FaceLandmarks;

                var points = FaceHomography.GetPoints(new ImageAligner.ProcessFaceData{ParsedFace = faceData.Face});

              

                foreach (var p in points.ToList())
                {
                    var x = _scaleHelper.Left + (p.X * _scaleHelper.ScaleX);
                    var y = _scaleHelper.Top + (p.Y * _scaleHelper.ScaleY);

                    var pointVm = Scope.Resolve<FaceDotViewModel>();
                    pointVm.X = x;
                    pointVm.Y = y;
                    FaceDots.Add(pointVm);

                }

                
            }
        }

        public string Thumbnail
        {
            get
            {
                if (_thumbnail == null && FaceData.FileName != null)
                {
                    _loadImage();
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

        async void _loadImage()
        {
            

            await Task.Yield();
            var thumb = "";

            await Task.Run(async () =>
            {
                thumb = await _imageService.GetThumbFile(FaceData.FileName);
            }).ConfigureAwait(true);

            Thumbnail = thumb;
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
    }
}
