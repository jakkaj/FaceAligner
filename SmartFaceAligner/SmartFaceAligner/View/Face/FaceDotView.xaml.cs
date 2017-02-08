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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmartFaceAligner.View.Face
{
    /// <summary>
    /// Interaction logic for FaceDotView.xaml
    /// </summary>
    public partial class FaceDotView : UserControl
    {
        public FaceDotView()
        {
            InitializeComponent();
            this.Loaded += FaceDotView_Loaded;
        }

        private void FaceDotView_Loaded(object sender, RoutedEventArgs e)
        {
            grid.Visibility = Visibility.Visible;
            if (DataContext is FaceDotViewModel vm)
            {
                if (vm.DoAnimation)
                {
                    Storyboard sb = this.FindResource("AnimateIn") as Storyboard;
                    //Storyboard.SetTarget(sb, this.btn);
                    sb.Begin();
                }
            }
        }
    }
}
