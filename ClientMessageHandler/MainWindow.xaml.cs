using System.ComponentModel;
using System.Windows;
using ClientMessageHandler.API;

namespace ClientMessageHandler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {   
        static LogWindow logWindow = LogWindow.Instance;
        
        public MainWindow()
        {
            InitializeComponent();
            logWindow.Hide();
            Logger.LogLevel = "Debug";
        }
        
        private void LogButtonHandler(object sender, RoutedEventArgs ev)
        {
            if (logWindow.IsVisible)
            {
                logWindow.Hide();
            }
            else
            {
                logWindow.Show();
            }
        }
        
        protected override void OnClosing(CancelEventArgs ev)
        {
            base.OnClosing(ev);
            Application.Current.Shutdown();
        }
        
        private void CompareHandler(object sender, RoutedEventArgs ev)
        {
            if (Compare.Instance.IsVisible)
            {
                Compare.Instance.Hide();
            }
            else
            {
                Compare.Instance.Show();
            }
        }
        
        private async void FileButtonHandler(object sender, RoutedEventArgs ev)
        {
            API.API.LoadXmlFile();

            await DataWindow.Instance.PopulateFileListAsync();
        }
    }
}