using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCoreLite.View;

namespace SmartFaceAligner.UtilVm
{
    public class FaceFilterViewModel : ViewModel
    {
        private bool _goggles;
        private bool _noGlasses;
        private bool _sunglasses;
        private bool _readingGlasses;
        private bool _males;
        private bool _females;
        private bool _notSmiling;
        private bool _smiling;
        private bool _faces;

        public bool Goggles
        {
            get { return _goggles; }
            set
            {
                _goggles = value;
                OnPropertyChanged();
            }
        }

        public bool Faces
        {
            get { return _faces; }
            set
            {
                _faces = value;
                OnPropertyChanged();
            }
        }

        public bool Smiling
        {
            get { return _smiling; }
            set
            {
                _smiling = value;
                OnPropertyChanged();
            }
        }

        public bool NotSmiling
        {
            get { return _notSmiling; }
            set
            {
                _notSmiling = value;
                OnPropertyChanged();
            }
        }

        public bool Females
        {
            get { return _females; }
            set
            {
                _females = value;
                OnPropertyChanged();
            }
        }

        public bool Males
        {
            get { return _males; }
            set
            {
                _males = value;
                OnPropertyChanged();
            }
        }

        public bool ReadingGlasses
        {
            get { return _readingGlasses; }
            set
            {
                _readingGlasses = value;
                OnPropertyChanged();
            }
        }

        public bool Sunglasses
        {
            get { return _sunglasses; }
            set
            {
                _sunglasses = value;
                OnPropertyChanged();
            }
        }

        public bool NoGlasses
        {
            get { return _noGlasses; }
            set
            {
                _noGlasses = value;
                OnPropertyChanged();
            }
        }
    }
}
