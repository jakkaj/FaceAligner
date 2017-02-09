using System;
using System.Collections.Generic;
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
using SmartFaceAligner.Util;

namespace SmartFaceAligner.View.Face
{
    /// <summary>
    /// Interaction logic for ThumbView.xaml
    /// </summary>
    public partial class ThumbView : UserControl
    {
        public ThumbView()
        {
            InitializeComponent();
            this.DataContextChanged += ThumbView_DataContextChanged;
        }

        private void ThumbView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is IDisposable du)
            {
                du.Dispose();
            }
            
        }
    }
}
