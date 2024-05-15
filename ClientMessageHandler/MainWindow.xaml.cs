using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using ClientMessageHandler.Generic;

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
            try
            {
                string? path = Generic.API.LoadXmlFile()!;
                if (!File.Exists(path))
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message + e.StackTrace);
                throw;
            }

            await DataWindow.Instance.PopulateFileListAsync();
        }
    }
}