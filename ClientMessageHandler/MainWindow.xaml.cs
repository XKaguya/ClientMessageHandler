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
        
        static DataWindow dataWindow = DataWindow.Instance;
        
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
        
        private void DataButtonHandler(object sender, RoutedEventArgs ev)
        {
            if (dataWindow.IsVisible)
            {
                dataWindow.Hide();
            }
            else
            {
                dataWindow.Show();
            }
        }
        
        private void FileButtonHandler(object sender, RoutedEventArgs ev)
        {
            API.API.LoadXmlFile();
            
            dataWindow.SetData(API.API.Messages);
            dataWindow.Show();
        }
    }
}