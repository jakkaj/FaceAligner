using System;
using System.Collections.Generic;
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

namespace SmartFaceAligner.View.Face
{
    public class FaceItemViewModel : ViewModel
    {
        private readonly IImageService _imageService;
        private bool? _hasFace = null;
        private string _fileName;
        private string _thumbnail;
        public FaceData FaceData { get; set; }


        public FaceItemViewModel(IImageService imageService)
        {
            _imageService = imageService;
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
