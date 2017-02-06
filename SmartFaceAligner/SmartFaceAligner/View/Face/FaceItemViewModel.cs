using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartFaceAligner.FaceSmarts;
using XCoreLite.View;

namespace SmartFaceAligner.View.Face
{
    public class FaceItemViewModel : ViewModel
    {
        private bool? _hasFace = null;

        public string FileName { get; set; }

        public bool? HasFace
        {
            get { return _hasFace; }
            set
            {
                _hasFace = value;
                OnPropertyChanged();
            }
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
                result = LocalFaceDetector.HasFace(FileName);
            }).ConfigureAwait(true);

            HasFace = result;
        }
    }
}
