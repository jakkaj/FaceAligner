using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmartFaceAligner.View.Face
{
    /// <summary>
    /// Interaction logic for FaceView.xaml
    /// </summary>
    public partial class FaceView : UserControl
    {
        public FaceView()
        {
            InitializeComponent();
            this.DataContextChanged += FaceView_DataContextChanged;
            this.SizeChanged += FaceView_SizeChanged;
        }

        private void FaceView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is FaceItemViewModel vm)
            {
                vm.UpdateDots(false);
            }
        }

        private void FaceView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is FaceItemViewModel vm)
            {
                vm.CalculateUIScale = CalculateImageScale;
            }
        }

        public class UIScaleHelper
        {
            public double Left { get; set; }
            public double Top { get; set; }
            public double ScaleX { get; set; }
            public double ScaleY { get; set; }
        }


        public UIScaleHelper CalculateImageScale(double imageWidth, double imageHeight)
        {
            var uiWidth = MainGrid.ActualWidth;
            var uiHeight = MainGrid.ActualHeight;


            var windowAspect = uiWidth / uiHeight;

            var imageAspect = imageWidth / imageHeight;

            UIScaleHelper result;

            if (imageAspect > windowAspect)
            {
                result = _hw(imageHeight, imageWidth, uiHeight, uiWidth);
            }
            else
            {
                result = _wh(imageHeight, imageWidth, uiHeight, uiWidth);
            }

            return result;
        }

        /// <summary>
            /// This was really hard to figure out :P 
            /// </summary>
            /// <param name="imageHeight"></param>
            /// <param name="imageWidth"></param>
            /// <param name="uiHeight"></param>
            /// <param name="uiWidth"></param>
            /// <returns></returns>
            public UIScaleHelper _wh(double imageHeight, double imageWidth, double uiHeight, double uiWidth)
        {
            var aspect = imageHeight / imageWidth;

            var actualHeight = uiHeight;
            var actualWidth = actualHeight / aspect;

            var center = uiWidth / 2;

            var left = center - (actualWidth / 2);
            var top = 0;

            var scaley = actualHeight / imageHeight;
            var scalex = actualWidth / imageWidth;

            return new UIScaleHelper
            {
                Left = left,
                Top = top,
                ScaleX = scalex,
                ScaleY = scaley
            };
        }

        public UIScaleHelper _hw(double imageHeight, double imageWidth, double uiHeight, double uiWidth)
        {
            var aspect = imageWidth / imageHeight;
            var actualWidth = uiWidth;
            var actualHeight = actualWidth / aspect;

            var center = uiHeight / 2;
            var left = 0;
            var top = center - (actualHeight / 2);

            var scaley = actualHeight / imageHeight;
            var scalex = actualWidth / imageWidth;

            return new UIScaleHelper
            {
                Left = left,
                Top = top,
                ScaleX = scalex,
                ScaleY = scaley
            };
        }
    }
}
