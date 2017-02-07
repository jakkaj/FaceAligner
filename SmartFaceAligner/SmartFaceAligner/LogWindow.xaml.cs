using Contracts.Interfaces;
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
using System.Windows.Shapes;

namespace SmartFaceAligner
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        public ILogService LogService { get; set; }
        public LogWindow()
        {
            InitializeComponent();

            this.Loaded += LogWindow_Loaded;
        }

        private void LogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LogService.Callback = _callback;
        }

        void _callback(string text)
        {
            Dispatcher.Invoke(() =>
            {
                LogText.Text += "\r\n" + text;
                LogText.ScrollToEnd();
            });

        }
    }
}
