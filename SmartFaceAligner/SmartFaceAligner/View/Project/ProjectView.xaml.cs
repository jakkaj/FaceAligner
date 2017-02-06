using SmartFaceAligner.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XamlingCore.Portable.Messages.XamlingMessenger;

namespace SmartFaceAligner.View.Project
{
    /// <summary>
    /// Interaction logic for ProjectView.xaml
    /// </summary>
    public partial class ProjectView : Page
    {
        public ProjectView()
        {
            InitializeComponent();

            ImageList.Loaded += ImageList_Loaded;
        }

        private void ImageList_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = FindSimpleVisualChild<ScrollViewer>(ImageList);
            if (scrollViewer != null)
            {
                ScrollBar scrollBar = scrollViewer.Template.FindName("PART_HorizontalScrollBar", scrollViewer) as ScrollBar;
                if (scrollBar != null)
                {
                    scrollBar.ValueChanged += delegate
                    {
                        //VerticalOffset and ViweportHeight is actually what you want if UI virtualization is turned on.
                        Console.WriteLine("Visible Item Start Index:{0}", scrollViewer.HorizontalOffset);
                        Console.WriteLine("Visible Item Count:{0}", scrollViewer.ViewportWidth);

                        new ViewItemMessage(scrollViewer.HorizontalOffset, scrollViewer.ViewportWidth).Send();
                    };
                }
            }
        }

        T FindSimpleVisualChild<T>(DependencyObject element) where T : class
        {
            while (element != null)
            {

                if (element is T)
                    return element as T;

                var c = VisualTreeHelper.GetChildrenCount(element);
                if (c == 0)
                {
                    return null;
                }
                element = VisualTreeHelper.GetChild(element, 0);
            }

            return null;
        }
    }
}
