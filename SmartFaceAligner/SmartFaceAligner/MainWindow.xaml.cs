using Autofac;
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
using System.Windows.Threading;
using SmartFaceAligner.Processor.Glue;
using SmartFaceAligner.Util;
using SmartFaceAligner.View;
using SmartFaceAligner.View.Setup;
using XamlingCore.Portable.Data.Glue;
using XCoreLite.Contract;

namespace SmartFaceAligner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ILifetimeScope _container;

        public MainWindow()
        {
            InitializeComponent();
          
            new ProjectGlue(_mainFrame.NavigationService).Init();

            _container = ContainerHost.Container;
            this.Loaded += MainWindow_Loaded;
            XDispatcher.Dispatcher = Dispatcher;
        }



        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _container.Resolve<IXNavigator>().NavigateTo<HomeViewModel>();
        }

        private async void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            await _container.Resolve<IXNavigator>().NavigateTo<SetupViewModel>();
        }
    }
}
